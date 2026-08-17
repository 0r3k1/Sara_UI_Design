using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using WinFormAnimation;

namespace Sara_UI_Design.SaraControls {

    [ToolboxItem(true)]
    [DefaultBindingProperty("Value")]
    /// <summary>
    /// Representa un control de barra de progreso circular con soporte para animaciones fluidas, 
    /// personalización de anillos concéntricos y terminaciones redondeadas modernas.
    /// </summary>
    public class SaraUI_CircularProgressBar:ProgressBar {
        // Campos de animación y control interno
        private readonly Animator _animator;
        private int? _animatedStartAngle;
        private float? _animatedValue;
        private AnimationFunctions.Function _animationFunction;
        private KnownAnimationFunctions _knownAnimationFunction;
        private ProgressBarStyle? _lastStyle;
        private int _lastValue;

        /// <summary>
        /// Obtiene o establece la función de interpolación (Easing) que define el comportamiento del movimiento de la barra.
        /// </summary>
        [Category("Sara UI Design")]
        public KnownAnimationFunctions AnimationFunction {
            get => _knownAnimationFunction;
            set {
                _animationFunction = AnimationFunctions.FromKnown(value);
                _knownAnimationFunction = value;
            }
        }

        /// <summary>
        /// Obtiene o establece la velocidad de la animación en milisegundos.
        /// </summary>
        [Category("Sara UI Design")]
        public int AnimationSpeed { get; set; }

        /// <summary>
        /// Obtiene o establece el color del círculo central interno. Use Color.Transparent para un diseño de anillo puro.
        /// </summary>
        [Category("Sara UI Design")]
        public Color InnerColor { get; set; } = Color.FromArgb(245, 245, 250);

        [Category("Sara UI Design")]
        public int InnerMargin { get; set; } = 2;

        /// <summary>
        /// Obtiene o establece el color del anillo exterior que sirve de fondo estático para el progreso.
        /// </summary>
        [Category("Sara UI Design")]
        public Color OuterColor { get; set; } = Color.FromArgb(230, 230, 240);

        [Category("Sara UI Design")]
        public int OuterWidth { get; set; } = 12;

        [Category("Sara UI Design")]
        public Color ProgressColor { get; set; } = Color.MediumSlateBlue;

        /// <summary>
        /// Obtiene o establece el grosor del arco que representa el progreso actual.
        /// </summary>
        [Category("Sara UI Design")]
        public int ProgressWidth { get; set; } = 12;

        [Category("Sara UI Design")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override Font Font {
            get => base.Font;
            set { base.Font = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece la fuente utilizada específicamente para el subíndice y el superíndice.
        /// </summary>
        [Category("Sara UI Design")]
        public Font SecondaryFont { get; set; }

        /// <summary>
        /// Obtiene o establece el ángulo inicial (en grados) donde comienza a dibujarse el progreso. 
        /// Por defecto es 270 (parte superior central).
        /// </summary>
        [Category("Sara UI Design")]
        public int StartAngle { get; set; } = 270;

        [Category("Sara UI Design")]
        public Color SubscriptColor { get; set; } = Color.FromArgb(166, 166, 166);

        [Category("Sara UI Design")]
        public Padding SubscriptMargin { get; set; } = new Padding(10, -25, 0, 0);

        /// <summary>
        /// Obtiene o establece el texto que aparece como subíndice (ej. decimales o unidades pequeñas).
        /// </summary>
        [Category("Sara UI Design")]
        public string SubscriptText { get; set; } = ".00";

        [Category("Sara UI Design")]
        public Color SuperscriptColor { get; set; } = Color.FromArgb(166, 166, 166);

        [Category("Sara UI Design")]
        public Padding SuperscriptMargin { get; set; } = new Padding(10, 25, 0, 0);

        /// <summary>
        /// Obtiene o establece el texto que aparece como superíndice (ej. símbolos de grado o unidades).
        /// </summary>
        [Category("Sara UI Design")]
        public string SuperscriptText { get; set; } = "%";

        [Category("Sara UI Design")]
        public Padding TextMargin { get; set; } = new Padding(0, 0, 0, 0);

        [Category("Sara UI Design")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override string Text {
            get => base.Text;
            set { base.Text = value; this.Invalidate(); }
        }

        public SaraUI_CircularProgressBar() {
            SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            _animator = DesignMode ? null : new Animator();
            AnimationFunction = KnownAnimationFunctions.Liner;
            AnimationSpeed = 500;

            this.BackColor = Color.Transparent;
            this.ForeColor = Color.FromArgb(64, 64, 64);
            this.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            this.SecondaryFont = new Font("Segoe UI", 12, FontStyle.Regular);
            this.Size = new Size(150, 150);
        }

        protected override void OnPaint(PaintEventArgs e) {
            if(!DesignMode) {
                if(Style == ProgressBarStyle.Marquee)
                    InitializeMarquee(_lastStyle != Style);
                else
                    InitializeContinues(_lastStyle != Style);
                _lastStyle = Style;
            }

            StartPaint(e.Graphics);
        }

        /// <summary>
        /// Realiza el dibujo por capas: canal estático, arco de progreso con puntas redondeadas premium y textos en eje.
        /// </summary>
        protected virtual void StartPaint(Graphics g) {
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Limpieza con transparencia real basada en el padre
            g.Clear(this.Parent?.BackColor ?? Color.White);

            // Calcular el diámetro mayor de dibujo libre de desbordamientos perimetrales
            int maxPenWidth = Math.Max(OuterWidth, ProgressWidth);
            float pad = maxPenWidth / 2f + 2;
            RectangleF rectArea = new RectangleF(pad, pad, this.Width - (pad * 2), this.Height - (pad * 2));

            // 1. Dibujar el Canal de Fondo (Anillo Estático)
            if(OuterColor != Color.Transparent && OuterWidth > 0) {
                using(Pen penOuter = new Pen(OuterColor, OuterWidth)) {
                    g.DrawEllipse(penOuter, rectArea);
                }
            }

            // 2. Dibujar el Círculo de Fondo Central (Cuerpo interno opcional)
            if(InnerColor != Color.Transparent) {
                float innerOffset = (maxPenWidth / 2f) + InnerMargin;
                RectangleF rectInner = RectangleF.Inflate(rectArea, -innerOffset, -innerOffset);
                using(SolidBrush brushInner = new SolidBrush(InnerColor)) {
                    g.FillEllipse(brushInner, rectInner);
                }
            }

            // 3. REINGENIERÍA SARA UI: Dibujar progreso como Arco con Puntas Esféricas (LineCap.Round)
            float sweepAngle = ((_animatedValue ?? Value) - Minimum) / (float)(Maximum - Minimum) * 360;
            if(sweepAngle > 0) {
                if(sweepAngle > 360)
                    sweepAngle = 360;

                using(Pen penProgress = new Pen(ProgressColor, ProgressWidth)) {
                    // Inyectamos el look redondeado en los extremos del arco de progreso
                    penProgress.StartCap = LineCap.Round;
                    penProgress.EndCap = LineCap.Round;
                    penProgress.LineJoin = LineJoin.Round;

                    g.DrawArc(penProgress, rectArea, _animatedStartAngle ?? StartAngle, sweepAngle);
                }
            }

            // 4. Renderizar Textos e Indicadores
            using(StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }) {
                RectangleF clientRect = new RectangleF(0, 0, this.Width, this.Height);

                // Texto Principal
                if(!string.IsNullOrEmpty(Text)) {
                    RectangleF textRect = new RectangleF(TextMargin.Left, TextMargin.Top, this.Width, this.Height);
                    using(SolidBrush foreBrush = new SolidBrush(this.ForeColor))
                        g.DrawString(Text, Font, foreBrush, textRect, sf);
                }

                // Superíndice (ej. %)
                if(!string.IsNullOrEmpty(SuperscriptText)) {
                    RectangleF superRect = new RectangleF(SuperscriptMargin.Left, SuperscriptMargin.Top, this.Width, this.Height);
                    using(SolidBrush superBrush = new SolidBrush(this.SuperscriptColor))
                        g.DrawString(SuperscriptText, SecondaryFont, superBrush, superRect, sf);
                }

                // Subíndice (ej. .00)
                if(!string.IsNullOrEmpty(SubscriptText)) {
                    RectangleF subRect = new RectangleF(SubscriptMargin.Left, SubscriptMargin.Top, this.Width, this.Height);
                    using(SolidBrush subBrush = new SolidBrush(this.SubscriptColor))
                        g.DrawString(SubscriptText, SecondaryFont, subBrush, subRect, sf);
                }
            }
        }

        protected virtual void InitializeContinues(bool firstTime) {
            if(_lastValue == Value && !firstTime)
                return;
            _lastValue = Value;
            _animator.Stop();
            if(AnimationSpeed <= 0) { _animatedValue = Value; Invalidate(); return; }

            _animator.Paths = new WinFormAnimation.Path(_animatedValue ?? Value, Value, (ulong)AnimationSpeed, _animationFunction).ToArray();
            _animator.Play(new SafeInvoker<float>(v => { _animatedValue = v; Invalidate(); }, this));
        }

        protected virtual void InitializeMarquee(bool firstTime) {
            if(!firstTime && _animator.ActivePath != null)
                return;
            _animator.Stop();
            _animator.Paths = new WinFormAnimation.Path(0, 359, (ulong)2000, _animationFunction).ToArray();
            _animator.Repeat = true;
            _animator.Play(new SafeInvoker<float>(v => { _animatedStartAngle = (int)v; Invalidate(); }, this));
        }
    }
}
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
    /// personalización de anillos concéntricos y textos decorativos (subíndices/superíndices).
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
        private Brush _backBrush;

        // --- Propiedades agrupadas en Sara UI Design ---

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
        /// Obtiene o establece el color del círculo central interno.
        /// </summary>
        [Category("Sara UI Design")]
        public Color InnerColor { get; set; } = Color.FromArgb(224, 224, 224);

        [Category("Sara UI Design")]
        public int InnerMargin { get; set; } = 2;

        [Category("Sara UI Design")]
        public int InnerWidth { get; set; } = -1;

        /// <summary>
        /// Obtiene o establece el color del anillo exterior que sirve de fondo para el progreso.
        /// </summary>
        [Category("Sara UI Design")]
        public Color OuterColor { get; set; } = Color.Gray;

        [Category("Sara UI Design")]
        public int OuterMargin { get; set; } = -25;

        [Category("Sara UI Design")]
        public int OuterWidth { get; set; } = 26;

        [Category("Sara UI Design")]
        public Color ProgressColor { get; set; } = Color.FromArgb(255, 128, 0);

        /// <summary>
        /// Obtiene o establece el grosor del arco que representa el progreso actual.
        /// </summary>
        [Category("Sara UI Design")]
        public int ProgressWidth { get; set; } = 25;

        [Category("Sara UI Design")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override Font Font {
            get => base.Font;
            set {
                base.Font = value;
                this.Invalidate();
            }
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
        public Padding SubscriptMargin { get; set; } = new Padding(10, -35, 0, 0);


        /// <summary>
        /// Obtiene o establece el texto que aparece como subíndice (ej. decimales o unidades pequeñas).
        /// </summary>
        [Category("Sara UI Design")]
        public string SubscriptText { get; set; } = ".00";
        
        [Category("Sara UI Design")]
        public Color SuperscriptColor { get; set; } = Color.FromArgb(166, 166, 166);

        [Category("Sara UI Design")]
        public Padding SuperscriptMargin { get; set; } = new Padding(10, 35, 0, 0);

        /// <summary>
        /// Obtiene o establece el texto que aparece como superíndice (ej. símbolos de grado o unidades).
        /// </summary>
        [Category("Sara UI Design")]
        public string SuperscriptText { get; set; } = "°C";

        [Category("Sara UI Design")]
        public Padding TextMargin { get; set; } = new Padding(8, 8, 0, 0);

        [Category("Sara UI Design")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override string Text {
            get => base.Text;
            set { base.Text = value; this.Invalidate(); }
        }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_CircularProgressBar"/> con estilos 
        /// de dibujo optimizados y configuración de animación por defecto.
        /// </summary>
        public SaraUI_CircularProgressBar() {
            SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            _animator = DesignMode ? null : new Animator();
            AnimationFunction = KnownAnimationFunctions.Liner;
            AnimationSpeed = 500;

            this.BackColor = Color.Transparent;
            this.ForeColor = Color.FromArgb(64, 64, 64);
            this.Font = new Font(this.Font.FontFamily, 40, FontStyle.Bold);
            this.SecondaryFont = new Font(this.Font.FontFamily, 20, FontStyle.Regular);
            this.Size = new Size(150, 150);
        }

        // --- Métodos de Dibujo con Optimizaciones de Memoria ---

        /// <summary>
        /// Gestiona el ciclo de dibujo del control, activando las animaciones si el estilo es Marquee o Continuous.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e) {
            if(!DesignMode) {
                if(Style == ProgressBarStyle.Marquee)
                    InitializeMarquee(_lastStyle != Style);
                else
                    InitializeContinues(_lastStyle != Style);
                _lastStyle = Style;
            }

            if(_backBrush == null)
                RecreateBackgroundBrush();
            StartPaint(e.Graphics);
        }

        /// <summary>
        /// Realiza el dibujo capa por capa del control: fondo, anillo exterior, arco de progreso y los tres niveles de texto.
        /// </summary>
        /// <param name="g">Objeto Graphics sobre el cual dibujar.</param>
        protected virtual void StartPaint(Graphics g) {
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = this.ClientRectangle;
            var point = new PointF(2, 2);
            var size = new SizeF(this.Width - 4, this.Height - 4);

            // 1. Dibujar Círculo Exterior
            if(OuterColor != Color.Transparent && OuterWidth != 0) {
                using(SolidBrush b = new SolidBrush(OuterColor))
                    g.FillEllipse(b, new RectangleF(point, size));

                if(OuterWidth >= 0) {
                    var innerPoint = new PointF(point.X + OuterWidth, point.Y + OuterWidth);
                    var innerSize = new SizeF(size.Width - (2 * OuterWidth), size.Height - (2 * OuterWidth));
                    g.FillEllipse(_backBrush, new RectangleF(innerPoint, innerSize));
                }
            }

            // 2. Dibujar Progreso (Arco)
            float angle = ((_animatedValue ?? Value) - Minimum) / (float)(Maximum - Minimum) * 360;
            using(SolidBrush pb = new SolidBrush(ProgressColor)) {
                g.FillPie(pb, Rectangle.Round(new RectangleF(point.X + Math.Abs(OuterMargin), point.Y + Math.Abs(OuterMargin),
                          size.Width - (2 * Math.Abs(OuterMargin)), size.Height - (2 * Math.Abs(OuterMargin)))),
                          _animatedStartAngle ?? StartAngle, angle);
            }

            // 3. Limpiar centro del progreso (Efecto de anillo)
            if(ProgressWidth >= 0) {
                float cut = Math.Abs(OuterMargin) + ProgressWidth;
                g.FillEllipse(_backBrush, new RectangleF(point.X + cut, point.Y + cut, size.Width - (2 * cut), size.Height - (2 * cut)));
            }

            // 4. Dibujar Texto, Superíndice y Subíndice
            using(StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }) {

                // Texto Principal
                if(!string.IsNullOrEmpty(Text)) {
                    using(SolidBrush foreBrush = new SolidBrush(this.ForeColor)) {
                        // Aplicamos el TextMargin desplazando el rectángulo de dibujo
                        RectangleF textRect = new RectangleF(TextMargin.Left, TextMargin.Top, this.Width, this.Height);
                        g.DrawString(Text, Font, foreBrush, textRect, sf);
                    }
                }

                // Superíndice (ej. °C)
                if(!string.IsNullOrEmpty(SuperscriptText)) {
                    using(SolidBrush superBrush = new SolidBrush(this.SuperscriptColor)) {
                        RectangleF superRect = new RectangleF(SuperscriptMargin.Left, SuperscriptMargin.Top, this.Width, this.Height);
                        g.DrawString(SuperscriptText, SecondaryFont, superBrush, superRect, sf);
                    }
                }

                // Subíndice (ej. .00)
                if(!string.IsNullOrEmpty(SubscriptText)) {
                    using(SolidBrush subBrush = new SolidBrush(this.SubscriptColor)) {
                        RectangleF subRect = new RectangleF(SubscriptMargin.Left, SubscriptMargin.Top, this.Width, this.Height);
                        g.DrawString(SubscriptText, SecondaryFont, subBrush, subRect, sf);
                    }
                }
            }
        }

        // --- Métodos de Animación y Soporte ---

        /// <summary>
        /// Configura y arranca la animación para cambios de valor lineales.
        /// </summary>
        /// <param name="firstTime">Indica si es la primera vez que se inicializa el estilo.</param>
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
            if(!firstTime && _animator.ActivePath != null) return; //
            _animator.Stop();
            _animator.Paths = new WinFormAnimation.Path(0, 359, (ulong)2000, _animationFunction).ToArray();
            _animator.Repeat = true;
            _animator.Play(new SafeInvoker<float>(v => { _animatedStartAngle = (int)v; Invalidate(); }, this));
        }

        /// <summary>
        /// Recrea la brocha de fondo para manejar correctamente las transparencias basadas en el color del contenedor padre.
        /// </summary>
        protected virtual void RecreateBackgroundBrush() {
            _backBrush?.Dispose();
            _backBrush = new SolidBrush(this.BackColor == Color.Transparent ? (this.Parent?.BackColor ?? Color.White) : this.BackColor);
        }

        protected override void OnParentBackColorChanged(EventArgs e) { base.OnParentBackColorChanged(e); RecreateBackgroundBrush(); }
    }
}
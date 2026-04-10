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

        [Category("Sara UI Design")]
        public KnownAnimationFunctions AnimationFunction {
            get => _knownAnimationFunction;
            set {
                _animationFunction = AnimationFunctions.FromKnown(value);
                _knownAnimationFunction = value;
            }
        }

        [Category("Sara UI Design")]
        public int AnimationSpeed { get; set; }

        [Category("Sara UI Design")]
        public Color InnerColor { get; set; } = Color.FromArgb(224, 224, 224);

        [Category("Sara UI Design")]
        public int InnerMargin { get; set; } = 2;

        [Category("Sara UI Design")]
        public int InnerWidth { get; set; } = -1;

        [Category("Sara UI Design")]
        public Color OuterColor { get; set; } = Color.Gray;

        [Category("Sara UI Design")]
        public int OuterMargin { get; set; } = -25;

        [Category("Sara UI Design")]
        public int OuterWidth { get; set; } = 26;

        [Category("Sara UI Design")]
        public Color ProgressColor { get; set; } = Color.FromArgb(255, 128, 0);

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

        [Category("Sara UI Design")]
        public Font SecondaryFont { get; set; }

        [Category("Sara UI Design")]
        public int StartAngle { get; set; } = 270;

        [Category("Sara UI Design")]
        public Color SubscriptColor { get; set; } = Color.FromArgb(166, 166, 166);

        [Category("Sara UI Design")]
        public Padding SubscriptMargin { get; set; } = new Padding(10, -35, 0, 0);

        [Category("Sara UI Design")]
        public string SubscriptText { get; set; } = ".00";

        [Category("Sara UI Design")]
        public Color SuperscriptColor { get; set; } = Color.FromArgb(166, 166, 166);

        [Category("Sara UI Design")]
        public Padding SuperscriptMargin { get; set; } = new Padding(10, 35, 0, 0);

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

            // 3. Limpiar centro del progreso
            if(ProgressWidth >= 0) {
                float cut = Math.Abs(OuterMargin) + ProgressWidth;
                g.FillEllipse(_backBrush, new RectangleF(point.X + cut, point.Y + cut, size.Width - (2 * cut), size.Height - (2 * cut)));
            }

            // 4. Dibujar Texto y Símbolos
            if(!string.IsNullOrEmpty(Text)) {
                using(SolidBrush foreBrush = new SolidBrush(this.ForeColor))
                using(StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }) {
                    g.DrawString(Text, Font, foreBrush, this.ClientRectangle, sf);
                }
            }
        }

        // --- Métodos de Animación y Soporte ---

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

        protected virtual void RecreateBackgroundBrush() {
            _backBrush?.Dispose();
            _backBrush = new SolidBrush(this.BackColor == Color.Transparent ? (this.Parent?.BackColor ?? Color.White) : this.BackColor);
        }

        protected override void OnParentBackColorChanged(EventArgs e) { base.OnParentBackColorChanged(e); RecreateBackgroundBrush(); }
    }
}
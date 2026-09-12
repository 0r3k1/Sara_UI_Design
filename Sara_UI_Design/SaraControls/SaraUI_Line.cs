using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sara_UI_Design.Animations;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Dibuja un separador horizontal o vertical con geometría segura, estados
    /// visuales y transiciones administradas por <see cref="SaraAnimator"/>.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultProperty(nameof(LineColor))]
    public class SaraUI_Line:Control {
        /// <summary>Define la dirección de la línea dentro del control.</summary>
        public enum LineOrientation {
            /// <summary>Dibuja la línea de izquierda a derecha.</summary>
            Horizontal,

            /// <summary>Dibuja la línea de arriba hacia abajo.</summary>
            Vertical
        }

        /// <summary>Define la posición de la línea en su eje transversal.</summary>
        public enum LineAlignment {
            /// <summary>Alinea la línea al borde superior o izquierdo.</summary>
            Near,

            /// <summary>Centra la línea en el espacio disponible.</summary>
            Center,

            /// <summary>Alinea la línea al borde inferior o derecho.</summary>
            Far
        }

        /// <summary>Describe el estado de interacción que gobierna la apariencia.</summary>
        public enum SaraLineVisualState {
            /// <summary>El control no tiene una interacción activa.</summary>
            Normal,

            /// <summary>El puntero se encuentra sobre el control.</summary>
            Hovered,

            /// <summary>El control tiene el foco de teclado.</summary>
            Focused,

            /// <summary>El control está deshabilitado.</summary>
            Disabled
        }

        private readonly SaraAnimator _animator;
        private LineOrientation _orientation = LineOrientation.Horizontal;
        private LineAlignment _alignment = LineAlignment.Center;
        private int _lineWidth = 2;
        private Color _lineColor = Color.DimGray;
        private Color _lineColor2 = Color.Empty;
        private Color _hoverLineColor = Color.Empty;
        private Color _focusLineColor = Color.Empty;
        private Color _disabledLineColor = Color.Empty;
        private Color _focusCueColor = Color.MediumSlateBlue;
        private DashStyle _lineStyle = DashStyle.Solid;
        private LineCap _startCap = LineCap.Round;
        private LineCap _endCap = LineCap.Round;
        private DashCap _dashCap = DashCap.Round;
        private bool _scaleLineWidthWithDpi = true;
        private bool _showFocusCue = true;
        private bool _animationEnabled = true;
        private int _animationDuration = 180;
        private int _animationFrameInterval = 15;
        private SaraEasing _animationEasing = SaraEasing.EaseOutCubic;
        private bool _isMouseOver;
        private bool _initialized;
        private bool _disposingResources;
        private SaraLineVisualState _visualState;
        private LineAppearance _displayAppearance;
        private LineAppearance _targetAppearance;

        /// <summary>Inicializa un separador horizontal de 100 por 2 píxeles.</summary>
        public SaraUI_Line() {
            _animator = new SaraAnimator();
            _animator.Completed += Animator_Completed;
            _animator.Canceled += Animator_Canceled;
            _animator.StateChanged += Animator_StateChanged;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.UserPaint,
                true);

            BackColor = Color.Transparent;
            Size = new Size(100, 2);
            TabStop = false;
            AccessibleRole = AccessibleRole.Separator;

            _initialized = true;
            _visualState = DetermineVisualState();
            _targetAppearance = ResolveAppearance(_visualState);
            _displayAppearance = _targetAppearance;
        }

        /// <summary>Se produce cuando cambia el estado visual de interacción.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? VisualStateChanged;

        /// <summary>Se produce cuando una transición visual llega a su destino.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationCompleted;

        /// <summary>Se produce cuando una transición activa se detiene o reemplaza.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationCanceled;

        /// <summary>Se produce cuando cambia el estado del motor interno.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationStateChanged;

        /// <summary>Obtiene o establece si la línea es horizontal o vertical.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(LineOrientation.Horizontal)]
        public LineOrientation Orientation {
            get => _orientation;
            set {
                ValidateEnum(value, nameof(Orientation));

                if(_orientation == value) {
                    return;
                }

                _orientation = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece la alineación en el eje transversal.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(LineAlignment.Center)]
        public LineAlignment Alignment {
            get => _alignment;
            set {
                ValidateEnum(value, nameof(Alignment));

                if(_alignment == value) {
                    return;
                }

                _alignment = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el grosor lógico de la línea en píxeles.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce con valores menores que uno.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(2)]
        public int LineWidth {
            get => _lineWidth;
            set {
                if(value < 1) {
                    throw new ArgumentOutOfRangeException(
                        nameof(LineWidth),
                        value,
                        "El grosor de la línea debe ser mayor que cero.");
                }

                if(_lineWidth == value) {
                    return;
                }

                _lineWidth = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el primer color de la línea.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "DimGray")]
        public Color LineColor {
            get => _lineColor;
            set {
                if(_lineColor == value) {
                    return;
                }

                _lineColor = value;
                RefreshAppearance();
            }
        }

        /// <summary>
        /// Obtiene o establece el segundo color. <see cref="Color.Empty"/> desactiva
        /// el degradado y conserva una línea sólida.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color LineColor2 {
            get => _lineColor2;
            set {
                if(_lineColor2 == value) {
                    return;
                }

                _lineColor2 = value;
                RefreshAppearance();
            }
        }

        /// <summary>
        /// Obtiene o establece el color durante el hover. <see cref="Color.Empty"/>
        /// calcula automáticamente una variante del color normal.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color HoverLineColor {
            get => _hoverLineColor;
            set {
                if(_hoverLineColor == value) {
                    return;
                }

                _hoverLineColor = value;
                RefreshAppearance();
            }
        }

        /// <summary>
        /// Obtiene o establece el color con foco. <see cref="Color.Empty"/> calcula
        /// automáticamente una variante del color normal.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color FocusLineColor {
            get => _focusLineColor;
            set {
                if(_focusLineColor == value) {
                    return;
                }

                _focusLineColor = value;
                RefreshAppearance();
            }
        }

        /// <summary>
        /// Obtiene o establece el color deshabilitado. <see cref="Color.Empty"/>
        /// utiliza <see cref="SystemColors.GrayText"/>.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledLineColor {
            get => _disabledLineColor;
            set {
                if(_disabledLineColor == value) {
                    return;
                }

                _disabledLineColor = value;
                RefreshAppearance();
            }
        }

        /// <summary>Obtiene o establece el color de la guía de foco.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "MediumSlateBlue")]
        public Color FocusCueColor {
            get => _focusCueColor;
            set {
                if(_focusCueColor == value) {
                    return;
                }

                _focusCueColor = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el patrón del trazo.</summary>
        /// <exception cref="ArgumentException">
        /// Se produce al asignar <see cref="DashStyle.Custom"/>, que requiere un patrón externo.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Se produce con un valor desconocido.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(DashStyle.Solid)]
        public DashStyle LineStyle {
            get => _lineStyle;
            set {
                ValidateEnum(value, nameof(LineStyle));

                if(value == DashStyle.Custom) {
                    throw new ArgumentException(
                        "DashStyle.Custom requiere un patrón que el control no administra.",
                        nameof(LineStyle));
                }

                if(_lineStyle == value) {
                    return;
                }

                _lineStyle = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el remate del inicio lógico de la línea.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(LineCap.Round)]
        public LineCap StartCap {
            get => _startCap;
            set {
                ValidateLineCap(value, nameof(StartCap));

                if(_startCap == value) {
                    return;
                }

                _startCap = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el remate del final lógico de la línea.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(LineCap.Round)]
        public LineCap EndCap {
            get => _endCap;
            set {
                ValidateLineCap(value, nameof(EndCap));

                if(_endCap == value) {
                    return;
                }

                _endCap = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el remate aplicado a los segmentos discontinuos.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(DashCap.Round)]
        public DashCap DashCap {
            get => _dashCap;
            set {
                ValidateEnum(value, nameof(DashCap));

                if(_dashCap == value) {
                    return;
                }

                _dashCap = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece si el grosor lógico se adapta al DPI actual.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool ScaleLineWidthWithDpi {
            get => _scaleLineWidthWithDpi;
            set {
                if(_scaleLineWidthWithDpi == value) {
                    return;
                }

                _scaleLineWidthWithDpi = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece si se dibuja una guía cuando el control recibe foco.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool ShowFocusCue {
            get => _showFocusCue;
            set {
                if(_showFocusCue == value) {
                    return;
                }

                _showFocusCue = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece si los cambios de apariencia se animan.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool AnimationEnabled {
            get => _animationEnabled;
            set {
                if(_animationEnabled == value) {
                    return;
                }

                _animationEnabled = value;

                if(!value) {
                    StopAnimatorIfActive();
                    _displayAppearance = _targetAppearance;
                    Invalidate();
                }
            }
        }

        /// <summary>Obtiene o establece la duración de la transición, en milisegundos.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce con valores negativos.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(180)]
        public int AnimationDuration {
            get => _animationDuration;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(
                        nameof(AnimationDuration),
                        value,
                        "La duración no puede ser negativa.");
                }

                _animationDuration = value;
            }
        }

        /// <summary>Obtiene o establece el intervalo solicitado entre cuadros.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce con valores menores que uno.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(15)]
        public int AnimationFrameInterval {
            get => _animationFrameInterval;
            set {
                if(value < 1) {
                    throw new ArgumentOutOfRangeException(
                        nameof(AnimationFrameInterval),
                        value,
                        "El intervalo debe ser mayor que cero.");
                }

                _animationFrameInterval = value;
            }
        }

        /// <summary>Obtiene o establece la curva de aceleración de la transición.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce con una curva desconocida.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(SaraEasing.EaseOutCubic)]
        public SaraEasing AnimationEasing {
            get => _animationEasing;
            set {
                ValidateEnum(value, nameof(AnimationEasing));
                _animationEasing = value;
            }
        }

        /// <summary>Obtiene el estado visual actual.</summary>
        [Browsable(false)]
        public SaraLineVisualState VisualState => _visualState;

        /// <summary>Obtiene el estado del motor de animación interno.</summary>
        [Browsable(false)]
        public SaraAnimationState AnimationState => _animator.State;

        /// <summary>Obtiene el primer color que se está dibujando.</summary>
        [Browsable(false)]
        public Color DisplayedLineColor => _displayAppearance.PrimaryColor;

        /// <summary>Pausa una transición visual activa.</summary>
        public bool PauseAnimation() => _animator.Pause();

        /// <summary>Reanuda una transición visual pausada.</summary>
        public bool ResumeAnimation() => _animator.Resume();

        /// <summary>Detiene una transición visual conservando la apariencia alcanzada.</summary>
        public bool StopAnimation() => _animator.Stop();

        /// <inheritdoc/>
        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            UpdateVisualState();
        }

        /// <inheritdoc/>
        protected override void OnGotFocus(EventArgs e) {
            base.OnGotFocus(e);
            UpdateVisualState();
        }

        /// <inheritdoc/>
        protected override void OnLostFocus(EventArgs e) {
            base.OnLostFocus(e);
            UpdateVisualState();
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            if(e.Button == MouseButtons.Left && TabStop && CanFocus) {
                Focus();
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseEnter(EventArgs e) {
            base.OnMouseEnter(e);
            _isMouseOver = true;
            UpdateVisualState();
        }

        /// <inheritdoc/>
        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            _isMouseOver = false;
            UpdateVisualState();
        }

        /// <inheritdoc/>
        protected override void OnPaddingChanged(EventArgs e) {
            base.OnPaddingChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnRightToLeftChanged(EventArgs e) {
            base.OnRightToLeftChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            RectangleF contentBounds = GetContentBounds();

            if(contentBounds.Width <= 0f || contentBounds.Height <= 0f) {
                DrawFocusCue(e.Graphics);
                return;
            }

            float thickness = GetEffectiveLineWidth(contentBounds);
            GetLinePoints(contentBounds, thickness, out PointF start, out PointF end);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Color primary = NormalizePaintColor(_displayAppearance.PrimaryColor);
            Color secondary = NormalizePaintColor(_displayAppearance.SecondaryColor);

            if(_lineColor2.IsEmpty) {
                using Pen pen = CreatePen(primary, thickness);
                e.Graphics.DrawLine(pen, start, end);
            } else {
                PointF gradientStart = start;
                PointF gradientEnd = end;

                if(AreSamePoint(gradientStart, gradientEnd)) {
                    gradientEnd = _orientation == LineOrientation.Horizontal
                        ? new PointF(gradientStart.X + 1f, gradientStart.Y)
                        : new PointF(gradientStart.X, gradientStart.Y + 1f);
                }

                using LinearGradientBrush brush = new LinearGradientBrush(
                    gradientStart,
                    gradientEnd,
                    primary,
                    secondary);
                brush.WrapMode = WrapMode.TileFlipXY;
                using Pen pen = CreatePen(brush, thickness);
                e.Graphics.DrawLine(pen, start, end);
            }

            DrawFocusCue(e.Graphics);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            if(disposing && !_disposingResources) {
                _disposingResources = true;
                _animator.Completed -= Animator_Completed;
                _animator.Canceled -= Animator_Canceled;
                _animator.StateChanged -= Animator_StateChanged;
                _animator.Dispose();
                VisualStateChanged = null;
                AnimationCompleted = null;
                AnimationCanceled = null;
                AnimationStateChanged = null;
            }

            base.Dispose(disposing);
        }

        private RectangleF GetContentBounds() {
            float left = Padding.Left;
            float top = Padding.Top;
            float width = Math.Max(0, ClientSize.Width - Padding.Horizontal);
            float height = Math.Max(0, ClientSize.Height - Padding.Vertical);
            return new RectangleF(left, top, width, height);
        }

        private float GetEffectiveLineWidth(RectangleF bounds) {
            float scale = _scaleLineWidthWithDpi ? DeviceDpi / 96f : 1f;
            float requested = Math.Max(1f, _lineWidth * scale);
            float crossSpace = _orientation == LineOrientation.Horizontal
                ? bounds.Height
                : bounds.Width;
            return Math.Max(1f, Math.Min(requested, Math.Max(1f, crossSpace)));
        }

        private void GetLinePoints(
            RectangleF bounds,
            float thickness,
            out PointF start,
            out PointF end) {
            float half = thickness / 2f;

            if(_orientation == LineOrientation.Horizontal) {
                float y = GetAlignedCoordinate(bounds.Top, bounds.Bottom, half);
                float left = Math.Min(bounds.Right, bounds.Left + half);
                float right = Math.Max(bounds.Left, bounds.Right - half);

                if(left > right) {
                    left = right = bounds.Left + (bounds.Width / 2f);
                }

                bool reverse = RightToLeft == RightToLeft.Yes;
                start = reverse ? new PointF(right, y) : new PointF(left, y);
                end = reverse ? new PointF(left, y) : new PointF(right, y);
                return;
            }

            float x = GetAlignedCoordinate(bounds.Left, bounds.Right, half);
            float top = Math.Min(bounds.Bottom, bounds.Top + half);
            float bottom = Math.Max(bounds.Top, bounds.Bottom - half);

            if(top > bottom) {
                top = bottom = bounds.Top + (bounds.Height / 2f);
            }

            start = new PointF(x, top);
            end = new PointF(x, bottom);
        }

        private float GetAlignedCoordinate(float near, float far, float halfThickness) {
            switch(_alignment) {
                case LineAlignment.Near:
                    return Math.Min(far, near + halfThickness);
                case LineAlignment.Far:
                    return Math.Max(near, far - halfThickness);
                default:
                    return near + ((far - near) / 2f);
            }
        }

        private Pen CreatePen(Color color, float thickness) {
            return ConfigurePen(new Pen(color, thickness));
        }

        private Pen CreatePen(Brush brush, float thickness) {
            return ConfigurePen(new Pen(brush, thickness));
        }

        private Pen ConfigurePen(Pen pen) {
            pen.DashStyle = _lineStyle;
            pen.StartCap = _startCap;
            pen.EndCap = _endCap;
            pen.DashCap = _dashCap;
            return pen;
        }

        private void DrawFocusCue(Graphics graphics) {
            if(!_showFocusCue || !Focused || !ShowFocusCues ||
                ClientSize.Width < 4 || ClientSize.Height < 4) {
                return;
            }

            Rectangle bounds = ClientRectangle;
            bounds.Inflate(-1, -1);
            ControlPaint.DrawFocusRectangle(graphics, bounds, _focusCueColor, BackColor);
        }

        private void UpdateVisualState() {
            if(!_initialized || _disposingResources) {
                return;
            }

            SaraLineVisualState nextState = DetermineVisualState();

            if(_visualState != nextState) {
                _visualState = nextState;
                VisualStateChanged?.Invoke(this, EventArgs.Empty);
            }

            TransitionTo(ResolveAppearance(nextState));
        }

        private SaraLineVisualState DetermineVisualState() {
            if(!Enabled) {
                return SaraLineVisualState.Disabled;
            }

            if(_isMouseOver) {
                return SaraLineVisualState.Hovered;
            }

            return Focused
                ? SaraLineVisualState.Focused
                : SaraLineVisualState.Normal;
        }

        private LineAppearance ResolveAppearance(SaraLineVisualState state) {
            Color primary = _lineColor;
            Color secondary = _lineColor2.IsEmpty ? _lineColor : _lineColor2;

            switch(state) {
                case SaraLineVisualState.Hovered:
                    return new LineAppearance(
                        _hoverLineColor.IsEmpty ? ControlPaint.Light(primary) : _hoverLineColor,
                        ControlPaint.Light(secondary));
                case SaraLineVisualState.Focused:
                    return new LineAppearance(
                        _focusLineColor.IsEmpty ? _focusCueColor : _focusLineColor,
                        _focusLineColor.IsEmpty ? ControlPaint.Dark(secondary) : _focusLineColor);
                case SaraLineVisualState.Disabled:
                    Color disabled = _disabledLineColor.IsEmpty
                        ? SystemColors.GrayText
                        : _disabledLineColor;
                    return new LineAppearance(disabled, disabled);
                default:
                    return new LineAppearance(primary, secondary);
            }
        }

        private void RefreshAppearance() {
            if(!_initialized || _disposingResources) {
                Invalidate();
                return;
            }

            TransitionTo(ResolveAppearance(_visualState));
        }

        private void TransitionTo(LineAppearance target) {
            _targetAppearance = target;

            if(!IsHandleCreated ||
                !_animationEnabled ||
                _animationDuration == 0 ||
                IsInDesignMode()) {
                StopAnimatorIfActive();
                _displayAppearance = target;
                Invalidate();
                return;
            }

            LineAppearance start = _displayAppearance;
            _animator.Start(
                0f,
                1f,
                progress => {
                    _displayAppearance = LineAppearance.Lerp(start, target, progress);
                    Invalidate();
                },
                new SaraAnimationOptions {
                    Duration = _animationDuration,
                    FrameInterval = _animationFrameInterval,
                    Easing = _animationEasing
                });
        }

        private void Animator_Completed(object? sender, EventArgs e) {
            _displayAppearance = _targetAppearance;
            Invalidate();
            AnimationCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void Animator_Canceled(object? sender, EventArgs e) {
            AnimationCanceled?.Invoke(this, EventArgs.Empty);
        }

        private void Animator_StateChanged(object? sender, EventArgs e) {
            AnimationStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StopAnimatorIfActive() {
            if(_animator.IsRunning || _animator.IsPaused) {
                _animator.Stop();
            }
        }

        private bool IsInDesignMode() {
            return DesignMode ||
                LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        private static void ValidateEnum<TEnum>(TEnum value, string propertyName)
            where TEnum:struct {
            if(!Enum.IsDefined(typeof(TEnum), value)) {
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    value,
                    "El valor indicado no pertenece a la enumeración compatible.");
            }
        }

        private static void ValidateLineCap(LineCap value, string propertyName) {
            ValidateEnum(value, propertyName);

            if(value == LineCap.Custom) {
                throw new ArgumentException(
                    "LineCap.Custom requiere un remate externo que el control no administra.",
                    propertyName);
            }
        }

        private static bool AreSamePoint(PointF first, PointF second) {
            return Math.Abs(first.X - second.X) < 0.01f &&
                Math.Abs(first.Y - second.Y) < 0.01f;
        }

        private static Color NormalizePaintColor(Color color) {
            return color.IsEmpty ? Color.Transparent : color;
        }

        private readonly struct LineAppearance {
            public LineAppearance(Color primaryColor, Color secondaryColor) {
                PrimaryColor = primaryColor;
                SecondaryColor = secondaryColor;
            }

            public Color PrimaryColor { get; }

            public Color SecondaryColor { get; }

            public static LineAppearance Lerp(
                LineAppearance from,
                LineAppearance to,
                float progress) {
                return new LineAppearance(
                    LerpColor(from.PrimaryColor, to.PrimaryColor, progress),
                    LerpColor(from.SecondaryColor, to.SecondaryColor, progress));
            }

            private static Color LerpColor(Color from, Color to, float progress) {
                float amount = Math.Max(0f, Math.Min(1f, progress));
                return Color.FromArgb(
                    LerpChannel(from.A, to.A, amount),
                    LerpChannel(from.R, to.R, amount),
                    LerpChannel(from.G, to.G, amount),
                    LerpChannel(from.B, to.B, amount));
            }

            private static int LerpChannel(int from, int to, float progress) {
                return (int)Math.Round(from + ((to - from) * progress));
            }
        }
    }
}

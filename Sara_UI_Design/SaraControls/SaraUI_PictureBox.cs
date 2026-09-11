using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sara_UI_Design.Animations;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Muestra una imagen circular o rectangular con recorte seguro, borde degradado
    /// y transiciones visuales administradas por <see cref="SaraAnimator"/>.
    /// </summary>
    [DefaultEvent(nameof(Click))]
    [ToolboxItem(true)]
    public class SaraUI_PictureBox:PictureBox {
        /// <summary>Describe el estado que gobierna la apariencia del borde.</summary>
        public enum SaraPictureBoxVisualState {
            /// <summary>El control no tiene una interacción activa.</summary>
            Normal,

            /// <summary>El puntero se encuentra sobre la imagen.</summary>
            Hovered,

            /// <summary>El control conserva el foco de teclado.</summary>
            Focused,

            /// <summary>El control está deshabilitado.</summary>
            Disabled
        }

        private readonly SaraAnimator _animator;
        private int _borderSize = 2;
        private Color _borderColor = Color.RoyalBlue;
        private Color _borderColor2 = Color.HotPink;
        private DashStyle _borderLineStyle = DashStyle.Solid;
        private DashCap _borderCapStyle = DashCap.Flat;
        private float _gradientAngle = 50f;
        private bool _isCircular = true;
        private int _borderRadius = 12;
        private bool _maintainCircularAspectRatio = true;
        private bool _clipToShape = true;
        private Color _hoverBorderColor = Color.Empty;
        private Color _hoverBorderColor2 = Color.Empty;
        private Color _focusBorderColor = Color.Empty;
        private Color _focusBorderColor2 = Color.Empty;
        private Color _disabledBorderColor = Color.Empty;
        private Color _disabledBorderColor2 = Color.Empty;
        private Color _disabledOverlayColor = Color.Empty;
        private bool _useAnimations = true;
        private int _animationDuration = 180;
        private int _animationFrameInterval = 15;
        private SaraEasing _animationEasing = SaraEasing.EaseOutQuad;
        private bool _showFocusCue = true;
        private bool _isMouseOver;
        private bool _adjustingCircularSize;
        private bool _initialized;
        private bool _disposed;
        private Region? _managedRegion;
        private Size _managedRegionSize = Size.Empty;
        private bool _managedRegionCircular;
        private int _managedRegionRadius = -1;
        private SaraPictureBoxVisualState _visualState;
        private PictureAppearance _displayAppearance;

        /// <summary>Inicializa un control de imagen circular, accesible y con doble búfer.</summary>
        public SaraUI_PictureBox() {
            _animator = new SaraAnimator();
            _animator.Completed += Animator_Completed;
            _animator.Canceled += Animator_Canceled;
            _animator.StateChanged += Animator_StateChanged;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable |
                ControlStyles.SupportsTransparentBackColor,
                true);

            Size = new Size(100, 100);
            SizeMode = PictureBoxSizeMode.StretchImage;
            TabStop = false;
            AccessibleRole = AccessibleRole.Graphic;

            _initialized = true;
            _visualState = DetermineVisualState();
            _displayAppearance = ResolveAppearance(_visualState);
            ApplyCircularSizeConstraint();
            UpdateShapeRegion();
        }

        /// <summary>Se produce cuando cambia el estado visual de la imagen.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? VisualStateChanged;

        /// <summary>Se produce cuando finaliza una transición visual.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationCompleted;

        /// <summary>Se produce cuando una transición visual se cancela o se reemplaza.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationCanceled;

        /// <summary>Se produce cuando cambia el estado del motor de animación interno.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationStateChanged;

        /// <summary>Obtiene o establece el grosor solicitado del borde, en píxeles.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(2)]
        public int BorderSize {
            get => _borderSize;
            set {
                EnsureNonNegative(value, nameof(BorderSize));

                if(_borderSize == value) {
                    return;
                }

                _borderSize = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el primer color del borde degradado.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "RoyalBlue")]
        public Color BorderColor {
            get => _borderColor;
            set => SetAppearanceColor(ref _borderColor, value);
        }

        /// <summary>Obtiene o establece el segundo color del borde degradado.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "HotPink")]
        public Color BorderColor2 {
            get => _borderColor2;
            set => SetAppearanceColor(ref _borderColor2, value);
        }

        /// <summary>Obtiene o establece el patrón de la línea del borde.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce con un valor desconocido.</exception>
        /// <exception cref="NotSupportedException">
        /// Se produce al solicitar <see cref="DashStyle.Custom"/>, que requiere un patrón externo.
        /// </exception>
        [Category("Sara UI Design")]
        [DefaultValue(DashStyle.Solid)]
        public DashStyle BorderLineStyle {
            get => _borderLineStyle;
            set {
                EnsureDefined(value, nameof(BorderLineStyle));

                if(value == DashStyle.Custom) {
                    throw new NotSupportedException(
                        "DashStyle.Custom requiere un patrón personalizado y no es compatible con esta propiedad.");
                }

                if(_borderLineStyle == value) {
                    return;
                }

                _borderLineStyle = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece la terminación de los trazos discontinuos.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce con un valor desconocido.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(DashCap.Flat)]
        public DashCap BorderCapStyle {
            get => _borderCapStyle;
            set {
                EnsureDefined(value, nameof(BorderCapStyle));

                if(_borderCapStyle == value) {
                    return;
                }

                _borderCapStyle = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el ángulo normalizado del degradado, entre 0 y 360 grados.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce con NaN o infinito.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(50f)]
        public float GradientAngle {
            get => _gradientAngle;
            set {
                float normalized = NormalizeAngle(value, nameof(GradientAngle));

                if(Math.Abs(_gradientAngle - normalized) < float.Epsilon) {
                    return;
                }

                _gradientAngle = normalized;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece si la superficie se representa como círculo o elipse.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool IsCircular {
            get => _isCircular;
            set {
                if(_isCircular == value) {
                    return;
                }

                _isCircular = value;
                ApplyCircularSizeConstraint();
                InvalidateRegionCache();
                UpdateShapeRegion();
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el radio solicitado cuando <see cref="IsCircular"/> es falso.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(12)]
        public int BorderRadius {
            get => _borderRadius;
            set {
                EnsureNonNegative(value, nameof(BorderRadius));

                if(_borderRadius == value) {
                    return;
                }

                _borderRadius = value;
                InvalidateRegionCache();
                UpdateShapeRegion();
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece si el ancho gobierna también la altura en modo circular.
        /// Desactívelo cuando un contenedor administre ambas dimensiones.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool MaintainCircularAspectRatio {
            get => _maintainCircularAspectRatio;
            set {
                if(_maintainCircularAspectRatio == value) {
                    return;
                }

                _maintainCircularAspectRatio = value;
                ApplyCircularSizeConstraint();
                InvalidateRegionCache();
                UpdateShapeRegion();
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece si la imagen se recorta utilizando la forma configurada.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool ClipToShape {
            get => _clipToShape;
            set {
                if(_clipToShape == value) {
                    return;
                }

                _clipToShape = value;
                InvalidateRegionCache();
                UpdateShapeRegion();
                Invalidate();
            }
        }

        /// <summary><see cref="Color.Empty"/> genera una variante clara del primer color durante hover.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color HoverBorderColor {
            get => _hoverBorderColor;
            set => SetAppearanceColor(ref _hoverBorderColor, value);
        }

        /// <summary><see cref="Color.Empty"/> genera una variante clara del segundo color durante hover.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color HoverBorderColor2 {
            get => _hoverBorderColor2;
            set => SetAppearanceColor(ref _hoverBorderColor2, value);
        }

        /// <summary><see cref="Color.Empty"/> utiliza el segundo color normal cuando el control tiene foco.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color FocusBorderColor {
            get => _focusBorderColor;
            set => SetAppearanceColor(ref _focusBorderColor, value);
        }

        /// <summary><see cref="Color.Empty"/> utiliza el primer color normal como segundo color con foco.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color FocusBorderColor2 {
            get => _focusBorderColor2;
            set => SetAppearanceColor(ref _focusBorderColor2, value);
        }

        /// <summary><see cref="Color.Empty"/> deriva un primer color atenuado al deshabilitarse.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledBorderColor {
            get => _disabledBorderColor;
            set => SetAppearanceColor(ref _disabledBorderColor, value);
        }

        /// <summary><see cref="Color.Empty"/> deriva un segundo color atenuado al deshabilitarse.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledBorderColor2 {
            get => _disabledBorderColor2;
            set => SetAppearanceColor(ref _disabledBorderColor2, value);
        }

        /// <summary><see cref="Color.Empty"/> aplica automáticamente una capa translúcida al deshabilitarse.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledOverlayColor {
            get => _disabledOverlayColor;
            set => SetAppearanceColor(ref _disabledOverlayColor, value);
        }

        /// <summary>Obtiene o establece si los cambios de estado interpolan los colores.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool UseAnimations {
            get => _useAnimations;
            set {
                if(_useAnimations == value) {
                    return;
                }

                _useAnimations = value;

                if(_initialized && !value) {
                    StopAnimatorIfActive();
                    ApplyAppearance(ResolveAppearance(_visualState));
                }
            }
        }

        /// <summary>Obtiene o establece la duración de cada transición, en milisegundos.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(180)]
        public int AnimationDuration {
            get => _animationDuration;
            set {
                EnsureNonNegative(value, nameof(AnimationDuration));
                _animationDuration = value;
            }
        }

        /// <summary>Obtiene o establece el intervalo entre fotogramas, en milisegundos.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor menor que uno.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(15)]
        public int AnimationFrameInterval {
            get => _animationFrameInterval;
            set {
                if(value < 1) {
                    throw new ArgumentOutOfRangeException(
                        nameof(AnimationFrameInterval), value, "El intervalo debe ser mayor que cero.");
                }

                _animationFrameInterval = value;
            }
        }

        /// <summary>Obtiene o establece la curva utilizada para interpolar los estados.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce con una curva desconocida.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(SaraEasing.EaseOutQuad)]
        public SaraEasing AnimationEasing {
            get => _animationEasing;
            set {
                EnsureDefined(value, nameof(AnimationEasing));
                _animationEasing = value;
            }
        }

        /// <summary>Obtiene o establece si se dibuja una guía cuando el control tiene foco.</summary>
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

        /// <summary>Obtiene el estado visual actual.</summary>
        [Browsable(false)]
        public SaraPictureBoxVisualState VisualState => _visualState;

        /// <summary>Obtiene el estado del motor de animación interno.</summary>
        [Browsable(false)]
        public SaraAnimationState AnimationState => _animator.State;

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
        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            InvalidateRegionCache();
            UpdateShapeRegion();
        }

        /// <inheritdoc/>
        protected override void OnHandleDestroyed(EventArgs e) {
            StopAnimatorIfActive();
            ReleaseManagedRegion();
            base.OnHandleDestroyed(e);
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
        protected override void OnResize(EventArgs e) {
            base.OnResize(e);

            if(!_initialized || _disposed) {
                return;
            }

            ApplyCircularSizeConstraint();
            InvalidateRegionCache();
            UpdateShapeRegion();
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnSizeModeChanged(EventArgs e) {
            base.OnSizeModeChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnVisibleChanged(EventArgs e) {
            base.OnVisibleChanged(e);

            if(_initialized && !Visible) {
                StopAnimatorIfActive();
                ApplyAppearance(ResolveAppearance(_visualState));
            }
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs pe) {
            UpdateShapeRegion();
            base.OnPaint(pe);

            if(ClientSize.Width <= 0 || ClientSize.Height <= 0) {
                return;
            }

            Graphics graphics = pe.Graphics;
            SmoothingMode previousSmoothing = graphics.SmoothingMode;
            PixelOffsetMode previousPixelOffset = graphics.PixelOffsetMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            try {
                DrawDisabledOverlay(graphics);
                DrawBorder(graphics);
                DrawFocusCue(graphics);
            } finally {
                graphics.SmoothingMode = previousSmoothing;
                graphics.PixelOffsetMode = previousPixelOffset;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            if(disposing && !_disposed) {
                _disposed = true;
                _animator.Completed -= Animator_Completed;
                _animator.Canceled -= Animator_Canceled;
                _animator.StateChanged -= Animator_StateChanged;
                _animator.Dispose();
                ReleaseManagedRegion();
                VisualStateChanged = null;
                AnimationCompleted = null;
                AnimationCanceled = null;
                AnimationStateChanged = null;
            }

            base.Dispose(disposing);
        }

        private void ApplyCircularSizeConstraint() {
            if(!_initialized || _disposed || _adjustingCircularSize ||
                !_isCircular || !_maintainCircularAspectRatio || Width == Height) {
                return;
            }

            _adjustingCircularSize = true;

            try {
                Height = Width;
            } finally {
                _adjustingCircularSize = false;
            }
        }

        private void UpdateVisualState(bool animate = true) {
            if(!_initialized || _disposed) {
                return;
            }

            SaraPictureBoxVisualState nextState = DetermineVisualState();

            if(nextState == _visualState) {
                return;
            }

            _visualState = nextState;
            VisualStateChanged?.Invoke(this, EventArgs.Empty);
            TransitionToAppearance(ResolveAppearance(nextState), animate);
        }

        private SaraPictureBoxVisualState DetermineVisualState() {
            if(!Enabled) {
                return SaraPictureBoxVisualState.Disabled;
            }

            if(_isMouseOver) {
                return SaraPictureBoxVisualState.Hovered;
            }

            return Focused
                ? SaraPictureBoxVisualState.Focused
                : SaraPictureBoxVisualState.Normal;
        }

        private PictureAppearance ResolveAppearance(SaraPictureBoxVisualState state) {
            switch(state) {
                case SaraPictureBoxVisualState.Hovered:
                    return new PictureAppearance(
                        ResolveOptionalColor(
                            _hoverBorderColor,
                            Blend(_borderColor, Color.White, 0.18f)),
                        ResolveOptionalColor(
                            _hoverBorderColor2,
                            Blend(_borderColor2, Color.White, 0.18f)),
                        Color.Transparent);

                case SaraPictureBoxVisualState.Focused:
                    return new PictureAppearance(
                        ResolveOptionalColor(_focusBorderColor, _borderColor2),
                        ResolveOptionalColor(_focusBorderColor2, _borderColor),
                        Color.Transparent);

                case SaraPictureBoxVisualState.Disabled:
                    return new PictureAppearance(
                        ResolveOptionalColor(
                            _disabledBorderColor,
                            Blend(_borderColor, SystemColors.ControlDark, 0.52f)),
                        ResolveOptionalColor(
                            _disabledBorderColor2,
                            Blend(_borderColor2, SystemColors.ControlDark, 0.52f)),
                        ResolveOptionalColor(
                            _disabledOverlayColor,
                            Color.FromArgb(110, SystemColors.Control)));

                default:
                    return new PictureAppearance(_borderColor, _borderColor2, Color.Transparent);
            }
        }

        private void TransitionToAppearance(PictureAppearance destination, bool animate) {
            if(!animate || !_useAnimations || _animationDuration == 0 ||
                IsInDesignMode || !IsHandleCreated || !Visible) {
                StopAnimatorIfActive();
                ApplyAppearance(destination);
                return;
            }

            PictureAppearance origin = _displayAppearance;
            _animator.Start(
                0f,
                1f,
                progress => {
                    _displayAppearance = PictureAppearance.Interpolate(origin, destination, progress);
                    Invalidate();
                },
                new SaraAnimationOptions {
                    Duration = _animationDuration,
                    Easing = _animationEasing,
                    FrameInterval = _animationFrameInterval
                });
        }

        private void RefreshCurrentAppearance() {
            if(!_initialized || _disposed) {
                return;
            }

            StopAnimatorIfActive();
            ApplyAppearance(ResolveAppearance(_visualState));
        }

        private void ApplyAppearance(PictureAppearance appearance) {
            _displayAppearance = appearance;
            Invalidate();
        }

        private void SetAppearanceColor(ref Color field, Color value) {
            if(field == value) {
                return;
            }

            field = value;
            RefreshCurrentAppearance();
        }

        private void DrawDisabledOverlay(Graphics graphics) {
            Color overlayColor = _displayAppearance.OverlayColor;

            if(overlayColor.A == 0) {
                return;
            }

            RectangleF surfaceRectangle = GetSurfaceRectangle();

            if(surfaceRectangle.Width <= 0f || surfaceRectangle.Height <= 0f) {
                return;
            }

            using GraphicsPath path = CreateShapePath(
                surfaceRectangle,
                _isCircular,
                GetEffectiveRadius(surfaceRectangle));
            using SolidBrush brush = new SolidBrush(overlayColor);
            graphics.FillPath(brush, path);
        }

        private void DrawBorder(Graphics graphics) {
            if(_borderSize <= 0) {
                return;
            }

            RectangleF surfaceRectangle = GetSurfaceRectangle();

            if(surfaceRectangle.Width <= 0f || surfaceRectangle.Height <= 0f) {
                return;
            }

            float maximumBorderSize = Math.Min(surfaceRectangle.Width, surfaceRectangle.Height) / 2f;
            float effectiveBorderSize = Math.Min(_borderSize, maximumBorderSize);
            float inset = effectiveBorderSize / 2f;
            RectangleF borderRectangle = RectangleF.Inflate(surfaceRectangle, -inset, -inset);

            if(borderRectangle.Width <= 0f || borderRectangle.Height <= 0f) {
                return;
            }

            float effectiveRadius = Math.Max(0f, GetEffectiveRadius(surfaceRectangle) - inset);
            using GraphicsPath borderPath = CreateShapePath(
                borderRectangle,
                _isCircular,
                effectiveRadius);
            using Brush borderBrush = CreateBorderBrush(borderRectangle);
            using Pen borderPen = new Pen(borderBrush, effectiveBorderSize) {
                Alignment = PenAlignment.Center,
                DashStyle = _borderLineStyle,
                DashCap = _borderCapStyle
            };
            graphics.DrawPath(borderPen, borderPath);
        }

        private Brush CreateBorderBrush(RectangleF rectangle) {
            if(_displayAppearance.BorderColor == _displayAppearance.BorderColor2) {
                return new SolidBrush(_displayAppearance.BorderColor);
            }

            return new LinearGradientBrush(
                rectangle,
                _displayAppearance.BorderColor,
                _displayAppearance.BorderColor2,
                _gradientAngle);
        }

        private void DrawFocusCue(Graphics graphics) {
            if(!_showFocusCue || !Focused || !ShowFocusCues || !Enabled) {
                return;
            }

            int inset = Math.Max(3, Math.Min(_borderSize + 2, 12));
            Rectangle focusRectangle = Rectangle.Inflate(ClientRectangle, -inset, -inset);

            if(focusRectangle.Width > 0 && focusRectangle.Height > 0) {
                ControlPaint.DrawFocusRectangle(
                    graphics,
                    focusRectangle,
                    ForeColor,
                    BackColor);
            }
        }

        private void UpdateShapeRegion() {
            if(!_initialized || _disposed) {
                return;
            }

            if(!_clipToShape || ClientSize.Width <= 0 || ClientSize.Height <= 0) {
                ReleaseManagedRegion();
                return;
            }

            RectangleF rectangle = new RectangleF(0f, 0f, ClientSize.Width, ClientSize.Height);
            int effectiveRadius = (int)Math.Round(GetEffectiveRadius(rectangle));
            bool requiresCustomRegion = _isCircular || effectiveRadius > 0;

            if(!requiresCustomRegion) {
                ReleaseManagedRegion();
                _managedRegionSize = ClientSize;
                _managedRegionCircular = false;
                _managedRegionRadius = 0;
                return;
            }

            if(_managedRegion != null &&
                ReferenceEquals(Region, _managedRegion) &&
                _managedRegionSize == ClientSize &&
                _managedRegionCircular == _isCircular &&
                _managedRegionRadius == effectiveRadius) {
                return;
            }

            using GraphicsPath path = CreateShapePath(rectangle, _isCircular, effectiveRadius);
            Region nextRegion = new Region(path);
            Region? previousManagedRegion = _managedRegion;
            Region = nextRegion;
            _managedRegion = nextRegion;
            _managedRegionSize = ClientSize;
            _managedRegionCircular = _isCircular;
            _managedRegionRadius = effectiveRadius;
            previousManagedRegion?.Dispose();
        }

        private void InvalidateRegionCache() {
            _managedRegionSize = Size.Empty;
            _managedRegionRadius = -1;
        }

        private void ReleaseManagedRegion() {
            Region? managedRegion = _managedRegion;

            if(managedRegion != null) {
                if(ReferenceEquals(Region, managedRegion)) {
                    Region = null;
                }

                managedRegion.Dispose();
                _managedRegion = null;
            }

            _managedRegionSize = Size.Empty;
            _managedRegionCircular = false;
            _managedRegionRadius = -1;
        }

        private RectangleF GetSurfaceRectangle() {
            return new RectangleF(
                0.5f,
                0.5f,
                Math.Max(0f, ClientSize.Width - 1f),
                Math.Max(0f, ClientSize.Height - 1f));
        }

        private float GetEffectiveRadius(RectangleF rectangle) {
            if(_isCircular) {
                return Math.Min(rectangle.Width, rectangle.Height) / 2f;
            }

            float maximumRadius = Math.Min(rectangle.Width, rectangle.Height) / 2f;
            return Math.Max(0f, Math.Min(_borderRadius, maximumRadius));
        }

        private static GraphicsPath CreateShapePath(
            RectangleF rectangle,
            bool circular,
            float radius) {
            GraphicsPath path = new GraphicsPath();

            if(rectangle.Width <= 0f || rectangle.Height <= 0f) {
                return path;
            }

            if(circular) {
                path.AddEllipse(rectangle);
                path.CloseFigure();
                return path;
            }

            float diameter = Math.Min(
                Math.Max(0f, radius * 2f),
                Math.Min(rectangle.Width, rectangle.Height));

            if(diameter <= 0f) {
                path.AddRectangle(rectangle);
                path.CloseFigure();
                return path;
            }

            RectangleF arc = new RectangleF(
                rectangle.Left,
                rectangle.Top,
                diameter,
                diameter);
            path.AddArc(arc, 180f, 90f);
            arc.X = rectangle.Right - diameter;
            path.AddArc(arc, 270f, 90f);
            arc.Y = rectangle.Bottom - diameter;
            path.AddArc(arc, 0f, 90f);
            arc.X = rectangle.Left;
            path.AddArc(arc, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        private void StopAnimatorIfActive() {
            if(!_disposed && (_animator.IsRunning || _animator.IsPaused)) {
                _animator.Stop();
            }
        }

        private bool IsInDesignMode {
            get {
                return LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
                    (Site?.DesignMode ?? false);
            }
        }

        private static Color ResolveOptionalColor(Color configuredColor, Color fallbackColor) {
            return configuredColor.IsEmpty ? fallbackColor : configuredColor;
        }

        private static Color Blend(Color from, Color to, float amount) {
            return InterpolateColor(from, to, Math.Max(0f, Math.Min(1f, amount)));
        }

        private static Color InterpolateColor(Color from, Color to, float progress) {
            float clampedProgress = Math.Max(0f, Math.Min(1f, progress));
            return Color.FromArgb(
                InterpolateChannel(from.A, to.A, clampedProgress),
                InterpolateChannel(from.R, to.R, clampedProgress),
                InterpolateChannel(from.G, to.G, clampedProgress),
                InterpolateChannel(from.B, to.B, clampedProgress));
        }

        private static int InterpolateChannel(int from, int to, float progress) {
            return (int)Math.Round(from + ((to - from) * progress));
        }

        private static float NormalizeAngle(float value, string propertyName) {
            if(float.IsNaN(value) || float.IsInfinity(value)) {
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    value,
                    "El ángulo debe ser un número finito.");
            }

            float normalized = value % 360f;
            return normalized < 0f ? normalized + 360f : normalized;
        }

        private static void EnsureNonNegative(int value, string propertyName) {
            if(value < 0) {
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    value,
                    "El valor no puede ser negativo.");
            }
        }

        private static void EnsureDefined<TEnum>(TEnum value, string propertyName)
            where TEnum:struct, Enum {
            if(!Enum.IsDefined(typeof(TEnum), value)) {
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    value,
                    "El valor indicado no pertenece a la enumeración compatible.");
            }
        }

        private void Animator_Completed(object? sender, EventArgs e) {
            AnimationCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void Animator_Canceled(object? sender, EventArgs e) {
            AnimationCanceled?.Invoke(this, EventArgs.Empty);
        }

        private void Animator_StateChanged(object? sender, EventArgs e) {
            AnimationStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private readonly struct PictureAppearance {
            public PictureAppearance(Color borderColor, Color borderColor2, Color overlayColor) {
                BorderColor = borderColor;
                BorderColor2 = borderColor2;
                OverlayColor = overlayColor;
            }

            public Color BorderColor { get; }

            public Color BorderColor2 { get; }

            public Color OverlayColor { get; }

            public static PictureAppearance Interpolate(
                PictureAppearance origin,
                PictureAppearance destination,
                float progress) {
                return new PictureAppearance(
                    InterpolateColor(origin.BorderColor, destination.BorderColor, progress),
                    InterpolateColor(origin.BorderColor2, destination.BorderColor2, progress),
                    InterpolateColor(origin.OverlayColor, destination.OverlayColor, progress));
            }
        }
    }
}

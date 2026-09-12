using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sara_UI_Design.Animations;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Representa un botón personalizado con bordes redondeados, iconos integrados,
    /// estados visuales accesibles y transiciones de color administradas por <see cref="SaraAnimator"/>.
    /// </summary>
    [DefaultEvent(nameof(Click))]
    [ToolboxItem(true)]
    public class SaraUI_Button:Button {
        /// <summary>
        /// Define la ubicación horizontal del icono respecto al texto.
        /// </summary>
        public enum SaraIconLocation {
            /// <summary>Coloca el icono antes del texto.</summary>
            Left,

            /// <summary>Coloca el icono después del texto.</summary>
            Right
        }

        /// <summary>
        /// Describe el estado de interacción que gobierna la apariencia actual del botón.
        /// </summary>
        public enum SaraButtonVisualState {
            /// <summary>El botón no tiene una interacción activa.</summary>
            Normal,

            /// <summary>El puntero se encuentra sobre el botón.</summary>
            Hovered,

            /// <summary>El botón se está presionando con el ratón o el teclado.</summary>
            Pressed,

            /// <summary>El botón tiene el foco de teclado.</summary>
            Focused,

            /// <summary>El botón está deshabilitado.</summary>
            Disabled
        }

        private readonly SaraAnimator _animator;
        private int _borderSize;
        private int _borderRadius = 20;
        private Color _borderColor = Color.PaleVioletRed;
        private string _iconName = "None";
        private int _iconSize = 16;
        private int _iconPadding = 8;
        private Color _iconColor = Color.Empty;
        private SaraIconLocation _iconLocation = SaraIconLocation.Left;
        private SaraUI_IconLibrary.SaraIconStyle _iconStyle = SaraUI_IconLibrary.SaraIconStyle.Outline;
        private Color _hoverBackColor = Color.Empty;
        private Color _hoverForeColor = Color.Empty;
        private Color _pressedBackColor = Color.Empty;
        private Color _pressedForeColor = Color.Empty;
        private Color _focusBorderColor = Color.Empty;
        private Color _disabledBackColor = Color.Empty;
        private Color _disabledForeColor = Color.Empty;
        private Color _disabledBorderColor = Color.Empty;
        private bool _useAnimations = true;
        private int _animationDuration = 140;
        private int _animationFrameInterval = 15;
        private SaraEasing _animationEasing = SaraEasing.EaseOutQuad;
        private bool _showFocusBorder = true;
        private int _pressedContentOffset = 1;
        private bool _isMouseOver;
        private bool _isPressed;
        private bool _initialized;
        private bool _disposed;
        private SaraButtonVisualState _visualState;
        private ButtonAppearance _displayAppearance;

        /// <summary>
        /// Inicializa un botón con apariencia moderna, doble búfer y animaciones de estado.
        /// </summary>
        public SaraUI_Button() {
            _animator = new SaraAnimator();
            _animator.Completed += Animator_Completed;
            _animator.Canceled += Animator_Canceled;
            _animator.StateChanged += Animator_StateChanged;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.UserPaint,
                true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            Size = new Size(150, 40);
            BackColor = Color.MediumSlateBlue;
            ForeColor = Color.White;
            AccessibleRole = AccessibleRole.PushButton;

            _initialized = true;
            _visualState = DetermineVisualState();
            _displayAppearance = ResolveAppearance(_visualState);
            UpdateControlRegion();
        }

        /// <summary>Se produce cuando cambia el estado visual del botón.</summary>
        public event EventHandler? VisualStateChanged;

        /// <summary>Se produce cuando finaliza una transición visual.</summary>
        public event EventHandler? AnimationCompleted;

        /// <summary>Se produce cuando una transición visual se cancela o es reemplazada.</summary>
        public event EventHandler? AnimationCanceled;

        /// <summary>Se produce cuando cambia el estado del motor de animación interno.</summary>
        public event EventHandler? AnimationStateChanged;

        /// <summary>Obtiene o establece el grosor del borde, expresado en píxeles.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(0)]
        public int BorderSize {
            get => _borderSize;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(
                        nameof(BorderSize), value, "El grosor del borde no puede ser negativo.");
                }

                if(_borderSize == value) {
                    return;
                }

                _borderSize = value;
                RefreshCurrentAppearance();
            }
        }

        /// <summary>
        /// Obtiene o establece el radio de las esquinas. El radio efectivo se adapta al tamaño del control.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(20)]
        public int BorderRadius {
            get => _borderRadius;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(
                        nameof(BorderRadius), value, "El radio del borde no puede ser negativo.");
                }

                if(_borderRadius == value) {
                    return;
                }

                _borderRadius = value;
                UpdateControlRegion();
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el color del borde en el estado normal.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "PaleVioletRed")]
        public Color BorderColor {
            get => _borderColor;
            set {
                if(_borderColor == value) {
                    return;
                }

                _borderColor = value;
                RefreshCurrentAppearance();
            }
        }

        /// <summary>Obtiene o establece el nombre del icono. Use <c>None</c> para ocultarlo.</summary>
        [Category("Sara UI Design")]
        [DefaultValue("None")]
        [TypeConverter(typeof(IconNameConverter))]
        public string IconName {
            get => _iconName;
            set {
                string normalizedValue = string.IsNullOrWhiteSpace(value) ? "None" : value.Trim();
                if(string.Equals(_iconName, normalizedValue, StringComparison.Ordinal)) {
                    return;
                }

                _iconName = normalizedValue;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el tamaño solicitado del icono. El tamaño efectivo se limita al área disponible.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor menor que uno.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(16)]
        public int IconSize {
            get => _iconSize;
            set {
                if(value < 1) {
                    throw new ArgumentOutOfRangeException(
                        nameof(IconSize), value, "El tamaño del icono debe ser mayor que cero.");
                }

                if(_iconSize == value) {
                    return;
                }

                _iconSize = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el espacio entre el icono y el texto.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(8)]
        public int IconPadding {
            get => _iconPadding;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(
                        nameof(IconPadding), value, "La separación del icono no puede ser negativa.");
                }

                if(_iconPadding == value) {
                    return;
                }

                _iconPadding = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el color del icono. <see cref="Color.Empty"/> hace que siga el color del texto.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color IconColor {
            get => _iconColor;
            set {
                if(_iconColor == value) {
                    return;
                }

                _iconColor = value;
                RefreshCurrentAppearance();
            }
        }

        /// <summary>Obtiene o establece la ubicación horizontal del icono.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar una ubicación desconocida.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(SaraIconLocation.Left)]
        public SaraIconLocation IconLocation {
            get => _iconLocation;
            set {
                EnsureDefined(value, nameof(IconLocation));
                if(_iconLocation == value) {
                    return;
                }

                _iconLocation = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el estilo utilizado por la biblioteca de iconos.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un estilo desconocido.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(SaraUI_IconLibrary.SaraIconStyle.Outline)]
        public SaraUI_IconLibrary.SaraIconStyle IconStyle {
            get => _iconStyle;
            set {
                EnsureDefined(value, nameof(IconStyle));
                if(_iconStyle == value) {
                    return;
                }

                _iconStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el fondo durante hover. <see cref="Color.Empty"/> genera una variante clara.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color HoverBackColor {
            get => _hoverBackColor;
            set => SetAppearanceColor(ref _hoverBackColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del texto durante hover. <see cref="Color.Empty"/> conserva ForeColor.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color HoverForeColor {
            get => _hoverForeColor;
            set => SetAppearanceColor(ref _hoverForeColor, value);
        }

        /// <summary>
        /// Obtiene o establece el fondo al presionar. <see cref="Color.Empty"/> genera una variante oscura.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color PressedBackColor {
            get => _pressedBackColor;
            set => SetAppearanceColor(ref _pressedBackColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del texto al presionar. <see cref="Color.Empty"/> conserva ForeColor.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color PressedForeColor {
            get => _pressedForeColor;
            set => SetAppearanceColor(ref _pressedForeColor, value);
        }

        /// <summary>
        /// Obtiene o establece el borde con foco. <see cref="Color.Empty"/> conserva BorderColor.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color FocusBorderColor {
            get => _focusBorderColor;
            set => SetAppearanceColor(ref _focusBorderColor, value);
        }

        /// <summary>
        /// Obtiene o establece el fondo deshabilitado. <see cref="Color.Empty"/> genera un tono atenuado.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledBackColor {
            get => _disabledBackColor;
            set => SetAppearanceColor(ref _disabledBackColor, value);
        }

        /// <summary>
        /// Obtiene o establece el texto e icono deshabilitados. <see cref="Color.Empty"/> usa GrayText.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledForeColor {
            get => _disabledForeColor;
            set => SetAppearanceColor(ref _disabledForeColor, value);
        }

        /// <summary>
        /// Obtiene o establece el borde deshabilitado. <see cref="Color.Empty"/> genera un tono atenuado.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledBorderColor {
            get => _disabledBorderColor;
            set => SetAppearanceColor(ref _disabledBorderColor, value);
        }

        /// <summary>Obtiene o establece si los cambios de estado deben interpolar sus colores.</summary>
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

        /// <summary>Obtiene o establece la duración de una transición, en milisegundos.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(140)]
        public int AnimationDuration {
            get => _animationDuration;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(
                        nameof(AnimationDuration), value, "La duración de la animación no puede ser negativa.");
                }

                _animationDuration = value;
            }
        }

        /// <summary>Obtiene o establece el intervalo entre actualizaciones de la animación.</summary>
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
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar una curva desconocida.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(SaraEasing.EaseOutQuad)]
        public SaraEasing AnimationEasing {
            get => _animationEasing;
            set {
                EnsureDefined(value, nameof(AnimationEasing));
                _animationEasing = value;
            }
        }

        /// <summary>Obtiene o establece si se dibuja una guía para el foco de teclado.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool ShowFocusBorder {
            get => _showFocusBorder;
            set {
                if(_showFocusBorder == value) {
                    return;
                }

                _showFocusBorder = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el desplazamiento del contenido mientras se presiona.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(1)]
        public int PressedContentOffset {
            get => _pressedContentOffset;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(
                        nameof(PressedContentOffset), value, "El desplazamiento no puede ser negativo.");
                }

                if(_pressedContentOffset == value) {
                    return;
                }

                _pressedContentOffset = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene el estado visual actual del botón.</summary>
        [Browsable(false)]
        public SaraButtonVisualState VisualState => _visualState;

        /// <summary>Obtiene el estado del motor de animación interno.</summary>
        [Browsable(false)]
        public SaraAnimationState AnimationState => _animator.State;

        /// <summary>Pausa la transición visual activa.</summary>
        /// <returns><see langword="true"/> si la transición cambió al estado pausado.</returns>
        public bool PauseAnimation() => _animator.Pause();

        /// <summary>Reanuda una transición visual pausada.</summary>
        /// <returns><see langword="true"/> si la transición volvió a ejecutarse.</returns>
        public bool ResumeAnimation() => _animator.Resume();

        /// <summary>Detiene la transición visual activa conservando la apariencia alcanzada.</summary>
        /// <returns><see langword="true"/> si se detuvo una transición activa.</returns>
        public bool StopAnimation() => _animator.Stop();

        /// <inheritdoc/>
        protected override void OnBackColorChanged(EventArgs e) {
            base.OnBackColorChanged(e);
            RefreshCurrentAppearance();
        }

        /// <inheritdoc/>
        protected override void OnForeColorChanged(EventArgs e) {
            base.OnForeColorChanged(e);
            RefreshCurrentAppearance();
        }

        /// <inheritdoc/>
        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            if(!Enabled) {
                _isPressed = false;
            }
            UpdateVisualState();
        }

        /// <inheritdoc/>
        protected override void OnFontChanged(EventArgs e) {
            base.OnFontChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnGotFocus(EventArgs e) {
            base.OnGotFocus(e);
            UpdateVisualState();
        }

        /// <inheritdoc/>
        protected override void OnLostFocus(EventArgs e) {
            base.OnLostFocus(e);
            _isPressed = false;
            UpdateVisualState();
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
        protected override void OnMouseDown(MouseEventArgs mevent) {
            base.OnMouseDown(mevent);
            if(mevent.Button == MouseButtons.Left) {
                _isPressed = true;
                UpdateVisualState();
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(MouseEventArgs mevent) {
            base.OnMouseUp(mevent);
            if(mevent.Button == MouseButtons.Left) {
                _isPressed = false;
                UpdateVisualState();
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseCaptureChanged(EventArgs e) {
            base.OnMouseCaptureChanged(e);
            if(!Capture && _isPressed) {
                _isPressed = false;
                UpdateVisualState();
            }
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs kevent) {
            base.OnKeyDown(kevent);
            if(kevent.KeyCode == Keys.Space || kevent.KeyCode == Keys.Enter) {
                _isPressed = true;
                UpdateVisualState();
            }
        }

        /// <inheritdoc/>
        protected override void OnKeyUp(KeyEventArgs kevent) {
            base.OnKeyUp(kevent);
            if(kevent.KeyCode == Keys.Space || kevent.KeyCode == Keys.Enter) {
                _isPressed = false;
                UpdateVisualState();
            }
        }

        /// <inheritdoc/>
        protected override void OnPaddingChanged(EventArgs e) {
            base.OnPaddingChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnParentBackColorChanged(EventArgs e) {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            UpdateControlRegion();
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnRightToLeftChanged(EventArgs e) {
            base.OnRightToLeftChanged(e);
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

        /// <summary>Dibuja la superficie, el borde, el icono, el texto y la guía de foco.</summary>
        /// <param name="pevent">Datos del evento de dibujo.</param>
        protected override void OnPaint(PaintEventArgs pevent) {
            if(ClientSize.Width <= 0 || ClientSize.Height <= 0) {
                return;
            }

            Graphics graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            RectangleF surfaceRectangle = new RectangleF(0f, 0f, Width, Height);
            float effectiveRadius = GetEffectiveRadius(surfaceRectangle);

            using(GraphicsPath surfacePath = CreateFigurePath(surfaceRectangle, effectiveRadius))
            using(SolidBrush backgroundBrush = new SolidBrush(_displayAppearance.BackColor)) {
                graphics.FillPath(backgroundBrush, surfacePath);
            }

            DrawBorder(graphics, surfaceRectangle, effectiveRadius);
            DrawContent(graphics);

            if(_showFocusBorder && Focused && ShowFocusCues && Enabled) {
                Rectangle focusRectangle = Rectangle.Inflate(ClientRectangle, -4, -4);
                if(focusRectangle.Width > 0 && focusRectangle.Height > 0) {
                    ControlPaint.DrawFocusRectangle(
                        graphics,
                        focusRectangle,
                        _displayAppearance.ForeColor,
                        _displayAppearance.BackColor);
                }
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

                Region? previousRegion = Region;
                Region = null;
                previousRegion?.Dispose();

                VisualStateChanged = null;
                AnimationCompleted = null;
                AnimationCanceled = null;
                AnimationStateChanged = null;
            }

            base.Dispose(disposing);
        }

        private void UpdateVisualState(bool animate = true) {
            if(!_initialized || _disposed) {
                return;
            }

            SaraButtonVisualState nextState = DetermineVisualState();
            bool stateChanged = nextState != _visualState;
            if(!stateChanged) {
                return;
            }

            _visualState = nextState;
            VisualStateChanged?.Invoke(this, EventArgs.Empty);
            TransitionToAppearance(ResolveAppearance(nextState), animate);
        }

        private void RefreshCurrentAppearance() {
            if(!_initialized || _disposed) {
                return;
            }

            StopAnimatorIfActive();
            ApplyAppearance(ResolveAppearance(_visualState));
        }

        private void SetAppearanceColor(ref Color field, Color value) {
            if(field == value) {
                return;
            }

            field = value;
            RefreshCurrentAppearance();
        }

        private void TransitionToAppearance(ButtonAppearance destination, bool animate) {
            if(!animate || !_useAnimations || _animationDuration == 0 || IsInDesignMode || !IsHandleCreated) {
                StopAnimatorIfActive();
                ApplyAppearance(destination);
                return;
            }

            ButtonAppearance origin = _displayAppearance;
            _animator.Start(
                0f,
                1f,
                progress => {
                    _displayAppearance = ButtonAppearance.Interpolate(origin, destination, progress);
                    Invalidate();
                },
                new SaraAnimationOptions {
                    Duration = _animationDuration,
                    Easing = _animationEasing,
                    FrameInterval = _animationFrameInterval
                });
        }

        private void ApplyAppearance(ButtonAppearance appearance) {
            _displayAppearance = appearance;
            Invalidate();
        }

        private SaraButtonVisualState DetermineVisualState() {
            if(!Enabled) {
                return SaraButtonVisualState.Disabled;
            }

            if(_isPressed) {
                return SaraButtonVisualState.Pressed;
            }

            if(_isMouseOver) {
                return SaraButtonVisualState.Hovered;
            }

            return Focused ? SaraButtonVisualState.Focused : SaraButtonVisualState.Normal;
        }

        private ButtonAppearance ResolveAppearance(SaraButtonVisualState state) {
            Color normalBackColor = BackColor;
            Color normalForeColor = ForeColor;
            Color normalIconColor = _iconColor.IsEmpty ? normalForeColor : _iconColor;

            switch(state) {
                case SaraButtonVisualState.Hovered:
                    Color hoverForeColor = ResolveOptionalColor(_hoverForeColor, normalForeColor);
                    return new ButtonAppearance(
                        ResolveOptionalColor(_hoverBackColor, Blend(normalBackColor, Color.White, 0.12f)),
                        hoverForeColor,
                        _borderColor,
                        _iconColor.IsEmpty ? hoverForeColor : _iconColor);

                case SaraButtonVisualState.Pressed:
                    Color pressedForeColor = ResolveOptionalColor(_pressedForeColor, normalForeColor);
                    return new ButtonAppearance(
                        ResolveOptionalColor(_pressedBackColor, Blend(normalBackColor, Color.Black, 0.14f)),
                        pressedForeColor,
                        _borderColor,
                        _iconColor.IsEmpty ? pressedForeColor : _iconColor);

                case SaraButtonVisualState.Focused:
                    return new ButtonAppearance(
                        normalBackColor,
                        normalForeColor,
                        ResolveOptionalColor(_focusBorderColor, _borderColor),
                        normalIconColor);

                case SaraButtonVisualState.Disabled:
                    Color disabledBackColor = ResolveOptionalColor(
                        _disabledBackColor,
                        Blend(normalBackColor, SystemColors.Control, 0.58f));
                    Color disabledForeColor = ResolveOptionalColor(_disabledForeColor, SystemColors.GrayText);
                    return new ButtonAppearance(
                        disabledBackColor,
                        disabledForeColor,
                        ResolveOptionalColor(
                            _disabledBorderColor,
                            Blend(_borderColor, SystemColors.ControlDark, 0.45f)),
                        disabledForeColor);

                default:
                    return new ButtonAppearance(
                        normalBackColor,
                        normalForeColor,
                        _borderColor,
                        normalIconColor);
            }
        }

        private void DrawBorder(Graphics graphics, RectangleF surfaceRectangle, float effectiveRadius) {
            if(_borderSize <= 0) {
                return;
            }

            float maximumBorder = Math.Min(surfaceRectangle.Width, surfaceRectangle.Height) / 2f;
            float effectiveBorderSize = Math.Min(_borderSize, maximumBorder);
            float inset = effectiveBorderSize / 2f;
            RectangleF borderRectangle = RectangleF.Inflate(surfaceRectangle, -inset, -inset);

            if(borderRectangle.Width <= 0f || borderRectangle.Height <= 0f) {
                return;
            }

            float borderRadius = Math.Max(0f, effectiveRadius - inset);
            using(GraphicsPath borderPath = CreateFigurePath(borderRectangle, borderRadius))
            using(Pen borderPen = new Pen(_displayAppearance.BorderColor, effectiveBorderSize)) {
                borderPen.Alignment = PenAlignment.Center;
                graphics.DrawPath(borderPen, borderPath);
            }
        }

        private void DrawContent(Graphics graphics) {
            Rectangle contentBounds = Rectangle.FromLTRB(
                Padding.Left,
                Padding.Top,
                Width - Padding.Right,
                Height - Padding.Bottom);

            if(contentBounds.Width <= 0 || contentBounds.Height <= 0) {
                return;
            }

            if(_visualState == SaraButtonVisualState.Pressed && _pressedContentOffset > 0) {
                contentBounds.Offset(_pressedContentOffset, _pressedContentOffset);
            }

            bool hasIcon = !string.Equals(_iconName, "None", StringComparison.OrdinalIgnoreCase);
            bool hasText = !string.IsNullOrEmpty(Text);
            int effectiveIconSize = hasIcon ? Math.Min(_iconSize, contentBounds.Height) : 0;
            int effectiveGap = hasIcon && hasText ? _iconPadding : 0;
            int availableTextWidth = Math.Max(0, contentBounds.Width - effectiveIconSize - effectiveGap);
            TextFormatFlags measurementFlags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
            Size measuredText = hasText
                ? TextRenderer.MeasureText(Text, Font, new Size(availableTextWidth, contentBounds.Height), measurementFlags)
                : Size.Empty;
            int textWidth = Math.Min(measuredText.Width, availableTextWidth);
            int groupWidth = effectiveIconSize + effectiveGap + textWidth;
            int groupHeight = Math.Max(effectiveIconSize, measuredText.Height);
            Rectangle groupBounds = AlignRectangle(contentBounds, new Size(groupWidth, groupHeight), TextAlign, RightToLeft);
            int iconX;
            int textX;

            if(hasIcon && _iconLocation == SaraIconLocation.Left) {
                iconX = groupBounds.Left;
                textX = iconX + effectiveIconSize + effectiveGap;
            } else {
                textX = groupBounds.Left;
                iconX = textX + textWidth + effectiveGap;
            }

            if(hasIcon && effectiveIconSize > 0) {
                int iconY = groupBounds.Top + ((groupBounds.Height - effectiveIconSize) / 2);
                SaraUI_IconLibrary.DrawIcon(
                    _iconName,
                    graphics,
                    new Rectangle(iconX, iconY, effectiveIconSize, effectiveIconSize),
                    _displayAppearance.IconColor,
                    _iconStyle);
            }

            if(hasText && textWidth > 0) {
                Rectangle textRectangle = new Rectangle(textX, groupBounds.Top, textWidth, groupBounds.Height);
                TextRenderer.DrawText(
                    graphics,
                    Text,
                    Font,
                    textRectangle,
                    _displayAppearance.ForeColor,
                    GetTextFormatFlags());
            }
        }

        private TextFormatFlags GetTextFormatFlags() {
            TextFormatFlags flags =
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine;

            if(AutoEllipsis) {
                flags |= TextFormatFlags.EndEllipsis;
            }

            if(!UseMnemonic) {
                flags |= TextFormatFlags.NoPrefix;
            } else if(!ShowKeyboardCues) {
                flags |= TextFormatFlags.HidePrefix;
            }

            if(RightToLeft == RightToLeft.Yes) {
                flags |= TextFormatFlags.RightToLeft;
            }

            return flags;
        }

        private void UpdateControlRegion() {
            if(!_initialized || _disposed || ClientSize.Width <= 0 || ClientSize.Height <= 0) {
                return;
            }

            Region? nextRegion = null;
            if(_borderRadius > 0) {
                RectangleF bounds = new RectangleF(0f, 0f, Width, Height);
                using(GraphicsPath path = CreateFigurePath(bounds, GetEffectiveRadius(bounds))) {
                    nextRegion = new Region(path);
                }
            }

            Region? previousRegion = Region;
            Region = nextRegion;
            previousRegion?.Dispose();
        }

        private float GetEffectiveRadius(RectangleF rectangle) {
            float maximumRadius = Math.Min(rectangle.Width, rectangle.Height) / 2f;
            return Math.Max(0f, Math.Min(_borderRadius, maximumRadius));
        }

        private static GraphicsPath CreateFigurePath(RectangleF rectangle, float radius) {
            GraphicsPath path = new GraphicsPath();
            if(radius <= 0f) {
                path.AddRectangle(rectangle);
                path.CloseFigure();
                return path;
            }

            float diameter = Math.Min(radius * 2f, Math.Min(rectangle.Width, rectangle.Height));
            RectangleF arc = new RectangleF(rectangle.Location, new SizeF(diameter, diameter));

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

        private static Rectangle AlignRectangle(
            Rectangle bounds,
            Size contentSize,
            ContentAlignment alignment,
            RightToLeft rightToLeft) {
            bool alignLeft = alignment == ContentAlignment.TopLeft ||
                alignment == ContentAlignment.MiddleLeft ||
                alignment == ContentAlignment.BottomLeft;
            bool alignRight = alignment == ContentAlignment.TopRight ||
                alignment == ContentAlignment.MiddleRight ||
                alignment == ContentAlignment.BottomRight;

            if(rightToLeft == RightToLeft.Yes) {
                bool previousLeft = alignLeft;
                alignLeft = alignRight;
                alignRight = previousLeft;
            }

            int x = alignLeft
                ? bounds.Left
                : alignRight
                    ? bounds.Right - contentSize.Width
                    : bounds.Left + ((bounds.Width - contentSize.Width) / 2);
            int y = alignment == ContentAlignment.TopLeft ||
                    alignment == ContentAlignment.TopCenter ||
                    alignment == ContentAlignment.TopRight
                ? bounds.Top
                : alignment == ContentAlignment.BottomLeft ||
                  alignment == ContentAlignment.BottomCenter ||
                  alignment == ContentAlignment.BottomRight
                    ? bounds.Bottom - contentSize.Height
                    : bounds.Top + ((bounds.Height - contentSize.Height) / 2);

            return new Rectangle(x, y, contentSize.Width, contentSize.Height);
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

        private readonly struct ButtonAppearance {
            public ButtonAppearance(
                Color backColor,
                Color foreColor,
                Color borderColor,
                Color iconColor) {
                BackColor = backColor;
                ForeColor = foreColor;
                BorderColor = borderColor;
                IconColor = iconColor;
            }

            public Color BackColor { get; }

            public Color ForeColor { get; }

            public Color BorderColor { get; }

            public Color IconColor { get; }

            public static ButtonAppearance Interpolate(
                ButtonAppearance origin,
                ButtonAppearance destination,
                float progress) {
                return new ButtonAppearance(
                    InterpolateColor(origin.BackColor, destination.BackColor, progress),
                    InterpolateColor(origin.ForeColor, destination.ForeColor, progress),
                    InterpolateColor(origin.BorderColor, destination.BorderColor, progress),
                    InterpolateColor(origin.IconColor, destination.IconColor, progress));
            }
        }
    }
}

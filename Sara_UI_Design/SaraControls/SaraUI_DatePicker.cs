using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Sara_UI_Design.Animations;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Selector de fecha que conserva el calendario y la navegación nativos de Windows Forms,
    /// con una superficie redondeada y estados visuales animados.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent(nameof(ValueChanged))]
    [DefaultProperty(nameof(Value))]
    public class SaraUI_DatePicker:DateTimePicker {
        private const int DtmCloseMonthCalendar = 0x100D;
        private const int VirtualKeyF4 = 0x73;
        private const int WmKeyDown = 0x0100;
        private const int WmKeyUp = 0x0101;

        /// <summary>Describe el estado de interacción que gobierna la apariencia del selector.</summary>
        public enum SaraDatePickerVisualState {
            /// <summary>El selector no tiene una interacción activa.</summary>
            Normal,

            /// <summary>El puntero se encuentra sobre el selector.</summary>
            Hovered,

            /// <summary>El selector se está presionando con el ratón o el teclado.</summary>
            Pressed,

            /// <summary>El selector tiene el foco de teclado.</summary>
            Focused,

            /// <summary>El calendario desplegable se encuentra abierto.</summary>
            DroppedDown,

            /// <summary>El selector está deshabilitado.</summary>
            Disabled
        }

        private readonly SaraAnimator _animator;
        private Color _skinColor = Color.MediumSlateBlue;
        private Color _textColor = Color.White;
        private Color _borderColor = Color.PaleVioletRed;
        private Color _hoverSkinColor = Color.Empty;
        private Color _pressedSkinColor = Color.Empty;
        private Color _droppedDownSkinColor = Color.Empty;
        private Color _focusBorderColor = Color.HotPink;
        private Color _disabledSkinColor = Color.Empty;
        private Color _disabledTextColor = Color.Empty;
        private Color _disabledBorderColor = Color.Empty;
        private Color _iconColor = Color.Empty;
        private int _borderSize;
        private int _borderRadius = 12;
        private int _iconSize = 16;
        private ContentAlignment _textAlign = ContentAlignment.MiddleLeft;
        private bool _showFocusBorder = true;
        private bool _animationEnabled = true;
        private int _animationDuration = 180;
        private int _animationFrameInterval = 15;
        private SaraEasing _animationEasing = SaraEasing.EaseOutCubic;
        private bool _isMouseOver;
        private bool _isPressed;
        private bool _isDropDownOpen;
        private bool _initialized;
        private bool _disposingResources;
        private SaraDatePickerVisualState _visualState;
        private DatePickerAppearance _displayAppearance;
        private DatePickerAppearance _targetAppearance;

        /// <summary>
        /// Inicializa un selector de fecha que hereda la fuente de su contenedor y conserva
        /// el comportamiento nativo del calendario.
        /// </summary>
        public SaraUI_DatePicker() {
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

            MinimumSize = new Size(0, 35);
            Size = new Size(200, 40);
            AccessibleRole = AccessibleRole.DropList;

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

        /// <summary>Se produce cuando una animación activa se detiene o se reemplaza.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationCanceled;

        /// <summary>Se produce cuando cambia el estado del motor de animación interno.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationStateChanged;

        /// <summary>Obtiene o establece el color de fondo principal del control.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "MediumSlateBlue")]
        public Color SkinColor {
            get => _skinColor;
            set => SetAppearanceColor(ref _skinColor, value);
        }

        /// <summary>Obtiene o establece el color del texto.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "White")]
        public Color TextColor {
            get => _textColor;
            set => SetAppearanceColor(ref _textColor, value);
        }

        /// <summary>Obtiene o establece el color del borde decorativo.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "PaleVioletRed")]
        public Color BorderColor {
            get => _borderColor;
            set => SetAppearanceColor(ref _borderColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color de fondo bajo el puntero.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante del color principal.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color HoverSkinColor {
            get => _hoverSkinColor;
            set => SetAppearanceColor(ref _hoverSkinColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color de fondo mientras se presiona el selector.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante del color principal.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color PressedSkinColor {
            get => _pressedSkinColor;
            set => SetAppearanceColor(ref _pressedSkinColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color de fondo mientras el calendario está abierto.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante del color principal.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DroppedDownSkinColor {
            get => _droppedDownSkinColor;
            set => SetAppearanceColor(ref _droppedDownSkinColor, value);
        }

        /// <summary>Obtiene o establece el color utilizado para indicar el foco y el calendario abierto.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "HotPink")]
        public Color FocusBorderColor {
            get => _focusBorderColor;
            set => SetAppearanceColor(ref _focusBorderColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color de fondo deshabilitado.
        /// <see cref="Color.Empty"/> calcula automáticamente un color atenuado.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledSkinColor {
            get => _disabledSkinColor;
            set => SetAppearanceColor(ref _disabledSkinColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del texto deshabilitado.
        /// <see cref="Color.Empty"/> utiliza un color del sistema.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledTextColor {
            get => _disabledTextColor;
            set => SetAppearanceColor(ref _disabledTextColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del borde deshabilitado.
        /// <see cref="Color.Empty"/> calcula automáticamente un color atenuado.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledBorderColor {
            get => _disabledBorderColor;
            set => SetAppearanceColor(ref _disabledBorderColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del icono. <see cref="Color.Empty"/> utiliza
        /// el color de texto correspondiente al estado actual.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color IconColor {
            get => _iconColor;
            set => SetAppearanceColor(ref _iconColor, value);
        }

        /// <summary>Obtiene o establece el grosor del borde en píxeles. Use cero para ocultarlo.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(0)]
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

        /// <summary>Obtiene o establece el radio de las esquinas en píxeles.</summary>
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
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el lado del icono, expresado en píxeles.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor menor que ocho.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(16)]
        public int IconSize {
            get => _iconSize;
            set {
                if(value < 8) {
                    throw new ArgumentOutOfRangeException(
                        nameof(IconSize),
                        value,
                        "El tamaño del icono debe ser de al menos ocho píxeles.");
                }

                if(_iconSize == value) {
                    return;
                }

                _iconSize = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece la alineación de la fecha dentro del área disponible.</summary>
        /// <exception cref="InvalidEnumArgumentException">Se produce al asignar una alineación desconocida.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment TextAlign {
            get => _textAlign;
            set {
                if(!Enum.IsDefined(typeof(ContentAlignment), value)) {
                    throw new InvalidEnumArgumentException(
                        nameof(TextAlign),
                        (int)value,
                        typeof(ContentAlignment));
                }

                if(_textAlign == value) {
                    return;
                }

                _textAlign = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece si debe mostrarse una guía de foco aunque el borde normal esté oculto.</summary>
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

        /// <summary>Obtiene o establece si los cambios de interacción deben animarse.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool AnimationEnabled {
            get => _animationEnabled;
            set {
                if(_animationEnabled == value) {
                    return;
                }

                _animationEnabled = value;

                if(value) {
                    UpdateVisualState(animate: true);
                } else {
                    ApplyTargetImmediately();
                }
            }
        }

        /// <summary>Obtiene o establece la duración de una transición, expresada en milisegundos.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(180)]
        public int AnimationDuration {
            get => _animationDuration;
            set {
                EnsureNonNegative(value, nameof(AnimationDuration));

                if(_animationDuration == value) {
                    return;
                }

                _animationDuration = value;
                RestartActiveAnimation();
            }
        }

        /// <summary>Obtiene o establece el intervalo solicitado entre fotogramas, en milisegundos.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor menor que uno.</exception>
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

                if(_animationFrameInterval == value) {
                    return;
                }

                _animationFrameInterval = value;
                RestartActiveAnimation();
            }
        }

        /// <summary>Obtiene o establece la curva utilizada por las transiciones.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar una curva desconocida.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(SaraEasing.EaseOutCubic)]
        public SaraEasing AnimationEasing {
            get => _animationEasing;
            set {
                if(!Enum.IsDefined(typeof(SaraEasing), value)) {
                    throw new ArgumentOutOfRangeException(
                        nameof(AnimationEasing),
                        value,
                        "La curva de animación indicada no es compatible.");
                }

                if(_animationEasing == value) {
                    return;
                }

                _animationEasing = value;
                RestartActiveAnimation();
            }
        }

        /// <summary>Obtiene el estado visual de interacción actual.</summary>
        [Browsable(false)]
        public SaraDatePickerVisualState VisualState => _visualState;

        /// <summary>Obtiene el estado actual del motor de animación interno.</summary>
        [Browsable(false)]
        public SaraAnimationState AnimationState => _animator.State;

        /// <summary>
        /// Obtiene el progreso visual del calendario: cero representa cerrado y uno representa abierto.
        /// </summary>
        [Browsable(false)]
        public float DisplayedDropDownProgress => _displayAppearance.DropDownProgress;

        /// <summary>Obtiene un valor que indica si el calendario desplegable se encuentra abierto.</summary>
        [Browsable(false)]
        public bool IsDropDownOpen => _isDropDownOpen;

        /// <summary>Abre el calendario nativo y transfiere el foco al selector.</summary>
        /// <returns>
        /// <see langword="true"/> si el calendario quedó abierto; <see langword="false"/>
        /// si el control está deshabilitado, aún no tiene identificador de ventana o utiliza
        /// botones incrementales.
        /// </returns>
        public bool OpenDropDown() {
            if(!Enabled || ShowUpDown || !IsHandleCreated || _disposingResources || IsDisposed) {
                return false;
            }

            if(_isDropDownOpen) {
                return true;
            }

            Focus();
            SendMessageW(Handle, WmKeyDown, new IntPtr(VirtualKeyF4), new IntPtr(1));
            SendMessageW(
                Handle,
                WmKeyUp,
                new IntPtr(VirtualKeyF4),
                new IntPtr(unchecked((int)0xC0000001)));
            return _isDropDownOpen;
        }

        /// <summary>Cierra el calendario nativo si se encuentra abierto.</summary>
        /// <returns><see langword="true"/> si se solicitó el cierre de un calendario abierto.</returns>
        public bool CloseDropDown() {
            if(!_isDropDownOpen || !IsHandleCreated || _disposingResources || IsDisposed) {
                return false;
            }

            SendMessageW(Handle, DtmCloseMonthCalendar, IntPtr.Zero, IntPtr.Zero);
            return !_isDropDownOpen;
        }

        /// <summary>Pausa la transición visual activa conservando su progreso.</summary>
        /// <returns><see langword="true"/> si la animación cambió al estado pausado.</returns>
        public bool PauseAnimation() => _animator.Pause();

        /// <summary>Reanuda una transición pausada desde el progreso conservado.</summary>
        /// <returns><see langword="true"/> si la animación volvió a ejecutarse.</returns>
        public bool ResumeAnimation() => _animator.Resume();

        /// <summary>Detiene la transición y muestra inmediatamente el estado lógico actual.</summary>
        /// <returns><see langword="true"/> si se detuvo una animación activa.</returns>
        public bool StopAnimation() {
            bool stopped = _animator.Stop();
            _targetAppearance = ResolveAppearance(DetermineVisualState());
            _displayAppearance = _targetAppearance;
            Invalidate();
            return stopped;
        }

        /// <inheritdoc/>
        public override Size GetPreferredSize(Size proposedSize) {
            Size textSize = TextRenderer.MeasureText(
                Text,
                Font,
                Size.Empty,
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            int accessoryWidth = _iconSize + 24 + (ShowCheckBox ? 22 : 0);
            int preferredWidth = textSize.Width + accessoryWidth + Padding.Horizontal + 16;
            int preferredHeight = Math.Max(35, Math.Max(textSize.Height, _iconSize) + Padding.Vertical + 12);
            return new Size(preferredWidth, preferredHeight);
        }

        /// <inheritdoc/>
        protected override void OnDropDown(EventArgs eventargs) {
            _isDropDownOpen = true;
            _isPressed = false;
            UpdateVisualState(animate: true);
            base.OnDropDown(eventargs);
        }

        /// <inheritdoc/>
        protected override void OnCloseUp(EventArgs eventargs) {
            _isDropDownOpen = false;
            _isPressed = false;
            UpdateVisualState(animate: true);
            base.OnCloseUp(eventargs);
        }

        /// <inheritdoc/>
        protected override void OnMouseEnter(EventArgs e) {
            base.OnMouseEnter(e);
            _isMouseOver = true;
            UpdateVisualState(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            _isMouseOver = false;
            _isPressed = false;
            UpdateVisualState(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(MouseEventArgs e) {
            if(e.Button == MouseButtons.Left && Enabled) {
                _isPressed = true;
                UpdateVisualState(animate: true);
            }

            base.OnMouseDown(e);
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);

            if(e.Button == MouseButtons.Left) {
                _isPressed = false;
                _isMouseOver = ClientRectangle.Contains(e.Location);
                UpdateVisualState(animate: true);
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseCaptureChanged(EventArgs e) {
            base.OnMouseCaptureChanged(e);

            if(_isPressed && Control.MouseButtons == MouseButtons.None) {
                _isPressed = false;
                UpdateVisualState(animate: true);
            }
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e) {
            if(Enabled && (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)) {
                _isPressed = true;
                UpdateVisualState(animate: true);
            }

            base.OnKeyDown(e);
        }

        /// <inheritdoc/>
        protected override void OnKeyUp(KeyEventArgs e) {
            base.OnKeyUp(e);

            if(e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) {
                _isPressed = false;
                UpdateVisualState(animate: true);
            }
        }

        /// <inheritdoc/>
        protected override void OnGotFocus(EventArgs e) {
            base.OnGotFocus(e);
            UpdateVisualState(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnLostFocus(EventArgs e) {
            base.OnLostFocus(e);
            _isPressed = false;
            UpdateVisualState(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);

            if(!Enabled) {
                _isPressed = false;
                _isMouseOver = false;
                _isDropDownOpen = false;
            }

            UpdateVisualState(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            _isDropDownOpen = false;
            UpdateVisualState(animate: false);
        }

        /// <inheritdoc/>
        protected override void OnHandleDestroyed(EventArgs e) {
            _isDropDownOpen = false;
            _isPressed = false;

            if(!_disposingResources && (_animator.IsRunning || _animator.IsPaused)) {
                _animator.Stop();
            }

            base.OnHandleDestroyed(e);
        }

        /// <inheritdoc/>
        protected override void OnVisibleChanged(EventArgs e) {
            base.OnVisibleChanged(e);

            if(_initialized && !_disposingResources && !IsDisposed) {
                UpdateVisualState(animate: false);
            }
        }

        /// <inheritdoc/>
        protected override void OnTextChanged(EventArgs e) {
            base.OnTextChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnValueChanged(EventArgs eventargs) {
            base.OnValueChanged(eventargs);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnFontChanged(EventArgs e) {
            base.OnFontChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnRightToLeftChanged(EventArgs e) {
            base.OnRightToLeftChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnPaddingChanged(EventArgs e) {
            base.OnPaddingChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnPaintBackground(PaintEventArgs pevent) {
            using SolidBrush backgroundBrush = new SolidBrush(ResolveBackgroundColor());
            pevent.Graphics.FillRectangle(backgroundBrush, ClientRectangle);
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e) {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle surfaceBounds = CreateSurfaceBounds();

            if(surfaceBounds.Width <= 0 || surfaceBounds.Height <= 0) {
                return;
            }

            using GraphicsPath surfacePath = CreateRoundedPath(surfaceBounds, _borderRadius);
            using SolidBrush surfaceBrush = new SolidBrush(_displayAppearance.SkinColor);
            graphics.FillPath(surfaceBrush, surfacePath);

            DrawContent(graphics, surfaceBounds);
            DrawBorder(graphics, surfacePath, surfaceBounds);
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

        private SaraDatePickerVisualState DetermineVisualState() {
            if(!Enabled) {
                return SaraDatePickerVisualState.Disabled;
            }

            if(_isDropDownOpen) {
                return SaraDatePickerVisualState.DroppedDown;
            }

            if(_isPressed) {
                return SaraDatePickerVisualState.Pressed;
            }

            if(_isMouseOver) {
                return SaraDatePickerVisualState.Hovered;
            }

            if(Focused) {
                return SaraDatePickerVisualState.Focused;
            }

            return SaraDatePickerVisualState.Normal;
        }

        private DatePickerAppearance ResolveAppearance(SaraDatePickerVisualState state) {
            Color skinColor = _skinColor;
            Color textColor = _textColor;
            Color borderColor = _borderColor;
            float dropDownProgress = _isDropDownOpen ? 1f : 0f;

            switch(state) {
                case SaraDatePickerVisualState.Hovered:
                    skinColor = _hoverSkinColor.IsEmpty
                        ? Blend(skinColor, Color.White, 0.12f)
                        : _hoverSkinColor;
                    break;
                case SaraDatePickerVisualState.Pressed:
                    skinColor = _pressedSkinColor.IsEmpty
                        ? Blend(skinColor, Color.Black, 0.12f)
                        : _pressedSkinColor;
                    break;
                case SaraDatePickerVisualState.Focused:
                    borderColor = ResolveFocusBorderColor();
                    break;
                case SaraDatePickerVisualState.DroppedDown:
                    skinColor = _droppedDownSkinColor.IsEmpty
                        ? Blend(skinColor, Color.Black, 0.08f)
                        : _droppedDownSkinColor;
                    borderColor = ResolveFocusBorderColor();
                    dropDownProgress = 1f;
                    break;
                case SaraDatePickerVisualState.Disabled:
                    Color background = ResolveBackgroundColor();
                    skinColor = _disabledSkinColor.IsEmpty
                        ? Blend(skinColor, background, 0.68f)
                        : _disabledSkinColor;
                    textColor = _disabledTextColor.IsEmpty
                        ? SystemColors.GrayText
                        : _disabledTextColor;
                    borderColor = _disabledBorderColor.IsEmpty
                        ? Blend(_borderColor, background, 0.62f)
                        : _disabledBorderColor;
                    dropDownProgress = 0f;
                    break;
            }

            Color iconColor = _iconColor.IsEmpty ? textColor : _iconColor;

            if(state == SaraDatePickerVisualState.Disabled && _disabledTextColor.IsEmpty) {
                iconColor = SystemColors.GrayText;
            }

            return new DatePickerAppearance(
                skinColor,
                textColor,
                borderColor,
                iconColor,
                dropDownProgress);
        }

        private void UpdateVisualState(bool animate) {
            if(!_initialized || _disposingResources || IsDisposed) {
                return;
            }

            SaraDatePickerVisualState newState = DetermineVisualState();
            bool stateChanged = _visualState != newState;
            _visualState = newState;
            _targetAppearance = ResolveAppearance(newState);

            if(stateChanged) {
                VisualStateChanged?.Invoke(this, EventArgs.Empty);
            }

            if(!animate || !CanAnimate() || _animationDuration == 0 ||
                _displayAppearance.Equals(_targetAppearance)) {
                ApplyTargetImmediately();
                return;
            }

            DatePickerAppearance origin = _displayAppearance;
            DatePickerAppearance destination = _targetAppearance;

            _animator.Start(
                0f,
                1f,
                progress => {
                    _displayAppearance = DatePickerAppearance.Interpolate(origin, destination, progress);
                    Invalidate();
                },
                new SaraAnimationOptions {
                    Duration = _animationDuration,
                    FrameInterval = _animationFrameInterval,
                    Easing = _animationEasing
                });
        }

        private void ApplyTargetImmediately() {
            if(_animator.IsRunning || _animator.IsPaused) {
                _animator.Stop();
            }

            _targetAppearance = ResolveAppearance(DetermineVisualState());
            _displayAppearance = _targetAppearance;
            Invalidate();
        }

        private void RestartActiveAnimation() {
            if(_animator.IsRunning || _animator.IsPaused) {
                UpdateVisualState(animate: true);
            }
        }

        private void RefreshAppearance() {
            if(_initialized) {
                UpdateVisualState(animate: false);
            } else {
                Invalidate();
            }
        }

        private void SetAppearanceColor(ref Color field, Color value) {
            if(field == value) {
                return;
            }

            field = value;
            RefreshAppearance();
        }

        private bool CanAnimate() {
            return _animationEnabled &&
                !IsInDesignMode() &&
                IsHandleCreated &&
                Visible &&
                !_disposingResources &&
                !Disposing &&
                !IsDisposed;
        }

        private bool IsInDesignMode() {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
                (Site?.DesignMode ?? false);
        }

        private Rectangle CreateSurfaceBounds() {
            return new Rectangle(
                0,
                0,
                Math.Max(0, ClientSize.Width - 1),
                Math.Max(0, ClientSize.Height - 1));
        }

        private void DrawContent(Graphics graphics, Rectangle bounds) {
            bool rightToLeft = RightToLeft == RightToLeft.Yes;
            int horizontalInset = Math.Max(8, Math.Min(16, (_borderRadius / 2) + 6));
            int iconSize = Math.Min(_iconSize, Math.Max(8, bounds.Height - 10));
            int iconX = rightToLeft
                ? bounds.Left + horizontalInset
                : bounds.Right - horizontalInset - iconSize;
            Rectangle iconBounds = new Rectangle(
                iconX,
                bounds.Top + ((bounds.Height - iconSize) / 2),
                iconSize,
                iconSize);

            int textLeft = bounds.Left + horizontalInset + Padding.Left;
            int textRight = bounds.Right - horizontalInset - Padding.Right;

            if(rightToLeft) {
                textLeft = iconBounds.Right + 8 + Padding.Left;
            } else {
                textRight = iconBounds.Left - 8 - Padding.Right;
            }

            if(ShowCheckBox) {
                Rectangle checkBounds = CreateCheckBoxBounds(bounds, rightToLeft, horizontalInset);
                DrawCheckBox(graphics, checkBounds);

                if(rightToLeft) {
                    textRight = checkBounds.Left - 6 - Padding.Right;
                } else {
                    textLeft = checkBounds.Right + 6 + Padding.Left;
                }
            }

            Rectangle textBounds = Rectangle.FromLTRB(
                Math.Min(textLeft, textRight),
                bounds.Top + Padding.Top,
                Math.Max(textLeft, textRight),
                bounds.Bottom - Padding.Bottom);

            if(textBounds.Width > 0 && textBounds.Height > 0) {
                TextRenderer.DrawText(
                    graphics,
                    Text,
                    Font,
                    textBounds,
                    _displayAppearance.TextColor,
                    CreateTextFormatFlags(rightToLeft));
            }

            DrawCalendarIcon(graphics, iconBounds);
        }

        private Rectangle CreateCheckBoxBounds(Rectangle bounds, bool rightToLeft, int horizontalInset) {
            int side = Math.Min(14, Math.Max(8, bounds.Height - 12));
            int x = rightToLeft
                ? bounds.Right - horizontalInset - side
                : bounds.Left + horizontalInset;
            return new Rectangle(
                x,
                bounds.Top + ((bounds.Height - side) / 2),
                side,
                side);
        }

        private void DrawCheckBox(Graphics graphics, Rectangle bounds) {
            ButtonState state = Enabled ? ButtonState.Normal : ButtonState.Inactive;

            if(Checked) {
                state |= ButtonState.Checked;
            }

            ControlPaint.DrawCheckBox(graphics, bounds, state);
        }

        private void DrawCalendarIcon(Graphics graphics, Rectangle iconBounds) {
            if(iconBounds.Width <= 0 || iconBounds.Height <= 0) {
                return;
            }

            GraphicsState state = graphics.Save();

            try {
                float angle = 8f * _displayAppearance.DropDownProgress;
                float centerX = iconBounds.Left + (iconBounds.Width / 2f);
                float centerY = iconBounds.Top + (iconBounds.Height / 2f);
                graphics.TranslateTransform(centerX, centerY);
                graphics.RotateTransform(angle);
                graphics.TranslateTransform(-centerX, -centerY);
                string iconName = ShowUpDown ? "ChevronDown" : "Calendar";
                SaraUI_IconLibrary.DrawIcon(
                    iconName,
                    graphics,
                    iconBounds,
                    _displayAppearance.IconColor);
            } finally {
                graphics.Restore(state);
            }
        }

        private void DrawBorder(Graphics graphics, GraphicsPath path, Rectangle bounds) {
            bool highlightState = Enabled &&
                (_visualState == SaraDatePickerVisualState.Focused ||
                 _visualState == SaraDatePickerVisualState.DroppedDown);
            int requestedWidth = _borderSize;

            if(_showFocusBorder && highlightState) {
                requestedWidth = Math.Max(1, requestedWidth);
            }

            if(requestedWidth <= 0) {
                return;
            }

            float maximumWidth = Math.Max(1f, Math.Min(bounds.Width, bounds.Height) / 2f);
            float borderWidth = Math.Min(requestedWidth, maximumWidth);
            using Pen borderPen = new Pen(_displayAppearance.BorderColor, borderWidth) {
                Alignment = PenAlignment.Inset
            };
            graphics.DrawPath(borderPen, path);
        }

        private TextFormatFlags CreateTextFormatFlags(bool rightToLeft) {
            TextFormatFlags flags = TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.SingleLine;

            switch(_textAlign) {
                case ContentAlignment.TopLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.BottomLeft:
                    flags |= TextFormatFlags.Left;
                    break;
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter:
                    flags |= TextFormatFlags.HorizontalCenter;
                    break;
                default:
                    flags |= TextFormatFlags.Right;
                    break;
            }

            switch(_textAlign) {
                case ContentAlignment.TopLeft:
                case ContentAlignment.TopCenter:
                case ContentAlignment.TopRight:
                    flags |= TextFormatFlags.Top;
                    break;
                case ContentAlignment.BottomLeft:
                case ContentAlignment.BottomCenter:
                case ContentAlignment.BottomRight:
                    flags |= TextFormatFlags.Bottom;
                    break;
                default:
                    flags |= TextFormatFlags.VerticalCenter;
                    break;
            }

            if(rightToLeft) {
                flags |= TextFormatFlags.RightToLeft;
            }

            return flags;
        }

        private Color ResolveFocusBorderColor() {
            return _focusBorderColor.IsEmpty ? SystemColors.Highlight : _focusBorderColor;
        }

        private Color ResolveBackgroundColor() {
            return Parent?.BackColor ?? SystemColors.Control;
        }

        private static GraphicsPath CreateRoundedPath(Rectangle bounds, int requestedRadius) {
            GraphicsPath path = new GraphicsPath();

            if(bounds.Width <= 0 || bounds.Height <= 0) {
                return path;
            }

            int radius = Math.Min(
                requestedRadius,
                Math.Max(0, Math.Min(bounds.Width, bounds.Height) / 2));

            if(radius == 0) {
                path.AddRectangle(bounds);
                return path;
            }

            int diameter = radius * 2;
            Rectangle arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
            path.StartFigure();
            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static Color Blend(Color source, Color target, float targetAmount) {
            float amount = Math.Max(0f, Math.Min(1f, targetAmount));
            int alpha = (int)Math.Round(source.A + ((target.A - source.A) * amount));
            int red = (int)Math.Round(source.R + ((target.R - source.R) * amount));
            int green = (int)Math.Round(source.G + ((target.G - source.G) * amount));
            int blue = (int)Math.Round(source.B + ((target.B - source.B) * amount));
            return Color.FromArgb(alpha, red, green, blue);
        }

        private static void EnsureNonNegative(int value, string propertyName) {
            if(value < 0) {
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    value,
                    "El valor no puede ser negativo.");
            }
        }

        [DllImport("user32.dll", EntryPoint = "SendMessageW")]
        private static extern IntPtr SendMessageW(
            IntPtr windowHandle,
            int message,
            IntPtr wordParameter,
            IntPtr longParameter);

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

        private readonly struct DatePickerAppearance:IEquatable<DatePickerAppearance> {
            public DatePickerAppearance(
                Color skinColor,
                Color textColor,
                Color borderColor,
                Color iconColor,
                float dropDownProgress) {
                SkinColor = skinColor;
                TextColor = textColor;
                BorderColor = borderColor;
                IconColor = iconColor;
                DropDownProgress = dropDownProgress;
            }

            public Color SkinColor { get; }

            public Color TextColor { get; }

            public Color BorderColor { get; }

            public Color IconColor { get; }

            public float DropDownProgress { get; }

            public bool Equals(DatePickerAppearance other) {
                return SkinColor == other.SkinColor &&
                    TextColor == other.TextColor &&
                    BorderColor == other.BorderColor &&
                    IconColor == other.IconColor &&
                    Math.Abs(DropDownProgress - other.DropDownProgress) < 0.001f;
            }

            public override bool Equals(object? obj) {
                return obj is DatePickerAppearance other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    int hashCode = SkinColor.GetHashCode();
                    hashCode = (hashCode * 397) ^ TextColor.GetHashCode();
                    hashCode = (hashCode * 397) ^ BorderColor.GetHashCode();
                    hashCode = (hashCode * 397) ^ IconColor.GetHashCode();
                    hashCode = (hashCode * 397) ^ DropDownProgress.GetHashCode();
                    return hashCode;
                }
            }

            public static DatePickerAppearance Interpolate(
                DatePickerAppearance origin,
                DatePickerAppearance destination,
                float progress) {
                float amount = Math.Max(0f, Math.Min(1f, progress));
                return new DatePickerAppearance(
                    Blend(origin.SkinColor, destination.SkinColor, amount),
                    Blend(origin.TextColor, destination.TextColor, amount),
                    Blend(origin.BorderColor, destination.BorderColor, amount),
                    Blend(origin.IconColor, destination.IconColor, amount),
                    origin.DropDownProgress +
                        ((destination.DropDownProgress - origin.DropDownProgress) * amount));
            }
        }
    }
}

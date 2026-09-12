using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sara_UI_Design.Animations;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Representa un botón de opción accesible con estados de interacción y transiciones
    /// visuales administradas por <see cref="SaraAnimator"/>.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent(nameof(CheckedChanged))]
    [DefaultProperty(nameof(Checked))]
    public class SaraUI_RadioButton:RadioButton {
        /// <summary>Describe el estado de interacción que gobierna la apariencia del control.</summary>
        public enum SaraRadioVisualState {
            /// <summary>El control no tiene una interacción activa.</summary>
            Normal,

            /// <summary>El puntero se encuentra sobre el control.</summary>
            Hovered,

            /// <summary>El control se está presionando con el ratón o el teclado.</summary>
            Pressed,

            /// <summary>El control tiene el foco de teclado.</summary>
            Focused,

            /// <summary>El control está deshabilitado.</summary>
            Disabled
        }

        private readonly SaraAnimator _animator;
        private Color _checkedColor = Color.MediumSlateBlue;
        private Color _uncheckedColor = Color.Gray;
        private Color _hoverColor = Color.Empty;
        private Color _pressedColor = Color.Empty;
        private Color _focusBorderColor = Color.HotPink;
        private Color _disabledColor = Color.Empty;
        private Color _disabledTextColor = Color.Empty;
        private int _borderSize = 2;
        private int _radioSize = 18;
        private int _indicatorSize = 10;
        private int _textSpacing = 8;
        private bool _animationEnabled = true;
        private int _animationDuration = 180;
        private int _animationFrameInterval = 15;
        private SaraEasing _animationEasing = SaraEasing.EaseOutCubic;
        private bool _showFocusBorder = true;
        private bool _isMouseOver;
        private bool _isPressed;
        private bool _initialized;
        private bool _disposingResources;
        private SaraRadioVisualState _visualState;
        private RadioAppearance _displayAppearance;
        private RadioAppearance _targetAppearance;

        /// <summary>Inicializa un botón de opción accesible, personalizable y animado.</summary>
        public SaraUI_RadioButton() {
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

            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            MinimumSize = new Size(0, 21);
            AccessibleRole = AccessibleRole.RadioButton;

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

        /// <summary>Obtiene o establece el color del borde y del indicador cuando el control está seleccionado.</summary>
        [Category("Sara UI Design")]
        public Color CheckedColor {
            get => _checkedColor;
            set => SetAppearanceColor(ref _checkedColor, value);
        }

        /// <summary>Obtiene o establece el color del borde cuando el control no está seleccionado.</summary>
        [Category("Sara UI Design")]
        public Color UncheckedColor {
            get => _uncheckedColor;
            set => SetAppearanceColor(ref _uncheckedColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del borde cuando el control no está seleccionado.
        /// Se conserva como alias compatible; el código nuevo debe utilizar <see cref="UncheckedColor"/>.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color UnCheckedColor {
            get => UncheckedColor;
            set => UncheckedColor = value;
        }

        /// <summary>
        /// Obtiene o establece el color del borde y del indicador bajo el puntero.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante del estado lógico actual.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color HoverColor {
            get => _hoverColor;
            set => SetAppearanceColor(ref _hoverColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del borde y del indicador mientras se presiona.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante del estado lógico actual.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color PressedColor {
            get => _pressedColor;
            set => SetAppearanceColor(ref _pressedColor, value);
        }

        /// <summary>Obtiene o establece el color utilizado para indicar el foco de teclado.</summary>
        [Category("Sara UI Design")]
        public Color FocusBorderColor {
            get => _focusBorderColor;
            set {
                if(_focusBorderColor == value) {
                    return;
                }

                _focusBorderColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el color del borde y del indicador cuando el control está deshabilitado.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante atenuada.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledColor {
            get => _disabledColor;
            set => SetAppearanceColor(ref _disabledColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del texto cuando el control está deshabilitado.
        /// <see cref="Color.Empty"/> utiliza <see cref="SystemColors.GrayText"/>.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledTextColor {
            get => _disabledTextColor;
            set => SetAppearanceColor(ref _disabledTextColor, value);
        }

        /// <summary>Obtiene o establece el grosor del borde del círculo exterior.</summary>
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

        /// <summary>Obtiene o establece el diámetro solicitado para el círculo exterior.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor menor que uno.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(18)]
        public int RadioSize {
            get => _radioSize;
            set {
                EnsurePositive(value, nameof(RadioSize));

                if(_radioSize == value) {
                    return;
                }

                _radioSize = value;
                RefreshPreferredSize();
            }
        }

        /// <summary>Obtiene o establece el diámetro máximo del indicador de selección.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor menor que uno.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(10)]
        public int IndicatorSize {
            get => _indicatorSize;
            set {
                EnsurePositive(value, nameof(IndicatorSize));

                if(_indicatorSize == value) {
                    return;
                }

                _indicatorSize = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece la separación entre el círculo y el texto.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(8)]
        public int TextSpacing {
            get => _textSpacing;
            set {
                EnsureNonNegative(value, nameof(TextSpacing));

                if(_textSpacing == value) {
                    return;
                }

                _textSpacing = value;
                RefreshPreferredSize();
            }
        }

        /// <summary>Obtiene o establece si los cambios lógicos y de interacción deben animarse.</summary>
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

        /// <summary>
        /// Obtiene o establece la duración de una transición, expresada en milisegundos.
        /// Un valor de cero aplica inmediatamente el destino.
        /// </summary>
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
                EnsurePositive(value, nameof(AnimationFrameInterval));

                if(_animationFrameInterval == value) {
                    return;
                }

                _animationFrameInterval = value;
                RestartActiveAnimation();
            }
        }

        /// <summary>Obtiene o establece la curva utilizada para las transiciones visuales.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar una curva no definida.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(SaraEasing.EaseOutCubic)]
        public SaraEasing AnimationEasing {
            get => _animationEasing;
            set {
                if(!Enum.IsDefined(typeof(SaraEasing), value)) {
                    throw new ArgumentOutOfRangeException(
                        nameof(AnimationEasing), value, "La curva de animación no es compatible.");
                }

                if(_animationEasing == value) {
                    return;
                }

                _animationEasing = value;
                RestartActiveAnimation();
            }
        }

        /// <summary>Obtiene o establece si el control debe mostrar una guía cuando recibe foco mediante teclado.</summary>
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

        /// <summary>Obtiene el estado visual de interacción actual.</summary>
        [Browsable(false)]
        public SaraRadioVisualState VisualState => _visualState;

        /// <summary>Obtiene el estado actual del motor de animación interno.</summary>
        [Browsable(false)]
        public SaraAnimationState AnimationState => _animator.State;

        /// <summary>Obtiene el progreso visual de selección interpolado entre cero y uno.</summary>
        [Browsable(false)]
        public float DisplayedCheckedProgress => _displayAppearance.SelectionProgress;

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
            TextFormatFlags measurementFlags =
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine;
            Size textSize = string.IsNullOrEmpty(Text)
                ? Size.Empty
                : TextRenderer.MeasureText(
                    Text,
                    Font,
                    new Size(32767, 32767),
                    measurementFlags);
            int spacing = textSize.Width > 0 ? _textSpacing : 0;
            int preferredWidth = Padding.Horizontal + _radioSize + spacing + textSize.Width + 2;
            int preferredHeight = Padding.Vertical + Math.Max(_radioSize, textSize.Height) + 2;

            return new Size(
                Math.Max(MinimumSize.Width, preferredWidth),
                Math.Max(MinimumSize.Height, preferredHeight));
        }

        /// <inheritdoc/>
        protected override void OnCheckedChanged(EventArgs e) {
            base.OnCheckedChanged(e);
            UpdateVisualState(animate: true);
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
            UpdateVisualState(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            if(e.Button == MouseButtons.Left && Enabled) {
                _isPressed = true;
                UpdateVisualState(animate: true);
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(MouseEventArgs e) {
            bool previousChecked = Checked;
            bool releasedPress = e.Button == MouseButtons.Left && _isPressed;

            if(e.Button == MouseButtons.Left) {
                _isPressed = false;
            }

            base.OnMouseUp(e);

            if(releasedPress && previousChecked == Checked) {
                UpdateVisualState(animate: true);
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseCaptureChanged(EventArgs e) {
            base.OnMouseCaptureChanged(e);

            if(!Capture && _isPressed) {
                _isPressed = false;
                UpdateVisualState(animate: true);
            }
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e) {
            if(e.KeyCode == Keys.Space && Enabled && !_isPressed) {
                _isPressed = true;
                UpdateVisualState(animate: true);
            }

            base.OnKeyDown(e);
        }

        /// <inheritdoc/>
        protected override void OnKeyUp(KeyEventArgs e) {
            bool previousChecked = Checked;
            bool releasedPress = e.KeyCode == Keys.Space && _isPressed;

            if(e.KeyCode == Keys.Space && _isPressed) {
                _isPressed = false;
            }

            base.OnKeyUp(e);

            if(releasedPress && previousChecked == Checked) {
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
            }

            UpdateVisualState(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            UpdateVisualState(animate: false);
        }

        /// <inheritdoc/>
        protected override void OnHandleDestroyed(EventArgs e) {
            if(!_disposingResources && (_animator.IsRunning || _animator.IsPaused)) {
                _animator.Stop();
            }

            base.OnHandleDestroyed(e);
        }

        /// <inheritdoc/>
        protected override void OnVisibleChanged(EventArgs e) {
            base.OnVisibleChanged(e);

            if(!_initialized || _disposingResources || IsDisposed) {
                return;
            }

            if(Visible) {
                if(!_animator.Resume()) {
                    UpdateVisualState(animate: false);
                }
            } else if(_animator.IsRunning) {
                _animator.Pause();
            }
        }

        /// <inheritdoc/>
        protected override void OnRightToLeftChanged(EventArgs e) {
            base.OnRightToLeftChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnPaddingChanged(EventArgs e) {
            base.OnPaddingChanged(e);
            RefreshPreferredSize();
        }

        /// <inheritdoc/>
        protected override void OnTextChanged(EventArgs e) {
            base.OnTextChanged(e);
            RefreshPreferredSize();
        }

        /// <inheritdoc/>
        protected override void OnFontChanged(EventArgs e) {
            base.OnFontChanged(e);
            RefreshPreferredSize();
        }

        /// <inheritdoc/>
        protected override void OnAutoSizeChanged(EventArgs e) {
            base.OnAutoSizeChanged(e);
            RefreshPreferredSize();
        }

        /// <inheritdoc/>
        protected override void OnForeColorChanged(EventArgs e) {
            base.OnForeColorChanged(e);
            RefreshAppearance();
        }

        /// <inheritdoc/>
        protected override void OnBackColorChanged(EventArgs e) {
            base.OnBackColorChanged(e);
            RefreshAppearance();
        }

        /// <inheritdoc/>
        protected override void OnParentBackColorChanged(EventArgs e) {
            base.OnParentBackColorChanged(e);
            RefreshAppearance();
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs pevent) {
            if(ClientSize.Width <= 0 || ClientSize.Height <= 0) {
                return;
            }

            Graphics graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.Clear(ResolveBackgroundColor());

            Rectangle contentBounds = CreateContentBounds();
            Rectangle radioBounds = CreateRadioBounds(contentBounds);

            DrawRadio(graphics, radioBounds);
            DrawText(graphics, contentBounds, radioBounds);
            DrawFocusCue(graphics);
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

        private SaraRadioVisualState DetermineVisualState() {
            if(!Enabled) {
                return SaraRadioVisualState.Disabled;
            }

            if(_isPressed) {
                return SaraRadioVisualState.Pressed;
            }

            if(_isMouseOver) {
                return SaraRadioVisualState.Hovered;
            }

            if(Focused) {
                return SaraRadioVisualState.Focused;
            }

            return SaraRadioVisualState.Normal;
        }

        private RadioAppearance ResolveAppearance(SaraRadioVisualState state) {
            Color borderColor = Checked ? _checkedColor : _uncheckedColor;
            Color indicatorColor = _checkedColor;
            Color textColor = ForeColor;
            float selectionProgress = Checked ? 1f : 0f;

            switch(state) {
                case SaraRadioVisualState.Hovered:
                    borderColor = _hoverColor.IsEmpty
                        ? Blend(borderColor, Color.Black, 0.12f)
                        : _hoverColor;
                    indicatorColor = _hoverColor.IsEmpty
                        ? Blend(indicatorColor, Color.Black, 0.08f)
                        : _hoverColor;
                    break;
                case SaraRadioVisualState.Pressed:
                    borderColor = _pressedColor.IsEmpty
                        ? Blend(borderColor, Color.Black, 0.24f)
                        : _pressedColor;
                    indicatorColor = borderColor;
                    break;
                case SaraRadioVisualState.Disabled:
                    Color background = ResolveBackgroundColor();
                    borderColor = _disabledColor.IsEmpty
                        ? Blend(borderColor, background, 0.62f)
                        : _disabledColor;
                    indicatorColor = _disabledColor.IsEmpty
                        ? Blend(indicatorColor, background, 0.62f)
                        : _disabledColor;
                    textColor = _disabledTextColor.IsEmpty
                        ? SystemColors.GrayText
                        : _disabledTextColor;
                    break;
            }

            return new RadioAppearance(borderColor, indicatorColor, textColor, selectionProgress);
        }

        private void UpdateVisualState(bool animate) {
            if(!_initialized || _disposingResources || IsDisposed) {
                return;
            }

            SaraRadioVisualState newState = DetermineVisualState();
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

            RadioAppearance origin = _displayAppearance;
            RadioAppearance destination = _targetAppearance;

            _animator.Start(
                0f,
                1f,
                progress => {
                    _displayAppearance = RadioAppearance.Interpolate(origin, destination, progress);
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

        private void RefreshPreferredSize() {
            if(!_initialized || _disposingResources || IsDisposed) {
                return;
            }

            if(AutoSize) {
                Size preferredSize = GetPreferredSize(Size.Empty);
                if(Size != preferredSize) {
                    Size = preferredSize;
                }
            }

            Invalidate();
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

        private Rectangle CreateContentBounds() {
            return Rectangle.FromLTRB(
                Padding.Left,
                Padding.Top,
                Math.Max(Padding.Left, ClientSize.Width - Padding.Right),
                Math.Max(Padding.Top, ClientSize.Height - Padding.Bottom));
        }

        private Rectangle CreateRadioBounds(Rectangle contentBounds) {
            if(contentBounds.Width <= 0 || contentBounds.Height <= 0) {
                return Rectangle.Empty;
            }

            int diameter = Math.Min(_radioSize, Math.Min(contentBounds.Width, contentBounds.Height));
            ContentAlignment alignment = GetEffectiveCheckAlign();
            int x;
            int y;

            if(IsLeftAligned(alignment)) {
                x = contentBounds.Left;
            } else if(IsRightAligned(alignment)) {
                x = contentBounds.Right - diameter;
            } else {
                x = contentBounds.Left + ((contentBounds.Width - diameter) / 2);
            }

            if(IsTopAligned(alignment)) {
                y = contentBounds.Top;
            } else if(IsBottomAligned(alignment)) {
                y = contentBounds.Bottom - diameter;
            } else {
                y = contentBounds.Top + ((contentBounds.Height - diameter) / 2);
            }

            return new Rectangle(x, y, diameter, diameter);
        }

        private void DrawRadio(Graphics graphics, Rectangle radioBounds) {
            if(radioBounds.Width <= 0 || radioBounds.Height <= 0) {
                return;
            }

            if(_borderSize > 0) {
                float borderWidth = Math.Min(
                    _borderSize,
                    Math.Max(1f, Math.Min(radioBounds.Width, radioBounds.Height) / 2f));
                float inset = borderWidth / 2f;
                RectangleF borderBounds = RectangleF.Inflate(radioBounds, -inset, -inset);

                if(borderBounds.Width > 0f && borderBounds.Height > 0f) {
                    using Pen borderPen = new Pen(_displayAppearance.BorderColor, borderWidth) {
                        Alignment = PenAlignment.Center
                    };
                    graphics.DrawEllipse(borderPen, borderBounds);
                }
            }

            float progress = Math.Max(0f, Math.Min(1f, _displayAppearance.SelectionProgress));
            int maximumIndicator = Math.Min(
                _indicatorSize,
                Math.Max(1, Math.Min(radioBounds.Width, radioBounds.Height) - (_borderSize * 2) - 2));
            float indicatorDiameter = maximumIndicator * progress;

            if(indicatorDiameter <= 0.1f) {
                return;
            }

            RectangleF indicatorBounds = new RectangleF(
                radioBounds.Left + ((radioBounds.Width - indicatorDiameter) / 2f),
                radioBounds.Top + ((radioBounds.Height - indicatorDiameter) / 2f),
                indicatorDiameter,
                indicatorDiameter);
            using SolidBrush indicatorBrush = new SolidBrush(_displayAppearance.IndicatorColor);
            graphics.FillEllipse(indicatorBrush, indicatorBounds);
        }

        private void DrawText(Graphics graphics, Rectangle contentBounds, Rectangle radioBounds) {
            if(string.IsNullOrEmpty(Text) || contentBounds.Width <= 0 || contentBounds.Height <= 0) {
                return;
            }

            Rectangle textBounds = CreateTextBounds(contentBounds, radioBounds);
            if(textBounds.Width <= 0 || textBounds.Height <= 0) {
                return;
            }

            TextRenderer.DrawText(
                graphics,
                Text,
                Font,
                textBounds,
                _displayAppearance.TextColor,
                GetTextFormatFlags());
        }

        private Rectangle CreateTextBounds(Rectangle contentBounds, Rectangle radioBounds) {
            ContentAlignment alignment = GetEffectiveCheckAlign();

            if(IsLeftAligned(alignment)) {
                int left = Math.Min(contentBounds.Right, radioBounds.Right + _textSpacing);
                return Rectangle.FromLTRB(left, contentBounds.Top, contentBounds.Right, contentBounds.Bottom);
            }

            if(IsRightAligned(alignment)) {
                int right = Math.Max(contentBounds.Left, radioBounds.Left - _textSpacing);
                return Rectangle.FromLTRB(contentBounds.Left, contentBounds.Top, right, contentBounds.Bottom);
            }

            if(IsTopAligned(alignment)) {
                int top = Math.Min(contentBounds.Bottom, radioBounds.Bottom + _textSpacing);
                return Rectangle.FromLTRB(contentBounds.Left, top, contentBounds.Right, contentBounds.Bottom);
            }

            if(IsBottomAligned(alignment)) {
                int bottom = Math.Max(contentBounds.Top, radioBounds.Top - _textSpacing);
                return Rectangle.FromLTRB(contentBounds.Left, contentBounds.Top, contentBounds.Right, bottom);
            }

            return contentBounds;
        }

        private void DrawFocusCue(Graphics graphics) {
            if(!_showFocusBorder || !Enabled || !Focused || !ShowFocusCues) {
                return;
            }

            Rectangle focusBounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if(focusBounds.Width <= 0 || focusBounds.Height <= 0) {
                return;
            }

            Color focusColor = _focusBorderColor.IsEmpty
                ? SystemColors.Highlight
                : _focusBorderColor;
            using Pen focusPen = new Pen(focusColor, 1f) {
                Alignment = PenAlignment.Inset,
                DashStyle = DashStyle.Dot
            };
            graphics.DrawRectangle(focusPen, focusBounds);
        }

        private TextFormatFlags GetTextFormatFlags() {
            ContentAlignment alignment = GetEffectiveTextAlign();
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

            if(IsLeftAligned(alignment)) {
                flags |= TextFormatFlags.Left;
            } else if(IsRightAligned(alignment)) {
                flags |= TextFormatFlags.Right;
            } else {
                flags |= TextFormatFlags.HorizontalCenter;
            }

            if(IsTopAligned(alignment)) {
                flags |= TextFormatFlags.Top;
            } else if(IsBottomAligned(alignment)) {
                flags |= TextFormatFlags.Bottom;
            } else {
                flags |= TextFormatFlags.VerticalCenter;
            }

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

        private ContentAlignment GetEffectiveCheckAlign() {
            return RightToLeft == RightToLeft.Yes ? MirrorAlignment(CheckAlign) : CheckAlign;
        }

        private ContentAlignment GetEffectiveTextAlign() {
            return RightToLeft == RightToLeft.Yes ? MirrorAlignment(TextAlign) : TextAlign;
        }

        private Color ResolveBackgroundColor() {
            if(BackColor != Color.Transparent) {
                return BackColor;
            }

            return Parent?.BackColor ?? SystemColors.Control;
        }

        private static ContentAlignment MirrorAlignment(ContentAlignment alignment) {
            switch(alignment) {
                case ContentAlignment.TopLeft:
                    return ContentAlignment.TopRight;
                case ContentAlignment.TopRight:
                    return ContentAlignment.TopLeft;
                case ContentAlignment.MiddleLeft:
                    return ContentAlignment.MiddleRight;
                case ContentAlignment.MiddleRight:
                    return ContentAlignment.MiddleLeft;
                case ContentAlignment.BottomLeft:
                    return ContentAlignment.BottomRight;
                case ContentAlignment.BottomRight:
                    return ContentAlignment.BottomLeft;
                default:
                    return alignment;
            }
        }

        private static bool IsLeftAligned(ContentAlignment alignment) {
            return alignment == ContentAlignment.TopLeft ||
                alignment == ContentAlignment.MiddleLeft ||
                alignment == ContentAlignment.BottomLeft;
        }

        private static bool IsRightAligned(ContentAlignment alignment) {
            return alignment == ContentAlignment.TopRight ||
                alignment == ContentAlignment.MiddleRight ||
                alignment == ContentAlignment.BottomRight;
        }

        private static bool IsTopAligned(ContentAlignment alignment) {
            return alignment == ContentAlignment.TopLeft ||
                alignment == ContentAlignment.TopCenter ||
                alignment == ContentAlignment.TopRight;
        }

        private static bool IsBottomAligned(ContentAlignment alignment) {
            return alignment == ContentAlignment.BottomLeft ||
                alignment == ContentAlignment.BottomCenter ||
                alignment == ContentAlignment.BottomRight;
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

        private static void EnsurePositive(int value, string propertyName) {
            if(value < 1) {
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    value,
                    "El valor debe ser mayor que cero.");
            }
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

        private readonly struct RadioAppearance:IEquatable<RadioAppearance> {
            public RadioAppearance(
                Color borderColor,
                Color indicatorColor,
                Color textColor,
                float selectionProgress) {
                BorderColor = borderColor;
                IndicatorColor = indicatorColor;
                TextColor = textColor;
                SelectionProgress = selectionProgress;
            }

            public Color BorderColor { get; }

            public Color IndicatorColor { get; }

            public Color TextColor { get; }

            public float SelectionProgress { get; }

            public bool Equals(RadioAppearance other) {
                return BorderColor == other.BorderColor &&
                    IndicatorColor == other.IndicatorColor &&
                    TextColor == other.TextColor &&
                    Math.Abs(SelectionProgress - other.SelectionProgress) < 0.001f;
            }

            public override bool Equals(object? obj) {
                return obj is RadioAppearance other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    int hashCode = BorderColor.GetHashCode();
                    hashCode = (hashCode * 397) ^ IndicatorColor.GetHashCode();
                    hashCode = (hashCode * 397) ^ TextColor.GetHashCode();
                    hashCode = (hashCode * 397) ^ SelectionProgress.GetHashCode();
                    return hashCode;
                }
            }

            public static RadioAppearance Interpolate(
                RadioAppearance origin,
                RadioAppearance destination,
                float progress) {
                float amount = Math.Max(0f, Math.Min(1f, progress));
                return new RadioAppearance(
                    Blend(origin.BorderColor, destination.BorderColor, amount),
                    Blend(origin.IndicatorColor, destination.IndicatorColor, amount),
                    Blend(origin.TextColor, destination.TextColor, amount),
                    origin.SelectionProgress +
                        ((destination.SelectionProgress - origin.SelectionProgress) * amount));
            }
        }
    }
}

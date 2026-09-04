using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sara_UI_Design.Animations;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Representa un interruptor de palanca con estados accesibles y transiciones
    /// visuales administradas por <see cref="SaraAnimator"/>.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent(nameof(CheckedChanged))]
    [DefaultProperty(nameof(Checked))]
    public class SaraUI_ToggleButton:CheckBox {
        /// <summary>Describe el estado de interacción que gobierna la apariencia del interruptor.</summary>
        public enum SaraToggleVisualState {
            /// <summary>El interruptor no tiene una interacción activa.</summary>
            Normal,

            /// <summary>El puntero se encuentra sobre el interruptor.</summary>
            Hovered,

            /// <summary>El interruptor se está presionando con el ratón o el teclado.</summary>
            Pressed,

            /// <summary>El interruptor tiene el foco de teclado.</summary>
            Focused,

            /// <summary>El interruptor está deshabilitado.</summary>
            Disabled
        }

        private readonly SaraAnimator _animator;
        private Color _onBackColor = Color.MediumSlateBlue;
        private Color _onToggleColor = Color.WhiteSmoke;
        private Color _offBackColor = Color.Gray;
        private Color _offToggleColor = Color.Gainsboro;
        private Color _indeterminateBackColor = Color.DarkGoldenrod;
        private Color _indeterminateToggleColor = Color.WhiteSmoke;
        private Color _hoverBackColor = Color.Empty;
        private Color _pressedBackColor = Color.Empty;
        private Color _focusBorderColor = Color.HotPink;
        private Color _disabledBackColor = Color.Empty;
        private Color _disabledToggleColor = Color.Empty;
        private bool _solidStyle = true;
        private int _borderSize = 2;
        private int _togglePadding = 2;
        private bool _animationEnabled = true;
        private int _animationDuration = 180;
        private int _animationFrameInterval = 15;
        private SaraEasing _animationEasing = SaraEasing.EaseOutCubic;
        private bool _showFocusBorder = true;
        private bool _isMouseOver;
        private bool _isPressed;
        private bool _initialized;
        private bool _disposingResources;
        private SaraToggleVisualState _visualState;
        private ToggleAppearance _displayAppearance;
        private ToggleAppearance _targetAppearance;

        /// <summary>Inicializa un interruptor compacto, accesible y animado.</summary>
        public SaraUI_ToggleButton() {
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

            AutoSize = false;
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            MinimumSize = new Size(45, 22);
            Size = new Size(45, 22);
            AccessibleRole = AccessibleRole.CheckButton;

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

        /// <summary>Obtiene o establece el color del canal cuando el interruptor está activado.</summary>
        [Category("Sara UI Design")]
        public Color OnBackColor {
            get => _onBackColor;
            set => SetAppearanceColor(ref _onBackColor, value);
        }

        /// <summary>Obtiene o establece el color del indicador cuando el interruptor está activado.</summary>
        [Category("Sara UI Design")]
        public Color OnToggleColor {
            get => _onToggleColor;
            set => SetAppearanceColor(ref _onToggleColor, value);
        }

        /// <summary>Obtiene o establece el color del canal cuando el interruptor está desactivado.</summary>
        [Category("Sara UI Design")]
        public Color OffBackColor {
            get => _offBackColor;
            set => SetAppearanceColor(ref _offBackColor, value);
        }

        /// <summary>Obtiene o establece el color del indicador cuando el interruptor está desactivado.</summary>
        [Category("Sara UI Design")]
        public Color OffToggleColor {
            get => _offToggleColor;
            set => SetAppearanceColor(ref _offToggleColor, value);
        }

        /// <summary>Obtiene o establece el color del canal en estado indeterminado.</summary>
        [Category("Sara UI Design")]
        public Color IndeterminateBackColor {
            get => _indeterminateBackColor;
            set => SetAppearanceColor(ref _indeterminateBackColor, value);
        }

        /// <summary>Obtiene o establece el color del indicador en estado indeterminado.</summary>
        [Category("Sara UI Design")]
        public Color IndeterminateToggleColor {
            get => _indeterminateToggleColor;
            set => SetAppearanceColor(ref _indeterminateToggleColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del canal bajo el puntero.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante del estado lógico actual.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color HoverBackColor {
            get => _hoverBackColor;
            set => SetAppearanceColor(ref _hoverBackColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del canal mientras se presiona.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante del estado lógico actual.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color PressedBackColor {
            get => _pressedBackColor;
            set => SetAppearanceColor(ref _pressedBackColor, value);
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
        /// Obtiene o establece el color del canal cuando el interruptor está deshabilitado.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante atenuada.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledBackColor {
            get => _disabledBackColor;
            set => SetAppearanceColor(ref _disabledBackColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del indicador cuando el interruptor está deshabilitado.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante atenuada.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledToggleColor {
            get => _disabledToggleColor;
            set => SetAppearanceColor(ref _disabledToggleColor, value);
        }

        /// <summary>
        /// Obtiene o establece el texto lógico del interruptor. Se conserva para accesibilidad y
        /// automatización, pero no se dibuja dentro de la superficie compacta.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#if NET8_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
        public override string Text {
            get => base.Text;
            set => base.Text = value ?? string.Empty;
        }

        /// <summary>Obtiene o establece si el canal se rellena o se representa únicamente con contorno.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool SolidStyle {
            get => _solidStyle;
            set {
                if(_solidStyle == value) {
                    return;
                }

                _solidStyle = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el grosor del contorno utilizado cuando <see cref="SolidStyle"/> es falso.</summary>
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

        /// <summary>Obtiene o establece la separación entre el indicador circular y el canal.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(2)]
        public int TogglePadding {
            get => _togglePadding;
            set {
                EnsureNonNegative(value, nameof(TogglePadding));

                if(_togglePadding == value) {
                    return;
                }

                _togglePadding = value;
                Invalidate();
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
                if(value < 1) {
                    throw new ArgumentOutOfRangeException(
                        nameof(AnimationFrameInterval),
                        value,
                        "El intervalo entre fotogramas debe ser mayor que cero.");
                }

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
        public SaraToggleVisualState VisualState => _visualState;

        /// <summary>Obtiene el estado actual del motor de animación interno.</summary>
        [Browsable(false)]
        public SaraAnimationState AnimationState => _animator.State;

        /// <summary>
        /// Obtiene la posición lógica interpolada del indicador: cero representa apagado,
        /// uno representa encendido y 0.5 representa el estado indeterminado.
        /// </summary>
        [Browsable(false)]
        public float DisplayedToggleProgress => _displayAppearance.Progress;

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
            return new Size(
                Math.Max(MinimumSize.Width, 45 + Padding.Horizontal),
                Math.Max(MinimumSize.Height, 22 + Padding.Vertical));
        }

        /// <inheritdoc/>
        protected override void OnCheckStateChanged(EventArgs e) {
            base.OnCheckStateChanged(e);
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
            CheckState previousState = CheckState;
            bool releasedPress = e.Button == MouseButtons.Left && _isPressed;

            if(e.Button == MouseButtons.Left) {
                _isPressed = false;
            }

            base.OnMouseUp(e);

            if(releasedPress && previousState == CheckState) {
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
            CheckState previousState = CheckState;
            bool releasedPress = e.KeyCode == Keys.Space && _isPressed;

            if(e.KeyCode == Keys.Space && _isPressed) {
                _isPressed = false;
            }

            base.OnKeyUp(e);

            if(releasedPress && previousState == CheckState) {
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
            Invalidate();
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
        protected override void OnPaintBackground(PaintEventArgs pevent) {
            pevent.Graphics.Clear(ResolveBackgroundColor());
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs pevent) {
            Graphics graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(ResolveBackgroundColor());

            Rectangle trackBounds = CreateTrackBounds();

            if(trackBounds.Width <= 0 || trackBounds.Height <= 0) {
                return;
            }

            using GraphicsPath trackPath = CreateCapsulePath(trackBounds);

            if(_solidStyle) {
                using SolidBrush trackBrush = new SolidBrush(_displayAppearance.TrackColor);
                graphics.FillPath(trackBrush, trackPath);
            } else if(_borderSize > 0) {
                float borderWidth = Math.Min(
                    _borderSize,
                    Math.Max(1f, Math.Min(trackBounds.Width, trackBounds.Height) / 2f));
                using Pen trackPen = new Pen(_displayAppearance.TrackColor, borderWidth) {
                    Alignment = PenAlignment.Inset
                };
                graphics.DrawPath(trackPen, trackPath);
            }

            Rectangle thumbBounds = CreateThumbBounds(trackBounds, _displayAppearance.Progress);

            if(thumbBounds.Width > 0 && thumbBounds.Height > 0) {
                using SolidBrush thumbBrush = new SolidBrush(_displayAppearance.ThumbColor);
                graphics.FillEllipse(thumbBrush, thumbBounds);
            }

            if(_showFocusBorder && Enabled && Focused && ShowFocusCues) {
                Color focusColor = _focusBorderColor.IsEmpty
                    ? SystemColors.Highlight
                    : _focusBorderColor;
                using Pen focusPen = new Pen(focusColor, 1f) {
                    Alignment = PenAlignment.Inset,
                    DashStyle = DashStyle.Dot
                };
                graphics.DrawPath(focusPen, trackPath);
            }
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

        private SaraToggleVisualState DetermineVisualState() {
            if(!Enabled) {
                return SaraToggleVisualState.Disabled;
            }

            if(_isPressed) {
                return SaraToggleVisualState.Pressed;
            }

            if(_isMouseOver) {
                return SaraToggleVisualState.Hovered;
            }

            if(Focused) {
                return SaraToggleVisualState.Focused;
            }

            return SaraToggleVisualState.Normal;
        }

        private ToggleAppearance ResolveAppearance(SaraToggleVisualState state) {
            Color trackColor;
            Color thumbColor;
            float progress;

            switch(CheckState) {
                case CheckState.Checked:
                    trackColor = _onBackColor;
                    thumbColor = _onToggleColor;
                    progress = 1f;
                    break;
                case CheckState.Indeterminate:
                    trackColor = _indeterminateBackColor;
                    thumbColor = _indeterminateToggleColor;
                    progress = 0.5f;
                    break;
                default:
                    trackColor = _offBackColor;
                    thumbColor = _offToggleColor;
                    progress = 0f;
                    break;
            }

            switch(state) {
                case SaraToggleVisualState.Hovered:
                    trackColor = _hoverBackColor.IsEmpty
                        ? Blend(trackColor, Color.White, 0.12f)
                        : _hoverBackColor;
                    break;
                case SaraToggleVisualState.Pressed:
                    trackColor = _pressedBackColor.IsEmpty
                        ? Blend(trackColor, Color.Black, 0.14f)
                        : _pressedBackColor;
                    break;
                case SaraToggleVisualState.Disabled:
                    Color background = ResolveBackgroundColor();
                    trackColor = _disabledBackColor.IsEmpty
                        ? Blend(trackColor, background, 0.62f)
                        : _disabledBackColor;
                    thumbColor = _disabledToggleColor.IsEmpty
                        ? Blend(thumbColor, SystemColors.ControlDark, 0.55f)
                        : _disabledToggleColor;
                    break;
            }

            return new ToggleAppearance(trackColor, thumbColor, progress);
        }

        private void UpdateVisualState(bool animate) {
            if(!_initialized || _disposingResources || IsDisposed) {
                return;
            }

            SaraToggleVisualState newState = DetermineVisualState();
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

            ToggleAppearance origin = _displayAppearance;
            ToggleAppearance destination = _targetAppearance;

            _animator.Start(
                0f,
                1f,
                progress => {
                    _displayAppearance = ToggleAppearance.Interpolate(origin, destination, progress);
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

        private Rectangle CreateTrackBounds() {
            int width = Math.Max(0, ClientSize.Width - Padding.Horizontal - 1);
            int height = Math.Max(0, ClientSize.Height - Padding.Vertical - 1);
            return new Rectangle(Padding.Left, Padding.Top, width, height);
        }

        private Rectangle CreateThumbBounds(Rectangle trackBounds, float logicalProgress) {
            int smallestSide = Math.Min(trackBounds.Width, trackBounds.Height);
            int maximumPadding = Math.Max(0, (smallestSide - 1) / 2);
            int padding = Math.Min(_togglePadding, maximumPadding);
            int diameter = Math.Max(1, trackBounds.Height - (padding * 2));
            diameter = Math.Min(diameter, Math.Max(1, trackBounds.Width - (padding * 2)));
            int travel = Math.Max(0, trackBounds.Width - (padding * 2) - diameter);
            float clampedProgress = Math.Max(0f, Math.Min(1f, logicalProgress));

            if(RightToLeft == RightToLeft.Yes) {
                clampedProgress = 1f - clampedProgress;
            }

            int x = trackBounds.Left + padding +
                (int)Math.Round(travel * clampedProgress, MidpointRounding.AwayFromZero);
            int y = trackBounds.Top + ((trackBounds.Height - diameter) / 2);
            return new Rectangle(x, y, diameter, diameter);
        }

        private Color ResolveBackgroundColor() {
            if(BackColor != Color.Transparent) {
                return BackColor;
            }

            return Parent?.BackColor ?? SystemColors.Control;
        }

        private static GraphicsPath CreateCapsulePath(Rectangle bounds) {
            GraphicsPath path = new GraphicsPath();

            if(bounds.Width <= 0 || bounds.Height <= 0) {
                return path;
            }

            if(bounds.Width <= bounds.Height) {
                path.AddEllipse(bounds);
                return path;
            }

            int diameter = bounds.Height;
            Rectangle arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
            path.StartFigure();
            path.AddArc(arc, 90, 180);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 180);
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

        private readonly struct ToggleAppearance:IEquatable<ToggleAppearance> {
            public ToggleAppearance(Color trackColor, Color thumbColor, float progress) {
                TrackColor = trackColor;
                ThumbColor = thumbColor;
                Progress = progress;
            }

            public Color TrackColor { get; }

            public Color ThumbColor { get; }

            public float Progress { get; }

            public bool Equals(ToggleAppearance other) {
                return TrackColor == other.TrackColor &&
                    ThumbColor == other.ThumbColor &&
                    Math.Abs(Progress - other.Progress) < 0.001f;
            }

            public override bool Equals(object? obj) {
                return obj is ToggleAppearance other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    int hashCode = TrackColor.GetHashCode();
                    hashCode = (hashCode * 397) ^ ThumbColor.GetHashCode();
                    hashCode = (hashCode * 397) ^ Progress.GetHashCode();
                    return hashCode;
                }
            }

            public static ToggleAppearance Interpolate(
                ToggleAppearance origin,
                ToggleAppearance destination,
                float progress) {
                float amount = Math.Max(0f, Math.Min(1f, progress));
                return new ToggleAppearance(
                    Blend(origin.TrackColor, destination.TrackColor, amount),
                    Blend(origin.ThumbColor, destination.ThumbColor, amount),
                    origin.Progress + ((destination.Progress - origin.Progress) * amount));
            }
        }
    }
}

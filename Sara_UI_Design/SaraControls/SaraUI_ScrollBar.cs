using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sara_UI_Design.Animations;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Representa una barra de desplazamiento horizontal o vertical con arrastre,
    /// teclado, rueda, estados visuales y transiciones administradas por <see cref="SaraAnimator"/>.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent(nameof(ValueChanged))]
    [DefaultProperty(nameof(Value))]
    [DefaultBindingProperty(nameof(Value))]
    public class SaraUI_ScrollBar:Control {
        private const int MouseWheelDelta = 120;

        /// <summary>Define el eje principal utilizado por la barra.</summary>
        public enum ScrollOrientation {
            /// <summary>Dispone el canal de izquierda a derecha.</summary>
            Horizontal,

            /// <summary>Dispone el canal de arriba hacia abajo.</summary>
            Vertical
        }

        /// <summary>Describe el estado de interacción que gobierna la apariencia del control.</summary>
        public enum SaraScrollBarVisualState {
            /// <summary>La barra no tiene una interacción activa.</summary>
            Normal,

            /// <summary>El puntero se encuentra sobre la barra.</summary>
            Hovered,

            /// <summary>El canal o el indicador se están presionando.</summary>
            Pressed,

            /// <summary>El indicador está siendo arrastrado.</summary>
            Dragging,

            /// <summary>La barra tiene el foco de teclado.</summary>
            Focused,

            /// <summary>La barra está deshabilitada.</summary>
            Disabled
        }

        private readonly SaraAnimator _valueAnimator;
        private readonly SaraAnimator _appearanceAnimator;
        private int _minimum;
        private int _maximum = 100;
        private int _value;
        private int _largeChange = 10;
        private int _smallChange = 1;
        private ScrollOrientation _orientation = ScrollOrientation.Vertical;
        private Color _channelColor = Color.FromArgb(224, 224, 230);
        private Color _thumbColor = Color.MediumSlateBlue;
        private Color _hoverThumbColor = Color.Empty;
        private Color _pressedThumbColor = Color.Empty;
        private Color _disabledChannelColor = Color.Empty;
        private Color _disabledThumbColor = Color.Empty;
        private Color _focusBorderColor = Color.HotPink;
        private Color _displayedChannelColor;
        private Color _displayedThumbColor;
        private int _borderRadius = 5;
        private int _minimumThumbSize = 20;
        private int _channelThickness;
        private bool _autoSizeForOrientation = true;
        private bool _reverseDirection;
        private bool _showFocusIndicator = true;
        private bool _animationEnabled = true;
        private int _animationDuration = 220;
        private int _animationFrameInterval = 15;
        private SaraEasing _animationEasing = SaraEasing.EaseOutCubic;
        private double _displayedValue;
        private bool _isMouseOver;
        private bool _isPressed;
        private bool _isDragging;
        private int _dragOffset;
        private bool _disposingResources;
        private SaraScrollBarVisualState _visualState;
        private SaraAnimationState _animationState;

        /// <summary>Inicializa una barra vertical con doble búfer y navegación accesible.</summary>
        public SaraUI_ScrollBar() {
            _valueAnimator = new SaraAnimator();
            _appearanceAnimator = new SaraAnimator();

            _valueAnimator.Completed += ValueAnimator_Completed;
            _valueAnimator.Canceled += Animator_Canceled;
            _valueAnimator.StateChanged += Animator_StateChanged;
            _appearanceAnimator.Completed += AppearanceAnimator_Completed;
            _appearanceAnimator.Canceled += Animator_Canceled;
            _appearanceAnimator.StateChanged += Animator_StateChanged;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.UserPaint,
                true);

            AccessibleRole = AccessibleRole.ScrollBar;
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Size = new Size(10, 150);
            TabStop = true;

            _displayedValue = _value;
            _visualState = DetermineVisualState();
            ResolveAppearance(_visualState, out _displayedChannelColor, out _displayedThumbColor);
            _animationState = ResolveAnimationState();
        }

        /// <summary>Se produce cuando cambia el valor lógico de la barra.</summary>
        [Category("Sara UI Design Logic")]
        public event EventHandler? ValueChanged;

        /// <summary>Se produce cuando una acción del usuario desplaza la barra.</summary>
        [Category("Sara UI Design Logic")]
        public event ScrollEventHandler? Scroll;

        /// <summary>Se produce cuando cambia la orientación del canal.</summary>
        [Category("Sara UI Design Logic")]
        public event EventHandler? OrientationChanged;

        /// <summary>Se produce cuando cambia el estado visual de interacción.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? VisualStateChanged;

        /// <summary>Se produce cuando una transición interna llega a su destino.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationCompleted;

        /// <summary>Se produce cuando una transición interna activa se cancela o se reemplaza.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationCanceled;

        /// <summary>Se produce cuando cambia el estado agregado de las animaciones internas.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationStateChanged;

        /// <summary>Obtiene o establece el límite inferior permitido.</summary>
        [Category("Sara UI Design Logic")]
        [DefaultValue(0)]
        public int Minimum {
            get => _minimum;
            set {
                if(_minimum == value) {
                    return;
                }

                _minimum = value;
                if(_maximum < _minimum) {
                    _maximum = _minimum;
                }

                NormalizeValueAfterRangeChange();
            }
        }

        /// <summary>Obtiene o establece el límite superior permitido.</summary>
        [Category("Sara UI Design Logic")]
        [DefaultValue(100)]
        public int Maximum {
            get => _maximum;
            set {
                if(_maximum == value) {
                    return;
                }

                _maximum = value;
                if(_minimum > _maximum) {
                    _minimum = _maximum;
                }

                NormalizeValueAfterRangeChange();
            }
        }

        /// <summary>
        /// Obtiene o establece el valor lógico solicitado. El valor se limita automáticamente
        /// al intervalo comprendido entre <see cref="Minimum"/> y <see cref="Maximum"/>.
        /// </summary>
        [Category("Sara UI Design Logic")]
        [Bindable(true)]
        [DefaultValue(0)]
        public int Value {
            get => _value;
            set => SetValueCore(value, null, ShouldAnimateValueChange());
        }

        /// <summary>Obtiene o establece el incremento utilizado al activar el canal o las teclas PageUp y PageDown.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design Logic")]
        [DefaultValue(10)]
        public int LargeChange {
            get => _largeChange;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(
                        nameof(LargeChange),
                        value,
                        "El cambio grande no puede ser negativo.");
                }

                if(_largeChange == value) {
                    return;
                }

                _largeChange = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el incremento utilizado por las flechas y la rueda del ratón.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design Logic")]
        [DefaultValue(1)]
        public int SmallChange {
            get => _smallChange;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(
                        nameof(SmallChange),
                        value,
                        "El cambio pequeño no puede ser negativo.");
                }

                _smallChange = value;
            }
        }

        /// <summary>Obtiene o establece si el canal se dispone horizontal o verticalmente.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar una orientación desconocida.</exception>
        [Category("Sara UI Design Logic")]
        [DefaultValue(ScrollOrientation.Vertical)]
        public ScrollOrientation Orientation {
            get => _orientation;
            set {
                if(!Enum.IsDefined(typeof(ScrollOrientation), value)) {
                    throw new ArgumentOutOfRangeException(
                        nameof(Orientation),
                        value,
                        "La orientación indicada no es compatible.");
                }

                if(_orientation == value) {
                    return;
                }

                _orientation = value;
                if(_autoSizeForOrientation) {
                    Size = new Size(Height, Width);
                }

                OnOrientationChanged(EventArgs.Empty);
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece si el ancho y el alto se intercambian al cambiar la orientación.</summary>
        [Category("Sara UI Design Logic")]
        [DefaultValue(true)]
        public bool AutoSizeForOrientation {
            get => _autoSizeForOrientation;
            set => _autoSizeForOrientation = value;
        }

        /// <summary>
        /// Obtiene o establece si el valor máximo debe dibujarse al inicio visual del canal.
        /// En orientación horizontal también se respeta automáticamente <see cref="Control.RightToLeft"/>.
        /// </summary>
        [Category("Sara UI Design Logic")]
        [DefaultValue(false)]
        public bool ReverseDirection {
            get => _reverseDirection;
            set {
                if(_reverseDirection == value) {
                    return;
                }

                _reverseDirection = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el color del canal.</summary>
        [Category("Sara UI Design Appearance")]
        public Color ChannelColor {
            get => _channelColor;
            set => SetAppearanceColor(ref _channelColor, value);
        }

        /// <summary>Obtiene o establece el color normal del indicador móvil.</summary>
        [Category("Sara UI Design Appearance")]
        public Color ThumbColor {
            get => _thumbColor;
            set => SetAppearanceColor(ref _thumbColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del indicador bajo el puntero.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante clara.
        /// </summary>
        [Category("Sara UI Design Appearance")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color HoverThumbColor {
            get => _hoverThumbColor;
            set => SetAppearanceColor(ref _hoverThumbColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del indicador durante la presión o el arrastre.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante oscura.
        /// </summary>
        [Category("Sara UI Design Appearance")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color PressedThumbColor {
            get => _pressedThumbColor;
            set => SetAppearanceColor(ref _pressedThumbColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del canal deshabilitado.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante atenuada.
        /// </summary>
        [Category("Sara UI Design Appearance")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledChannelColor {
            get => _disabledChannelColor;
            set => SetAppearanceColor(ref _disabledChannelColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del indicador deshabilitado.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante atenuada.
        /// </summary>
        [Category("Sara UI Design Appearance")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledThumbColor {
            get => _disabledThumbColor;
            set => SetAppearanceColor(ref _disabledThumbColor, value);
        }

        /// <summary>Obtiene o establece el color de la guía dibujada al recibir el foco.</summary>
        [Category("Sara UI Design Appearance")]
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

        /// <summary>Obtiene o establece el radio de las esquinas del canal y del indicador.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design Appearance")]
        [DefaultValue(5)]
        public int BorderRadius {
            get => _borderRadius;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(
                        nameof(BorderRadius),
                        value,
                        "El radio del borde no puede ser negativo.");
                }

                if(_borderRadius == value) {
                    return;
                }

                _borderRadius = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece la longitud mínima del indicador, expresada en píxeles.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor menor que uno.</exception>
        [Category("Sara UI Design Appearance")]
        [DefaultValue(20)]
        public int MinimumThumbSize {
            get => _minimumThumbSize;
            set {
                if(value < 1) {
                    throw new ArgumentOutOfRangeException(
                        nameof(MinimumThumbSize),
                        value,
                        "La longitud mínima del indicador debe ser mayor que cero.");
                }

                if(_minimumThumbSize == value) {
                    return;
                }

                _minimumThumbSize = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el grosor transversal del canal. Un valor de cero utiliza
        /// todo el grosor disponible después de aplicar <see cref="Control.Padding"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design Appearance")]
        [DefaultValue(0)]
        public int ChannelThickness {
            get => _channelThickness;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(
                        nameof(ChannelThickness),
                        value,
                        "El grosor del canal no puede ser negativo.");
                }

                if(_channelThickness == value) {
                    return;
                }

                _channelThickness = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece si se dibuja una guía cuando el control tiene foco visible.</summary>
        [Category("Sara UI Design Appearance")]
        [DefaultValue(true)]
        public bool ShowFocusIndicator {
            get => _showFocusIndicator;
            set {
                if(_showFocusIndicator == value) {
                    return;
                }

                _showFocusIndicator = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece si los cambios de valor y apariencia se interpolan.</summary>
        [Category("Sara UI Design Animation")]
        [DefaultValue(true)]
        public bool AnimationEnabled {
            get => _animationEnabled;
            set {
                if(_animationEnabled == value) {
                    return;
                }

                _animationEnabled = value;
                if(!value) {
                    StopAnimation();
                    _displayedValue = _value;
                    ResolveAppearance(_visualState, out _displayedChannelColor, out _displayedThumbColor);
                    Invalidate();
                }
            }
        }

        /// <summary>Obtiene o establece la duración de las transiciones, expresada en milisegundos.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design Animation")]
        [DefaultValue(220)]
        public int AnimationDuration {
            get => _animationDuration;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(
                        nameof(AnimationDuration),
                        value,
                        "La duración de la animación no puede ser negativa.");
                }

                _animationDuration = value;
            }
        }

        /// <summary>Obtiene o establece el intervalo solicitado entre fotogramas, expresado en milisegundos.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor menor que uno.</exception>
        [Category("Sara UI Design Animation")]
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

                _animationFrameInterval = value;
            }
        }

        /// <summary>Obtiene o establece la curva utilizada por las transiciones.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar una curva desconocida.</exception>
        [Category("Sara UI Design Animation")]
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

                _animationEasing = value;
            }
        }

        /// <summary>Obtiene el valor que determina actualmente la posición dibujada del indicador.</summary>
        [Browsable(false)]
        public double DisplayedValue => _displayedValue;

        /// <summary>Obtiene los límites actuales del canal dentro del control.</summary>
        [Browsable(false)]
        public Rectangle TrackBounds => GetTrackRectangle();

        /// <summary>Obtiene los límites actuales del indicador móvil.</summary>
        [Browsable(false)]
        public Rectangle ThumbBounds => GetThumbRectangle();

        /// <summary>Obtiene si el usuario está arrastrando el indicador.</summary>
        [Browsable(false)]
        public bool IsDragging => _isDragging;

        /// <summary>Obtiene el estado visual actual.</summary>
        [Browsable(false)]
        public SaraScrollBarVisualState VisualState => _visualState;

        /// <summary>Obtiene el estado agregado de las transiciones internas.</summary>
        [Browsable(false)]
        public SaraAnimationState AnimationState => _animationState;

        /// <summary>Obtiene el estado de la transición que mueve el indicador.</summary>
        [Browsable(false)]
        public SaraAnimationState ValueAnimationState => _valueAnimator.State;

        /// <summary>Obtiene el estado de la transición de colores.</summary>
        [Browsable(false)]
        public SaraAnimationState AppearanceAnimationState => _appearanceAnimator.State;

        /// <summary>Establece ambos límites como una operación única.</summary>
        /// <param name="minimum">Límite inferior.</param>
        /// <param name="maximum">Límite superior.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce cuando <paramref name="maximum"/> es menor que <paramref name="minimum"/>.
        /// </exception>
        public void SetRange(int minimum, int maximum) {
            if(maximum < minimum) {
                throw new ArgumentOutOfRangeException(
                    nameof(maximum),
                    maximum,
                    "El máximo no puede ser menor que el mínimo.");
            }

            if(_minimum == minimum && _maximum == maximum) {
                return;
            }

            _minimum = minimum;
            _maximum = maximum;
            NormalizeValueAfterRangeChange();
        }

        /// <summary>Establece el valor y permite decidir si su desplazamiento debe animarse.</summary>
        /// <param name="value">Valor lógico solicitado.</param>
        /// <param name="animate">Indica si debe interpolarse la posición visible.</param>
        public void SetValue(int value, bool animate) {
            SetValueCore(value, null, animate && ShouldAnimateValueChange());
        }

        /// <summary>Incrementa o disminuye el valor lógico sin exceder el rango.</summary>
        /// <param name="delta">Cantidad con signo que se sumará al valor actual.</param>
        public void Increment(int delta) {
            ScrollEventType type = delta < 0
                ? ScrollEventType.SmallDecrement
                : ScrollEventType.SmallIncrement;

            SetValueCore(AddClamped(_value, delta), type, ShouldAnimateValueChange());
        }

        /// <summary>Pausa las transiciones de valor y apariencia que estén avanzando.</summary>
        /// <returns><see langword="true"/> si al menos una transición cambió a pausada.</returns>
        public bool PauseAnimation() {
            bool valuePaused = _valueAnimator.Pause();
            bool appearancePaused = _appearanceAnimator.Pause();
            return valuePaused || appearancePaused;
        }

        /// <summary>Reanuda las transiciones internas que estén pausadas.</summary>
        /// <returns><see langword="true"/> si al menos una transición volvió a ejecutarse.</returns>
        public bool ResumeAnimation() {
            bool valueResumed = _valueAnimator.Resume();
            bool appearanceResumed = _appearanceAnimator.Resume();
            return valueResumed || appearanceResumed;
        }

        /// <summary>Detiene las transiciones internas y conserva sus valores visibles actuales.</summary>
        /// <returns><see langword="true"/> si se detuvo al menos una transición activa.</returns>
        public bool StopAnimation() {
            bool valueStopped = _valueAnimator.Stop();
            bool appearanceStopped = _appearanceAnimator.Stop();
            return valueStopped || appearanceStopped;
        }

        /// <summary>Genera el evento <see cref="ValueChanged"/>.</summary>
        /// <param name="e">Argumentos del evento.</param>
        protected virtual void OnValueChanged(EventArgs e) {
            ValueChanged?.Invoke(this, e);
        }

        /// <summary>Genera el evento <see cref="Scroll"/>.</summary>
        /// <param name="e">Información sobre el tipo y los valores del desplazamiento.</param>
        protected virtual void OnScroll(ScrollEventArgs e) {
            Scroll?.Invoke(this, e);
        }

        /// <summary>Genera el evento <see cref="OrientationChanged"/>.</summary>
        /// <param name="e">Argumentos del evento.</param>
        protected virtual void OnOrientationChanged(EventArgs e) {
            OrientationChanged?.Invoke(this, e);
        }

        /// <inheritdoc/>
        protected override void OnPaintBackground(PaintEventArgs e) {
            base.OnPaintBackground(e);
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            Rectangle trackRectangle = GetTrackRectangle();
            if(trackRectangle.Width <= 0 || trackRectangle.Height <= 0) {
                return;
            }

            Graphics graphics = e.Graphics;
            SmoothingMode previousSmoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using(GraphicsPath channelPath = CreateRoundedPath(trackRectangle, _borderRadius))
            using(SolidBrush channelBrush = new SolidBrush(_displayedChannelColor)) {
                graphics.FillPath(channelBrush, channelPath);
            }

            Rectangle thumbRectangle = GetThumbRectangle();
            if(thumbRectangle.Width > 0 && thumbRectangle.Height > 0) {
                using(GraphicsPath thumbPath = CreateRoundedPath(thumbRectangle, Math.Max(0, _borderRadius - 1)))
                using(SolidBrush thumbBrush = new SolidBrush(_displayedThumbColor)) {
                    graphics.FillPath(thumbBrush, thumbPath);
                }
            }

            if(_showFocusIndicator && Focused && ShowFocusCues) {
                Rectangle focusRectangle = Rectangle.Inflate(trackRectangle, -1, -1);
                if(focusRectangle.Width > 0 && focusRectangle.Height > 0) {
                    using Pen focusPen = new Pen(_focusBorderColor) {
                        DashStyle = DashStyle.Dot
                    };
                    graphics.DrawRectangle(focusPen, focusRectangle);
                }
            }

            graphics.SmoothingMode = previousSmoothingMode;
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
            if(!Capture) {
                _isMouseOver = false;
                _isPressed = false;
                UpdateVisualState(animate: true);
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            if(e.Button != MouseButtons.Left || !Enabled) {
                return;
            }

            Focus();
            _isMouseOver = true;
            _isPressed = true;
            Rectangle thumbRectangle = GetThumbRectangle();

            if(thumbRectangle.Contains(e.Location)) {
                PrepareForDragging();
                thumbRectangle = GetThumbRectangle();
                _dragOffset = GetPrimaryCoordinate(e.Location) - GetPrimaryStart(thumbRectangle);
                _isDragging = true;
            } else {
                int pointerCoordinate = GetPrimaryCoordinate(e.Location);
                int thumbStart = GetPrimaryStart(thumbRectangle);
                int visualDirection = pointerCoordinate < thumbStart ? -1 : 1;
                ChangeByVisualDirection(visualDirection, _largeChange, ResolveLargeScrollType(visualDirection));
            }

            Capture = true;
            UpdateVisualState(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            _isMouseOver = ClientRectangle.Contains(e.Location);

            if(_isDragging && Capture) {
                DragThumbTo(e.Location);
            }

            UpdateVisualState(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);
            if(e.Button != MouseButtons.Left) {
                return;
            }

            bool wasDragging = _isDragging;
            _isDragging = false;
            _isPressed = false;
            _isMouseOver = ClientRectangle.Contains(e.Location);

            if(Capture) {
                Capture = false;
            }

            if(wasDragging) {
                RaiseScroll(ScrollEventType.EndScroll, _value, _value);
            }

            UpdateVisualState(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnMouseCaptureChanged(EventArgs e) {
            base.OnMouseCaptureChanged(e);
            if(!Capture && (_isDragging || _isPressed)) {
                bool wasDragging = _isDragging;
                _isDragging = false;
                _isPressed = false;

                if(wasDragging) {
                    RaiseScroll(ScrollEventType.EndScroll, _value, _value);
                }

                UpdateVisualState(animate: true);
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseWheel(MouseEventArgs e) {
            base.OnMouseWheel(e);
            if(!Enabled || e.Delta == 0) {
                return;
            }

            int detents = Math.Max(1, Math.Abs(e.Delta) / MouseWheelDelta);
            int visualDirection = e.Delta > 0 ? -1 : 1;
            int step = MultiplyClamped(_smallChange, detents);
            ChangeByVisualDirection(visualDirection, step, ResolveSmallScrollType(visualDirection));
        }

        /// <inheritdoc/>
        protected override bool IsInputKey(Keys keyData) {
            Keys keyCode = keyData & Keys.KeyCode;
            if(IsScrollKey(keyCode)) {
                return true;
            }

            return base.IsInputKey(keyData);
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if(!Enabled) {
                return;
            }

            switch(e.KeyCode) {
            case Keys.Left:
            case Keys.Up:
                ChangeByVisualDirection(-1, _smallChange, ResolveSmallScrollType(-1));
                e.Handled = true;
                break;

            case Keys.Right:
            case Keys.Down:
                ChangeByVisualDirection(1, _smallChange, ResolveSmallScrollType(1));
                e.Handled = true;
                break;

            case Keys.PageUp:
                ChangeByVisualDirection(-1, _largeChange, ResolveLargeScrollType(-1));
                e.Handled = true;
                break;

            case Keys.PageDown:
                ChangeByVisualDirection(1, _largeChange, ResolveLargeScrollType(1));
                e.Handled = true;
                break;

            case Keys.Home:
                SetValueCore(_minimum, ScrollEventType.First, ShouldAnimateValueChange());
                e.Handled = true;
                break;

            case Keys.End:
                SetValueCore(_maximum, ScrollEventType.Last, ShouldAnimateValueChange());
                e.Handled = true;
                break;
            }

            if(e.Handled) {
                e.SuppressKeyPress = true;
                _isPressed = true;
                UpdateVisualState(animate: true);
            }
        }

        /// <inheritdoc/>
        protected override void OnKeyUp(KeyEventArgs e) {
            base.OnKeyUp(e);
            if(_isPressed && IsScrollKey(e.KeyCode)) {
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
                CancelPointerInteraction();
            }

            UpdateVisualState(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnRightToLeftChanged(EventArgs e) {
            base.OnRightToLeftChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnPaddingChanged(EventArgs e) {
            base.OnPaddingChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            _displayedValue = _value;
            UpdateVisualState(animate: false);
        }

        /// <inheritdoc/>
        protected override void OnHandleDestroyed(EventArgs e) {
            CancelPointerInteraction();

            if(!_disposingResources) {
                StopAnimatorIfActive(_valueAnimator);
                StopAnimatorIfActive(_appearanceAnimator);
            }

            base.OnHandleDestroyed(e);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            if(disposing && !_disposingResources) {
                _disposingResources = true;
                CancelPointerInteraction();

                _valueAnimator.Completed -= ValueAnimator_Completed;
                _valueAnimator.Canceled -= Animator_Canceled;
                _valueAnimator.StateChanged -= Animator_StateChanged;
                _appearanceAnimator.Completed -= AppearanceAnimator_Completed;
                _appearanceAnimator.Canceled -= Animator_Canceled;
                _appearanceAnimator.StateChanged -= Animator_StateChanged;
                _valueAnimator.Dispose();
                _appearanceAnimator.Dispose();

                ValueChanged = null;
                Scroll = null;
                OrientationChanged = null;
                VisualStateChanged = null;
                AnimationCompleted = null;
                AnimationCanceled = null;
                AnimationStateChanged = null;
            }

            base.Dispose(disposing);
        }

        private void NormalizeValueAfterRangeChange() {
            int clampedValue = ClampToRange(_value);
            bool valueChanged = clampedValue != _value;
            _value = clampedValue;
            StopAnimatorIfActive(_valueAnimator);
            _displayedValue = _value;

            if(valueChanged) {
                OnValueChanged(EventArgs.Empty);
            }

            Invalidate();
        }

        private bool SetValueCore(int requestedValue, ScrollEventType? scrollType, bool animate) {
            int newValue = ClampToRange(requestedValue);
            int oldValue = _value;
            if(oldValue == newValue) {
                return false;
            }

            _value = newValue;
            TransitionDisplayedValue(animate);
            OnValueChanged(EventArgs.Empty);

            if(scrollType.HasValue) {
                RaiseScroll(scrollType.Value, oldValue, newValue);
            }

            return true;
        }

        private void TransitionDisplayedValue(bool animate) {
            if(!animate || !IsHandleCreated || _animationDuration == 0) {
                StopAnimatorIfActive(_valueAnimator);
                _displayedValue = _value;
                Invalidate();
                return;
            }

            double startValue = _displayedValue;
            double destinationValue = _value;
            if(Math.Abs(destinationValue - startValue) < double.Epsilon) {
                _displayedValue = destinationValue;
                Invalidate();
                return;
            }

            _valueAnimator.Start(
                0f,
                1f,
                progress => {
                    _displayedValue = startValue + ((destinationValue - startValue) * progress);
                    Invalidate();
                },
                CreateAnimationOptions());
        }

        private void PrepareForDragging() {
            if(_valueAnimator.IsRunning || _valueAnimator.IsPaused) {
                _valueAnimator.Stop();
            }

            int visualValue = ClampToRange((int)Math.Round(_displayedValue, MidpointRounding.AwayFromZero));
            if(_value != visualValue) {
                int oldValue = _value;
                _value = visualValue;
                OnValueChanged(EventArgs.Empty);
                RaiseScroll(ScrollEventType.ThumbPosition, oldValue, visualValue);
            }

            _displayedValue = _value;
        }

        private void DragThumbTo(Point pointerLocation) {
            Rectangle trackRectangle = GetTrackRectangle();
            Rectangle thumbRectangle = GetThumbRectangle();
            int availableLength = GetPrimaryLength(trackRectangle) - GetPrimaryLength(thumbRectangle);
            if(availableLength <= 0 || _maximum <= _minimum) {
                return;
            }

            int trackStart = GetPrimaryStart(trackRectangle);
            int requestedStart = GetPrimaryCoordinate(pointerLocation) - _dragOffset;
            int relativeStart = Clamp(requestedStart - trackStart, 0, availableLength);
            double visualRatio = (double)relativeStart / availableLength;
            int requestedValue = RatioToValue(visualRatio);

            SetValueCore(requestedValue, ScrollEventType.ThumbTrack, animate: false);
        }

        private void ChangeByVisualDirection(int visualDirection, int amount, ScrollEventType scrollType) {
            int logicalDirection = IsDirectionReversed() ? -visualDirection : visualDirection;
            int delta = logicalDirection < 0 ? -amount : amount;
            SetValueCore(AddClamped(_value, delta), scrollType, ShouldAnimateValueChange());
        }

        private ScrollEventType ResolveSmallScrollType(int visualDirection) {
            int logicalDirection = IsDirectionReversed() ? -visualDirection : visualDirection;
            return logicalDirection < 0
                ? ScrollEventType.SmallDecrement
                : ScrollEventType.SmallIncrement;
        }

        private ScrollEventType ResolveLargeScrollType(int visualDirection) {
            int logicalDirection = IsDirectionReversed() ? -visualDirection : visualDirection;
            return logicalDirection < 0
                ? ScrollEventType.LargeDecrement
                : ScrollEventType.LargeIncrement;
        }

        private void RaiseScroll(ScrollEventType type, int oldValue, int newValue) {
            System.Windows.Forms.ScrollOrientation formsOrientation =
                _orientation == ScrollOrientation.Vertical
                    ? System.Windows.Forms.ScrollOrientation.VerticalScroll
                    : System.Windows.Forms.ScrollOrientation.HorizontalScroll;

            OnScroll(new ScrollEventArgs(type, oldValue, newValue, formsOrientation));
        }

        private Rectangle GetTrackRectangle() {
            int left = Math.Min(ClientSize.Width, Math.Max(0, Padding.Left));
            int top = Math.Min(ClientSize.Height, Math.Max(0, Padding.Top));
            int right = Math.Max(left, ClientSize.Width - Math.Max(0, Padding.Right));
            int bottom = Math.Max(top, ClientSize.Height - Math.Max(0, Padding.Bottom));
            Rectangle available = Rectangle.FromLTRB(left, top, right, bottom);

            if(_channelThickness <= 0) {
                return available;
            }

            if(_orientation == ScrollOrientation.Vertical) {
                int thickness = Math.Min(_channelThickness, available.Width);
                return new Rectangle(
                    available.Left + ((available.Width - thickness) / 2),
                    available.Top,
                    thickness,
                    available.Height);
            }

            int horizontalThickness = Math.Min(_channelThickness, available.Height);
            return new Rectangle(
                available.Left,
                available.Top + ((available.Height - horizontalThickness) / 2),
                available.Width,
                horizontalThickness);
        }

        private Rectangle GetThumbRectangle() {
            Rectangle trackRectangle = GetTrackRectangle();
            int trackLength = GetPrimaryLength(trackRectangle);
            if(trackLength <= 0 || _maximum <= _minimum) {
                return Rectangle.Empty;
            }

            int thumbLength = CalculateThumbLength(trackLength);
            int availableLength = Math.Max(0, trackLength - thumbLength);
            double valueRatio = ValueToRatio(_displayedValue);
            int offset = (int)Math.Round(availableLength * valueRatio, MidpointRounding.AwayFromZero);
            offset = Clamp(offset, 0, availableLength);

            if(_orientation == ScrollOrientation.Vertical) {
                return new Rectangle(
                    trackRectangle.Left,
                    trackRectangle.Top + offset,
                    trackRectangle.Width,
                    thumbLength);
            }

            return new Rectangle(
                trackRectangle.Left + offset,
                trackRectangle.Top,
                thumbLength,
                trackRectangle.Height);
        }

        private int CalculateThumbLength(int trackLength) {
            long range = GetRangeLength();
            if(range <= 0 || trackLength <= 0) {
                return 0;
            }

            double viewportRatio = (double)_largeChange / (range + _largeChange);
            int proportionalLength = (int)Math.Round(trackLength * viewportRatio, MidpointRounding.AwayFromZero);
            int minimumLength = Math.Min(_minimumThumbSize, trackLength);
            return Clamp(Math.Max(minimumLength, proportionalLength), 1, trackLength);
        }

        private double ValueToRatio(double candidateValue) {
            long range = GetRangeLength();
            if(range <= 0) {
                return 0d;
            }

            double ratio = (candidateValue - _minimum) / range;
            ratio = Math.Max(0d, Math.Min(1d, ratio));
            return IsDirectionReversed() ? 1d - ratio : ratio;
        }

        private int RatioToValue(double visualRatio) {
            double ratio = Math.Max(0d, Math.Min(1d, visualRatio));
            if(IsDirectionReversed()) {
                ratio = 1d - ratio;
            }

            double candidate = _minimum + (GetRangeLength() * ratio);
            if(candidate <= int.MinValue) {
                return ClampToRange(int.MinValue);
            }

            if(candidate >= int.MaxValue) {
                return ClampToRange(int.MaxValue);
            }

            return ClampToRange((int)Math.Round(candidate, MidpointRounding.AwayFromZero));
        }

        private bool IsDirectionReversed() {
            return _reverseDirection ||
                (_orientation == ScrollOrientation.Horizontal && RightToLeft == RightToLeft.Yes);
        }

        private SaraScrollBarVisualState DetermineVisualState() {
            if(!Enabled) {
                return SaraScrollBarVisualState.Disabled;
            }

            if(_isDragging) {
                return SaraScrollBarVisualState.Dragging;
            }

            if(_isPressed) {
                return SaraScrollBarVisualState.Pressed;
            }

            if(_isMouseOver) {
                return SaraScrollBarVisualState.Hovered;
            }

            if(Focused) {
                return SaraScrollBarVisualState.Focused;
            }

            return SaraScrollBarVisualState.Normal;
        }

        private void UpdateVisualState(bool animate) {
            if(_disposingResources) {
                return;
            }

            SaraScrollBarVisualState newState = DetermineVisualState();
            bool stateChanged = newState != _visualState;
            if(!stateChanged) {
                return;
            }

            _visualState = newState;
            TransitionAppearance(animate);
            VisualStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TransitionAppearance(bool animate) {
            ResolveAppearance(_visualState, out Color targetChannelColor, out Color targetThumbColor);

            if(!animate || !_animationEnabled || !IsHandleCreated || _animationDuration == 0) {
                StopAnimatorIfActive(_appearanceAnimator);
                _displayedChannelColor = targetChannelColor;
                _displayedThumbColor = targetThumbColor;
                Invalidate();
                return;
            }

            Color startChannelColor = _displayedChannelColor;
            Color startThumbColor = _displayedThumbColor;
            if(startChannelColor == targetChannelColor && startThumbColor == targetThumbColor) {
                return;
            }

            _appearanceAnimator.Start(
                0f,
                1f,
                progress => {
                    _displayedChannelColor = InterpolateColor(startChannelColor, targetChannelColor, progress);
                    _displayedThumbColor = InterpolateColor(startThumbColor, targetThumbColor, progress);
                    Invalidate();
                },
                CreateAnimationOptions());
        }

        private void ResolveAppearance(
            SaraScrollBarVisualState state,
            out Color channelColor,
            out Color thumbColor) {
            channelColor = _channelColor;
            thumbColor = _thumbColor;

            switch(state) {
            case SaraScrollBarVisualState.Hovered:
                thumbColor = ResolveOptionalColor(_hoverThumbColor, Blend(_thumbColor, Color.White, 0.18f));
                break;

            case SaraScrollBarVisualState.Pressed:
            case SaraScrollBarVisualState.Dragging:
                thumbColor = ResolveOptionalColor(_pressedThumbColor, Blend(_thumbColor, Color.Black, 0.18f));
                break;

            case SaraScrollBarVisualState.Disabled:
                channelColor = ResolveOptionalColor(
                    _disabledChannelColor,
                    Blend(_channelColor, SystemColors.Control, 0.58f));
                thumbColor = ResolveOptionalColor(
                    _disabledThumbColor,
                    Blend(_thumbColor, SystemColors.GrayText, 0.58f));
                break;
            }
        }

        private void SetAppearanceColor(ref Color field, Color value) {
            if(field == value) {
                return;
            }

            field = value;
            TransitionAppearance(animate: true);
        }

        private SaraAnimationOptions CreateAnimationOptions() {
            return new SaraAnimationOptions {
                Duration = _animationDuration,
                FrameInterval = _animationFrameInterval,
                Easing = _animationEasing
            };
        }

        private void ValueAnimator_Completed(object? sender, EventArgs e) {
            _displayedValue = _value;
            Invalidate();
            AnimationCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void AppearanceAnimator_Completed(object? sender, EventArgs e) {
            ResolveAppearance(_visualState, out _displayedChannelColor, out _displayedThumbColor);
            Invalidate();
            AnimationCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void Animator_Canceled(object? sender, EventArgs e) {
            AnimationCanceled?.Invoke(this, EventArgs.Empty);
        }

        private void Animator_StateChanged(object? sender, EventArgs e) {
            SaraAnimationState newState = ResolveAnimationState(sender as SaraAnimator);
            if(_animationState == newState) {
                return;
            }

            _animationState = newState;
            AnimationStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private SaraAnimationState ResolveAnimationState(SaraAnimator? changedAnimator = null) {
            if(_valueAnimator.State == SaraAnimationState.Running ||
                _appearanceAnimator.State == SaraAnimationState.Running) {
                return SaraAnimationState.Running;
            }

            if(_valueAnimator.State == SaraAnimationState.Paused ||
                _appearanceAnimator.State == SaraAnimationState.Paused) {
                return SaraAnimationState.Paused;
            }

            if(changedAnimator != null) {
                return changedAnimator.State;
            }

            if(_valueAnimator.State == SaraAnimationState.Completed ||
                _appearanceAnimator.State == SaraAnimationState.Completed) {
                return SaraAnimationState.Completed;
            }

            return SaraAnimationState.Stopped;
        }

        private void CancelPointerInteraction() {
            _isDragging = false;
            _isPressed = false;
            _isMouseOver = false;

            if(Capture) {
                Capture = false;
            }
        }

        private bool ShouldAnimateValueChange() {
            return _animationEnabled && !_isDragging;
        }

        private int ClampToRange(int candidate) {
            if(candidate < _minimum) {
                return _minimum;
            }

            if(candidate > _maximum) {
                return _maximum;
            }

            return candidate;
        }

        private int AddClamped(int source, int delta) {
            long candidate = (long)source + delta;
            if(candidate < _minimum) {
                return _minimum;
            }

            if(candidate > _maximum) {
                return _maximum;
            }

            return (int)candidate;
        }

        private static int MultiplyClamped(int value, int multiplier) {
            long result = (long)value * multiplier;
            return result > int.MaxValue ? int.MaxValue : (int)result;
        }

        private long GetRangeLength() {
            return (long)_maximum - _minimum;
        }

        private int GetPrimaryCoordinate(Point point) {
            return _orientation == ScrollOrientation.Vertical ? point.Y : point.X;
        }

        private int GetPrimaryStart(Rectangle rectangle) {
            return _orientation == ScrollOrientation.Vertical ? rectangle.Top : rectangle.Left;
        }

        private int GetPrimaryLength(Rectangle rectangle) {
            return _orientation == ScrollOrientation.Vertical ? rectangle.Height : rectangle.Width;
        }

        private static int Clamp(int value, int minimum, int maximum) {
            return Math.Max(minimum, Math.Min(maximum, value));
        }

        private static bool IsScrollKey(Keys keyCode) {
            return keyCode == Keys.Left || keyCode == Keys.Right ||
                keyCode == Keys.Up || keyCode == Keys.Down ||
                keyCode == Keys.Home || keyCode == Keys.End ||
                keyCode == Keys.PageUp || keyCode == Keys.PageDown;
        }

        private static Color ResolveOptionalColor(Color candidate, Color fallback) {
            return candidate.IsEmpty ? fallback : candidate;
        }

        private static Color Blend(Color source, Color target, float amount) {
            return InterpolateColor(source, target, Math.Max(0f, Math.Min(1f, amount)));
        }

        private static Color InterpolateColor(Color from, Color to, float progress) {
            float amount = Math.Max(0f, Math.Min(1f, progress));
            return Color.FromArgb(
                InterpolateByte(from.A, to.A, amount),
                InterpolateByte(from.R, to.R, amount),
                InterpolateByte(from.G, to.G, amount),
                InterpolateByte(from.B, to.B, amount));
        }

        private static int InterpolateByte(int from, int to, float progress) {
            return Clamp(
                (int)Math.Round(from + ((to - from) * progress), MidpointRounding.AwayFromZero),
                byte.MinValue,
                byte.MaxValue);
        }

        private static GraphicsPath CreateRoundedPath(Rectangle rectangle, int radius) {
            GraphicsPath path = new GraphicsPath();
            if(rectangle.Width <= 0 || rectangle.Height <= 0) {
                return path;
            }

            float curveSize = Math.Min(radius * 2f, Math.Min(rectangle.Width, rectangle.Height));
            if(curveSize <= 1f) {
                path.AddRectangle(rectangle);
                return path;
            }

            path.StartFigure();
            path.AddArc(rectangle.Left, rectangle.Top, curveSize, curveSize, 180f, 90f);
            path.AddArc(rectangle.Right - curveSize, rectangle.Top, curveSize, curveSize, 270f, 90f);
            path.AddArc(rectangle.Right - curveSize, rectangle.Bottom - curveSize, curveSize, curveSize, 0f, 90f);
            path.AddArc(rectangle.Left, rectangle.Bottom - curveSize, curveSize, curveSize, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        private static void StopAnimatorIfActive(SaraAnimator animator) {
            if(animator.IsRunning || animator.IsPaused) {
                animator.Stop();
            }
        }
    }
}

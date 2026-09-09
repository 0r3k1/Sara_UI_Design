using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sara_UI_Design.Animations;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Define las posiciones disponibles para el texto de una barra de progreso lineal.
    /// </summary>
    public enum TextPosition {
        /// <summary>Coloca el texto en el extremo izquierdo del control.</summary>
        Left,

        /// <summary>Coloca el texto en el extremo derecho del control.</summary>
        Right,

        /// <summary>Centra el texto dentro del control.</summary>
        Center,

        /// <summary>Mueve el texto junto con el extremo visible del indicador.</summary>
        Sliding,

        /// <summary>Oculta el texto del progreso.</summary>
        None
    }

    /// <summary>
    /// Representa una barra de progreso lineal con bordes redondeados, degradado,
    /// texto configurable y transiciones administradas por <see cref="SaraAnimator"/>.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultBindingProperty(nameof(Value))]
    [DefaultProperty(nameof(Value))]
    public class SaraUI_ProgressBar:ProgressBar {
        private readonly SaraAnimator _animator;
        private Color _channelColor = Color.LightSteelBlue;
        private Color _sliderColor = Color.RoyalBlue;
        private Color _sliderColorEnd = Color.HotPink;
        private Color _disabledChannelColor = Color.Empty;
        private Color _disabledSliderColor = Color.Empty;
        private Color _disabledForeColor = Color.Empty;
        private int _channelHeight = 6;
        private int _sliderHeight = 6;
        private bool _useGradient = true;
        private TextPosition _showValue = TextPosition.Right;
        private string _symbolBefore = string.Empty;
        private string _symbolAfter = string.Empty;
        private bool _showMaximum;
        private bool _animationEnabled = true;
        private int _animationDuration = 500;
        private int _animationFrameInterval = 15;
        private int _marqueeAnimationDuration = 1400;
        private int _marqueeSegmentPercentage = 30;
        private SaraEasing _animationEasing = SaraEasing.Linear;
        private float _displayedValue;
        private float _marqueeProgress = 0.5f;
        private int _lastKnownValue;
        private ProgressBarStyle _lastKnownStyle;
        private bool _disposingResources;

        /// <summary>
        /// Inicializa una barra lineal con doble búfer y pintura personalizada.
        /// </summary>
        public SaraUI_ProgressBar() {
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
            ForeColor = Color.DimGray;
            AccessibleRole = AccessibleRole.ProgressBar;
            _displayedValue = base.Value;
            _lastKnownValue = base.Value;
            _lastKnownStyle = base.Style;
        }

        /// <summary>Se produce cuando una transición determinada llega al valor solicitado.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationCompleted;

        /// <summary>Se produce cuando una animación activa se detiene o se reemplaza.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationCanceled;

        /// <summary>Se produce cuando cambia el estado de la animación interna.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationStateChanged;

        /// <summary>Obtiene o establece el color del canal que representa el rango total.</summary>
        [Category("Sara UI Design")]
        public Color ChannelColor {
            get => _channelColor;
            set {
                if(_channelColor == value) {
                    return;
                }

                _channelColor = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el color inicial del indicador de progreso.</summary>
        [Category("Sara UI Design")]
        public Color SliderColor {
            get => _sliderColor;
            set {
                if(_sliderColor == value) {
                    return;
                }

                _sliderColor = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece si el indicador utiliza un degradado horizontal.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool UseGradient {
            get => _useGradient;
            set {
                if(_useGradient == value) {
                    return;
                }

                _useGradient = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el color final del degradado del indicador.</summary>
        [Category("Sara UI Design")]
        public Color SliderColorEnd {
            get => _sliderColorEnd;
            set {
                if(_sliderColorEnd == value) {
                    return;
                }

                _sliderColorEnd = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el color del canal cuando el control está deshabilitado.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante atenuada.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledChannelColor {
            get => _disabledChannelColor;
            set {
                if(_disabledChannelColor == value) {
                    return;
                }

                _disabledChannelColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el color del indicador cuando el control está deshabilitado.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante atenuada.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledSliderColor {
            get => _disabledSliderColor;
            set {
                if(_disabledSliderColor == value) {
                    return;
                }

                _disabledSliderColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el color del texto cuando el control está deshabilitado.
        /// <see cref="Color.Empty"/> utiliza <see cref="SystemColors.GrayText"/>.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledForeColor {
            get => _disabledForeColor;
            set {
                if(_disabledForeColor == value) {
                    return;
                }

                _disabledForeColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece la altura del canal, expresada en píxeles.
        /// Un valor de cero oculta el canal.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(6)]
        public int ChannelHeight {
            get => _channelHeight;
            set {
                EnsureNonNegative(value, nameof(ChannelHeight));

                if(_channelHeight == value) {
                    return;
                }

                _channelHeight = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece la altura del indicador, expresada en píxeles.
        /// Un valor de cero oculta el indicador.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(6)]
        public int SliderHeight {
            get => _sliderHeight;
            set {
                EnsureNonNegative(value, nameof(SliderHeight));

                if(_sliderHeight == value) {
                    return;
                }

                _sliderHeight = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece la posición utilizada para mostrar el valor.</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce al asignar una posición no definida.
        /// </exception>
        [Category("Sara UI Design")]
        [DefaultValue(TextPosition.Right)]
        public TextPosition ShowValue {
            get => _showValue;
            set {
                if(!Enum.IsDefined(typeof(TextPosition), value)) {
                    throw new ArgumentOutOfRangeException(
                        nameof(ShowValue), value, "La posición del texto no es compatible.");
                }

                if(_showValue == value) {
                    return;
                }

                _showValue = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el texto que precede al valor numérico.</summary>
        [Category("Sara UI Design")]
        [DefaultValue("")]
        public string SymbolBefore {
            get => _symbolBefore;
            set {
                string normalizedValue = value ?? string.Empty;

                if(_symbolBefore == normalizedValue) {
                    return;
                }

                _symbolBefore = normalizedValue;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el texto que sigue al valor numérico.</summary>
        [Category("Sara UI Design")]
        [DefaultValue("")]
        public string SymbolAfter {
            get => _symbolAfter;
            set {
                string normalizedValue = value ?? string.Empty;

                if(_symbolAfter == normalizedValue) {
                    return;
                }

                _symbolAfter = normalizedValue;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece si el texto incluye el valor máximo configurado.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(false)]
        public bool ShowMaximum {
            get => _showMaximum;
            set {
                if(_showMaximum == value) {
                    return;
                }

                _showMaximum = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Conserva el nombre utilizado por versiones anteriores para indicar si se muestra el máximo.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public bool ShowMaximun {
            get => ShowMaximum;
            set => ShowMaximum = value;
        }

        /// <summary>Obtiene o establece si los cambios de valor y el modo Marquee deben animarse.</summary>
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
                    ConfigureAnimationForStyle();
                } else {
                    ApplyCurrentTargetImmediately();
                }
            }
        }

        /// <summary>
        /// Obtiene o establece la duración de una transición de valor, expresada en milisegundos.
        /// Un valor de cero aplica inmediatamente el destino.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(500)]
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

        /// <summary>Obtiene o establece la curva aplicada a las animaciones del control.</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce al asignar una curva no definida.
        /// </exception>
        [Category("Sara UI Design")]
        [DefaultValue(SaraEasing.Linear)]
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

        /// <summary>
        /// Obtiene o establece la duración de un recorrido completo en modo Marquee, en milisegundos.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(1400)]
        public int MarqueeAnimationDuration {
            get => _marqueeAnimationDuration;
            set {
                EnsureNonNegative(value, nameof(MarqueeAnimationDuration));

                if(_marqueeAnimationDuration == value) {
                    return;
                }

                _marqueeAnimationDuration = value;

                if(base.Style == ProgressBarStyle.Marquee) {
                    StartMarqueeAnimation();
                }
            }
        }

        /// <summary>Obtiene o establece el ancho porcentual del segmento mostrado en modo Marquee.</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce al asignar un valor fuera del intervalo de 1 a 100.
        /// </exception>
        [Category("Sara UI Design")]
        [DefaultValue(30)]
        public int MarqueeSegmentPercentage {
            get => _marqueeSegmentPercentage;
            set {
                if(value < 1 || value > 100) {
                    throw new ArgumentOutOfRangeException(
                        nameof(MarqueeSegmentPercentage),
                        value,
                        "El segmento Marquee debe ocupar entre 1 y 100 por ciento del canal.");
                }

                if(_marqueeSegmentPercentage == value) {
                    return;
                }

                _marqueeSegmentPercentage = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene el estado actual del motor de animación interno.</summary>
        [Browsable(false)]
        public SaraAnimationState AnimationState => _animator.State;

        /// <summary>
        /// Obtiene el valor interpolado que se está representando. Puede diferir temporalmente de
        /// <see cref="Value"/> mientras una transición está en curso.
        /// </summary>
        [Browsable(false)]
        public float DisplayedValue => ClampToRange(_displayedValue);

        /// <summary>Obtiene o establece el valor lógico de destino del progreso.</summary>
        [Category("Behavior")]
        [DefaultValue(0)]
        [Bindable(true)]
        public new int Value {
            get => base.Value;
            set {
                int previousValue = base.Value;
                base.Value = value;
                HandleValueChange(previousValue);
            }
        }

        /// <summary>Obtiene o establece el límite inferior del progreso.</summary>
        [Category("Behavior")]
        [DefaultValue(0)]
        public new int Minimum {
            get => base.Minimum;
            set {
                base.Minimum = value;
                SynchronizeRangeChange();
            }
        }

        /// <summary>Obtiene o establece el límite superior del progreso.</summary>
        [Category("Behavior")]
        [DefaultValue(100)]
        public new int Maximum {
            get => base.Maximum;
            set {
                base.Maximum = value;
                SynchronizeRangeChange();
            }
        }

        /// <summary>
        /// Obtiene o establece el estilo. <see cref="ProgressBarStyle.Marquee"/> muestra un
        /// segmento indeterminado animado; los otros estilos representan el valor configurado.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(ProgressBarStyle.Blocks)]
        public new ProgressBarStyle Style {
            get => base.Style;
            set {
                if(!Enum.IsDefined(typeof(ProgressBarStyle), value)) {
                    throw new ArgumentOutOfRangeException(
                        nameof(Style), value, "El estilo de progreso no es compatible.");
                }

                if(base.Style == value) {
                    return;
                }

                base.Style = value;
                _lastKnownStyle = value;
                ConfigureAnimationForStyle();
            }
        }

        /// <summary>Obtiene o establece la fuente utilizada para dibujar el valor.</summary>
        [Category("Sara UI Design")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
#if NET8_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
        public override Font Font {
            get => base.Font;
            set => base.Font = value;
        }

        /// <summary>Obtiene o establece el color utilizado para dibujar el valor.</summary>
        [Category("Sara UI Design")]
        public override Color ForeColor {
            get => base.ForeColor;
            set => base.ForeColor = value;
        }

        /// <summary>Incrementa el valor lógico y anima la transición resultante.</summary>
        /// <param name="value">Cantidad que se suma al valor actual.</param>
        public new void Increment(int value) {
            int previousValue = base.Value;
            base.Increment(value);
            HandleValueChange(previousValue);
        }

        /// <summary>Avanza el progreso según la propiedad heredada <see cref="ProgressBar.Step"/>.</summary>
        public new void PerformStep() {
            int previousValue = base.Value;
            base.PerformStep();
            HandleValueChange(previousValue);
        }

        /// <summary>Pausa la animación activa conservando el avance mostrado.</summary>
        /// <returns><see langword="true"/> si la animación cambió al estado pausado.</returns>
        public bool PauseAnimation() => _animator.Pause();

        /// <summary>Reanuda una animación pausada desde el avance conservado.</summary>
        /// <returns><see langword="true"/> si la animación volvió a ejecutarse.</returns>
        public bool ResumeAnimation() => _animator.Resume();

        /// <summary>Detiene la animación y muestra inmediatamente el estado lógico actual.</summary>
        /// <returns><see langword="true"/> si se detuvo una animación activa.</returns>
        public bool StopAnimation() {
            bool stopped = _animator.Stop();
            _displayedValue = base.Value;
            _marqueeProgress = 0.5f;
            Invalidate();
            return stopped;
        }

        /// <inheritdoc/>
        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            _lastKnownValue = base.Value;
            _lastKnownStyle = base.Style;
            _displayedValue = base.Value;
            ConfigureAnimationForStyle();
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

            if(_disposingResources || IsDisposed) {
                return;
            }

            if(Visible) {
                if(!_animator.Resume()) {
                    ConfigureAnimationForStyle();
                }
            } else if(_animator.IsRunning) {
                _animator.Pause();
            }
        }

        /// <inheritdoc/>
        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnParentBackColorChanged(EventArgs e) {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnBackColorChanged(EventArgs e) {
            base.OnBackColorChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnFontChanged(EventArgs e) {
            base.OnFontChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnForeColorChanged(EventArgs e) {
            base.OnForeColorChanged(e);
            Invalidate();
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
        protected override void OnPaintBackground(PaintEventArgs pevent) {
            pevent.Graphics.Clear(ResolveBackgroundColor());
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e) {
            SynchronizeExternalChanges();

            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(ResolveBackgroundColor());

            Rectangle contentBounds = CreateContentBounds();

            if(contentBounds.Width <= 0 || contentBounds.Height <= 0) {
                return;
            }

            Color channelColor = ResolveChannelColor();
            Color sliderColor = ResolveSliderColor(_sliderColor);
            Color sliderColorEnd = ResolveSliderColor(_sliderColorEnd);

            DrawChannel(graphics, contentBounds, channelColor);

            Rectangle sliderBounds = base.Style == ProgressBarStyle.Marquee
                ? CreateMarqueeBounds(contentBounds)
                : CreateDeterminateBounds(contentBounds);

            DrawSlider(graphics, contentBounds, sliderBounds, sliderColor, sliderColorEnd);

            if(base.Style != ProgressBarStyle.Marquee && _showValue != TextPosition.None) {
                DrawValueText(graphics, contentBounds, sliderBounds);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            if(disposing) {
                _disposingResources = true;
                _animator.Completed -= Animator_Completed;
                _animator.Canceled -= Animator_Canceled;
                _animator.StateChanged -= Animator_StateChanged;
                _animator.Dispose();
                AnimationCompleted = null;
                AnimationCanceled = null;
                AnimationStateChanged = null;
            }

            base.Dispose(disposing);
        }

        private void HandleValueChange(int previousValue) {
            _lastKnownValue = base.Value;

            if(previousValue == base.Value) {
                return;
            }

            if(base.Style == ProgressBarStyle.Marquee) {
                Invalidate();
            } else {
                StartValueAnimation();
            }
        }

        private void ConfigureAnimationForStyle() {
            if(_disposingResources || IsDisposed) {
                return;
            }

            if(base.Style == ProgressBarStyle.Marquee) {
                StartMarqueeAnimation();
            } else {
                StartValueAnimation();
            }
        }

        private void RestartActiveAnimation() {
            if(_animator.IsRunning || _animator.IsPaused) {
                ConfigureAnimationForStyle();
            }
        }

        private void StartValueAnimation() {
            float destination = base.Value;
            _lastKnownValue = base.Value;

            if(!CanAnimate() || _animationDuration == 0 ||
                Math.Abs(_displayedValue - destination) < 0.001f) {
                StopAnimatorIfActive();
                _displayedValue = destination;
                Invalidate();
                return;
            }

            _animator.Start(
                _displayedValue,
                destination,
                value => {
                    _displayedValue = ClampToRange(value);
                    Invalidate();
                },
                new SaraAnimationOptions {
                    Duration = _animationDuration,
                    FrameInterval = _animationFrameInterval,
                    Easing = _animationEasing
                });
        }

        private void StartMarqueeAnimation() {
            if(!CanAnimate() || _marqueeAnimationDuration == 0) {
                StopAnimatorIfActive();
                _marqueeProgress = 0.5f;
                Invalidate();
                return;
            }

            _animator.Start(
                0f,
                1f,
                value => {
                    _marqueeProgress = value;
                    Invalidate();
                },
                new SaraAnimationOptions {
                    Duration = _marqueeAnimationDuration,
                    FrameInterval = _animationFrameInterval,
                    Easing = _animationEasing,
                    Repeat = true
                });
        }

        private void ApplyCurrentTargetImmediately() {
            StopAnimatorIfActive();
            _displayedValue = base.Value;
            _marqueeProgress = 0.5f;
            Invalidate();
        }

        private void StopAnimatorIfActive() {
            if(_animator.IsRunning || _animator.IsPaused) {
                _animator.Stop();
            }
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

        private void SynchronizeExternalChanges() {
            if(_lastKnownStyle != base.Style) {
                _lastKnownStyle = base.Style;
                _lastKnownValue = base.Value;
                ConfigureAnimationForStyle();
                return;
            }

            if(_lastKnownValue != base.Value) {
                int previousValue = _lastKnownValue;
                HandleValueChange(previousValue);
            }
        }

        private void SynchronizeRangeChange() {
            StopAnimatorIfActive();
            _lastKnownValue = base.Value;
            _displayedValue = base.Value;
            Invalidate();
        }

        private Rectangle CreateContentBounds() {
            int width = Math.Max(0, ClientSize.Width - Padding.Horizontal);
            int height = Math.Max(0, ClientSize.Height - Padding.Vertical);
            return new Rectangle(Padding.Left, Padding.Top, width, height);
        }

        private void DrawChannel(Graphics graphics, Rectangle contentBounds, Color color) {
            int height = Math.Min(_channelHeight, contentBounds.Height);

            if(height <= 0) {
                return;
            }

            Rectangle bounds = new Rectangle(
                contentBounds.Left,
                contentBounds.Top + ((contentBounds.Height - height) / 2),
                contentBounds.Width,
                height);

            using GraphicsPath path = CreateRoundedPath(bounds);
            using SolidBrush brush = new SolidBrush(color);
            graphics.FillPath(brush, path);
        }

        private void DrawSlider(
            Graphics graphics,
            Rectangle contentBounds,
            Rectangle sliderBounds,
            Color startColor,
            Color endColor) {
            if(_sliderHeight <= 0 || sliderBounds.Width <= 0) {
                return;
            }

            Rectangle visibleBounds = Rectangle.Intersect(contentBounds, sliderBounds);

            if(visibleBounds.Width <= 0 || visibleBounds.Height <= 0) {
                return;
            }

            using GraphicsPath path = CreateRoundedPath(visibleBounds);

            if(_useGradient && visibleBounds.Width > 1 && startColor != endColor) {
                Color leftColor = IsRightToLeftDirection() ? endColor : startColor;
                Color rightColor = IsRightToLeftDirection() ? startColor : endColor;
                using LinearGradientBrush brush = new LinearGradientBrush(
                    visibleBounds,
                    leftColor,
                    rightColor,
                    LinearGradientMode.Horizontal);
                graphics.FillPath(brush, path);
                return;
            }

            using SolidBrush solidBrush = new SolidBrush(startColor);
            graphics.FillPath(solidBrush, path);
        }

        private Rectangle CreateDeterminateBounds(Rectangle contentBounds) {
            double progress = CalculateNormalizedProgress();
            int width = (int)Math.Round(contentBounds.Width * progress, MidpointRounding.AwayFromZero);
            int height = Math.Min(_sliderHeight, contentBounds.Height);
            int x = IsRightToLeftDirection() ? contentBounds.Right - width : contentBounds.Left;
            int y = contentBounds.Top + ((contentBounds.Height - height) / 2);
            return new Rectangle(x, y, width, height);
        }

        private Rectangle CreateMarqueeBounds(Rectangle contentBounds) {
            int height = Math.Min(_sliderHeight, contentBounds.Height);
            int segmentWidth = Math.Max(
                1,
                (int)Math.Round(
                    contentBounds.Width * (_marqueeSegmentPercentage / 100d),
                    MidpointRounding.AwayFromZero));
            int travelDistance = contentBounds.Width + segmentWidth;
            int traveled = (int)Math.Round(travelDistance * _marqueeProgress, MidpointRounding.AwayFromZero);
            int x = IsRightToLeftDirection()
                ? contentBounds.Right - traveled
                : contentBounds.Left - segmentWidth + traveled;
            int y = contentBounds.Top + ((contentBounds.Height - height) / 2);
            return new Rectangle(x, y, segmentWidth, height);
        }

        private double CalculateNormalizedProgress() {
            double range = (double)base.Maximum - base.Minimum;

            if(range <= 0d) {
                return 0d;
            }

            double progress = (DisplayedValue - base.Minimum) / range;
            return Math.Max(0d, Math.Min(1d, progress));
        }

        private void DrawValueText(Graphics graphics, Rectangle contentBounds, Rectangle sliderBounds) {
            int displayedInteger = (int)Math.Round(DisplayedValue, MidpointRounding.AwayFromZero);
            string text = _symbolBefore + displayedInteger + _symbolAfter;

            if(_showMaximum) {
                text += "/" + _symbolBefore + base.Maximum + _symbolAfter;
            }

            TextFormatFlags flags = TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine |
                TextFormatFlags.VerticalCenter;
            Size measuredSize = TextRenderer.MeasureText(
                graphics,
                text,
                Font,
                contentBounds.Size,
                flags);
            int textWidth = Math.Min(measuredSize.Width, contentBounds.Width);
            int margin = Math.Min(4, Math.Max(0, contentBounds.Width - textWidth));
            int minimumX = contentBounds.Left;
            int maximumX = contentBounds.Right - textWidth;
            int x;

            switch(_showValue) {
                case TextPosition.Left:
                    x = contentBounds.Left + margin;
                    break;
                case TextPosition.Right:
                    x = contentBounds.Right - textWidth - margin;
                    break;
                case TextPosition.Center:
                    x = contentBounds.Left + ((contentBounds.Width - textWidth) / 2);
                    break;
                case TextPosition.Sliding:
                    x = IsRightToLeftDirection()
                        ? sliderBounds.Left + margin
                        : sliderBounds.Right - textWidth - margin;
                    break;
                default:
                    return;
            }

            x = Math.Max(minimumX, Math.Min(maximumX, x));
            Rectangle textBounds = new Rectangle(x, contentBounds.Top, textWidth, contentBounds.Height);
            Color textColor = Enabled
                ? ForeColor
                : (_disabledForeColor.IsEmpty ? SystemColors.GrayText : _disabledForeColor);
            TextRenderer.DrawText(graphics, text, Font, textBounds, textColor, flags);
        }

        private Color ResolveBackgroundColor() {
            if(BackColor != Color.Transparent) {
                return BackColor;
            }

            return Parent?.BackColor ?? SystemColors.Control;
        }

        private Color ResolveChannelColor() {
            if(Enabled) {
                return _channelColor;
            }

            return _disabledChannelColor.IsEmpty
                ? Blend(_channelColor, SystemColors.Control, 0.65f)
                : _disabledChannelColor;
        }

        private Color ResolveSliderColor(Color color) {
            if(Enabled) {
                return color;
            }

            return _disabledSliderColor.IsEmpty
                ? Blend(color, SystemColors.ControlDark, 0.65f)
                : _disabledSliderColor;
        }

        private bool IsRightToLeftDirection() => RightToLeft == RightToLeft.Yes;

        private float ClampToRange(float value) {
            return Math.Max(base.Minimum, Math.Min(base.Maximum, value));
        }

        private static GraphicsPath CreateRoundedPath(Rectangle bounds) {
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
            if(base.Style != ProgressBarStyle.Marquee) {
                _displayedValue = base.Value;
                Invalidate();
                AnimationCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Animator_Canceled(object? sender, EventArgs e) {
            AnimationCanceled?.Invoke(this, EventArgs.Empty);
        }

        private void Animator_StateChanged(object? sender, EventArgs e) {
            AnimationStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

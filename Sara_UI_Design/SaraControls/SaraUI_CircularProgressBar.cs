using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using Sara_UI_Design.Animations;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Representa una barra de progreso circular con animaciones, anillos personalizables
    /// y texto central con indicadores secundarios.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultBindingProperty(nameof(Value))]
    public class SaraUI_CircularProgressBar:ProgressBar {
        private readonly SaraAnimator _animator = new SaraAnimator();
        private Font? _ownedPrimaryFont;
        private Font _secondaryFont;
        private bool _ownsSecondaryFont;
        private bool _disposingResources;
        private float? _animatedValue;
        private int? _animatedStartAngle;
        private int _lastKnownValue;
        private ProgressBarStyle _lastKnownStyle;
        private SaraEasing _animationFunction = SaraEasing.Linear;
        private int _animationSpeed = 500;
        private int _animationFrameInterval = 15;
        private int _marqueeAnimationDuration = 2000;
        private bool _animationEnabled = true;
        private Color _innerColor = Color.FromArgb(245, 245, 250);
        private int _innerMargin = 2;
        private Color _outerColor = Color.FromArgb(230, 230, 240);
        private int _outerWidth = 12;
        private Color _progressColor = Color.MediumSlateBlue;
        private int _progressWidth = 12;
        private int _startAngle = 270;
        private Color _subscriptColor = Color.FromArgb(166, 166, 166);
        private Padding _subscriptMargin = new Padding(10, -25, 0, 0);
        private string _subscriptText = ".00";
        private Color _superscriptColor = Color.FromArgb(166, 166, 166);
        private Padding _superscriptMargin = new Padding(10, 25, 0, 0);
        private string _superscriptText = "%";
        private Padding _textMargin;

        /// <summary>
        /// Inicializa una nueva instancia del control de progreso circular.
        /// </summary>
        public SaraUI_CircularProgressBar() {
            SetStyle(
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            _animator.Completed += Animator_Completed;
            _animator.StateChanged += Animator_StateChanged;

            BackColor = Color.Transparent;
            ForeColor = Color.FromArgb(64, 64, 64);

            _ownedPrimaryFont = new Font("Segoe UI", 24f, FontStyle.Bold);
            Font = _ownedPrimaryFont;

            _secondaryFont = new Font("Segoe UI", 12f, FontStyle.Regular);
            _ownsSecondaryFont = true;

            Size = new Size(150, 150);
            _lastKnownValue = base.Value;
            _lastKnownStyle = base.Style;
            _animatedValue = base.Value;
        }

        /// <summary>
        /// Se produce cuando cambia el estado de la animación interna.
        /// </summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationStateChanged;

        /// <summary>
        /// Obtiene o establece la curva utilizada para interpolar el progreso y el movimiento Marquee.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce al asignar un valor que no representa una curva conocida.
        /// </exception>
        [Category("Sara UI Design")]
        [DefaultValue(SaraEasing.Linear)]
        public SaraEasing AnimationFunction {
            get => _animationFunction;
            set {
                if(!Enum.IsDefined(typeof(SaraEasing), value)) {
                    throw new ArgumentOutOfRangeException(
                        nameof(AnimationFunction),
                        value,
                        "La curva de animación indicada no es compatible.");
                }

                if(_animationFunction == value) {
                    return;
                }

                _animationFunction = value;
                RestartCurrentAnimation();
            }
        }

        /// <summary>
        /// Obtiene o establece la duración, en milisegundos, de las transiciones de valor.
        /// Un valor de cero desactiva la interpolación y aplica el valor inmediatamente.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(500)]
        public int AnimationSpeed {
            get => _animationSpeed;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(
                        nameof(AnimationSpeed),
                        value,
                        "La duración de la animación no puede ser negativa.");
                }

                if(_animationSpeed == value) {
                    return;
                }

                _animationSpeed = value;

                if(base.Style != ProgressBarStyle.Marquee) {
                    StartValueAnimation();
                }
            }
        }

        /// <summary>
        /// Obtiene o establece el intervalo solicitado entre fotogramas, expresado en milisegundos.
        /// </summary>
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
                RestartCurrentAnimation();
            }
        }

        /// <summary>
        /// Obtiene o establece la duración, en milisegundos, de una vuelta completa en modo Marquee.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(2000)]
        public int MarqueeAnimationDuration {
            get => _marqueeAnimationDuration;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(
                        nameof(MarqueeAnimationDuration),
                        value,
                        "La duración de la animación Marquee no puede ser negativa.");
                }

                if(_marqueeAnimationDuration == value) {
                    return;
                }

                _marqueeAnimationDuration = value;

                if(base.Style == ProgressBarStyle.Marquee) {
                    StartMarqueeAnimation();
                }
            }
        }

        /// <summary>
        /// Obtiene o establece si el control debe animar sus cambios.
        /// </summary>
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
                    StopAnimation();
                }
            }
        }

        /// <summary>
        /// Obtiene el estado actual de la animación interna.
        /// </summary>
        [Browsable(false)]
        public SaraAnimationState AnimationState => _animator.State;

        /// <summary>
        /// Obtiene o establece el valor actual del progreso.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(0)]
        [Bindable(true)]
        public new int Value {
            get => base.Value;
            set {
                int previousValue = base.Value;
                base.Value = value;

                if(previousValue == base.Value) {
                    return;
                }

                _lastKnownValue = base.Value;

                if(base.Style != ProgressBarStyle.Marquee) {
                    StartValueAnimation();
                } else {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Obtiene o establece el límite inferior del progreso.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(0)]
        public new int Minimum {
            get => base.Minimum;
            set {
                base.Minimum = value;
                SynchronizeValueAfterRangeChange();
            }
        }

        /// <summary>
        /// Obtiene o establece el límite superior del progreso.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(100)]
        public new int Maximum {
            get => base.Maximum;
            set {
                base.Maximum = value;
                SynchronizeValueAfterRangeChange();
            }
        }

        /// <summary>
        /// Obtiene o establece el estilo de progreso. El modo Marquee produce una rotación continua.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(ProgressBarStyle.Blocks)]
        public new ProgressBarStyle Style {
            get => base.Style;
            set {
                if(base.Style == value) {
                    return;
                }

                base.Style = value;
                _lastKnownStyle = value;
                ConfigureAnimationForStyle();
            }
        }

        /// <summary>
        /// Obtiene o establece el color del círculo central. Use <see cref="Color.Transparent"/> para mostrar solo el anillo.
        /// </summary>
        [Category("Sara UI Design")]
        public Color InnerColor {
            get => _innerColor;
            set {
                if(_innerColor == value) {
                    return;
                }

                _innerColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece la separación entre el anillo exterior y el círculo interno.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(2)]
        public int InnerMargin {
            get => _innerMargin;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(nameof(InnerMargin), value, "El margen interior no puede ser negativo.");
                }

                if(_innerMargin == value) {
                    return;
                }

                _innerMargin = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el color del anillo estático de fondo.
        /// </summary>
        [Category("Sara UI Design")]
        public Color OuterColor {
            get => _outerColor;
            set {
                if(_outerColor == value) {
                    return;
                }

                _outerColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el grosor del anillo estático de fondo.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(12)]
        public int OuterWidth {
            get => _outerWidth;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(nameof(OuterWidth), value, "El grosor exterior no puede ser negativo.");
                }

                if(_outerWidth == value) {
                    return;
                }

                _outerWidth = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el color del arco que representa el progreso.
        /// </summary>
        [Category("Sara UI Design")]
        public Color ProgressColor {
            get => _progressColor;
            set {
                if(_progressColor == value) {
                    return;
                }

                _progressColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el grosor del arco que representa el progreso.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(12)]
        public int ProgressWidth {
            get => _progressWidth;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(nameof(ProgressWidth), value, "El grosor del progreso no puede ser negativo.");
                }

                if(_progressWidth == value) {
                    return;
                }

                _progressWidth = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece la fuente de los indicadores secundario y superior.
        /// La fuente predeterminada es propiedad del control; una fuente asignada externamente sigue siendo propiedad del llamador.
        /// </summary>
        /// <exception cref="ArgumentNullException">Se produce al asignar <see langword="null"/>.</exception>
        [Category("Sara UI Design")]
        public Font SecondaryFont {
            get => _secondaryFont;
            set {
                if(value is null) {
                    throw new ArgumentNullException(nameof(SecondaryFont));
                }

                if(ReferenceEquals(_secondaryFont, value)) {
                    return;
                }

                DisposeOwnedSecondaryFont();
                _secondaryFont = value;
                _ownsSecondaryFont = false;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el ángulo inicial, en grados, del arco de progreso.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(270)]
        public int StartAngle {
            get => _startAngle;
            set {
                if(_startAngle == value) {
                    return;
                }

                _startAngle = value;

                if(base.Style == ProgressBarStyle.Marquee) {
                    StartMarqueeAnimation();
                } else {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Obtiene o establece el color del texto inferior secundario.
        /// </summary>
        [Category("Sara UI Design")]
        public Color SubscriptColor {
            get => _subscriptColor;
            set {
                if(_subscriptColor == value) {
                    return;
                }

                _subscriptColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el desplazamiento del texto inferior secundario.
        /// </summary>
        [Category("Sara UI Design")]
        public Padding SubscriptMargin {
            get => _subscriptMargin;
            set {
                if(_subscriptMargin == value) {
                    return;
                }

                _subscriptMargin = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el texto inferior secundario.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(".00")]
        public string SubscriptText {
            get => _subscriptText;
            set {
                string normalizedValue = value ?? string.Empty;

                if(_subscriptText == normalizedValue) {
                    return;
                }

                _subscriptText = normalizedValue;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el color del texto superior secundario.
        /// </summary>
        [Category("Sara UI Design")]
        public Color SuperscriptColor {
            get => _superscriptColor;
            set {
                if(_superscriptColor == value) {
                    return;
                }

                _superscriptColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el desplazamiento del texto superior secundario.
        /// </summary>
        [Category("Sara UI Design")]
        public Padding SuperscriptMargin {
            get => _superscriptMargin;
            set {
                if(_superscriptMargin == value) {
                    return;
                }

                _superscriptMargin = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el texto superior secundario.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue("%")]
        public string SuperscriptText {
            get => _superscriptText;
            set {
                string normalizedValue = value ?? string.Empty;

                if(_superscriptText == normalizedValue) {
                    return;
                }

                _superscriptText = normalizedValue;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el desplazamiento del texto principal.
        /// </summary>
        [Category("Sara UI Design")]
        public Padding TextMargin {
            get => _textMargin;
            set {
                if(_textMargin == value) {
                    return;
                }

                _textMargin = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Pausa la animación activa conservando el progreso alcanzado.
        /// </summary>
        /// <returns><see langword="true"/> si la animación cambió al estado pausado.</returns>
        public bool PauseAnimation() => _animator.Pause();

        /// <summary>
        /// Reanuda una animación pausada.
        /// </summary>
        /// <returns><see langword="true"/> si la animación volvió a ejecutarse.</returns>
        public bool ResumeAnimation() => _animator.Resume();

        /// <summary>
        /// Detiene la animación y muestra inmediatamente el estado actual del control.
        /// </summary>
        /// <returns><see langword="true"/> si se detuvo una animación activa.</returns>
        public bool StopAnimation() {
            bool stopped = _animator.Stop();
            _animatedValue = base.Value;
            _animatedStartAngle = base.Style == ProgressBarStyle.Marquee
                ? NormalizeAngle(StartAngle)
                : null;
            Invalidate();
            return stopped;
        }

        /// <inheritdoc/>
        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            _lastKnownValue = base.Value;
            _lastKnownStyle = base.Style;
            _animatedValue = base.Value;
            ConfigureAnimationForStyle();
        }

        /// <inheritdoc/>
        protected override void OnHandleDestroyed(EventArgs e) {
            if(!_disposingResources) {
                _animator.Stop();
            }

            base.OnHandleDestroyed(e);
        }

        /// <inheritdoc/>
        protected override void OnVisibleChanged(EventArgs e) {
            base.OnVisibleChanged(e);

            if(_disposingResources || IsInDesignMode()) {
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
        protected override void OnTextChanged(EventArgs e) {
            base.OnTextChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnFontChanged(EventArgs e) {
            base.OnFontChanged(e);

            if(_ownedPrimaryFont is not null && !ReferenceEquals(Font, _ownedPrimaryFont)) {
                _ownedPrimaryFont.Dispose();
                _ownedPrimaryFont = null;
            }

            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnForeColorChanged(EventArgs e) {
            base.OnForeColorChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnBackColorChanged(EventArgs e) {
            base.OnBackColorChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnParentBackColorChanged(EventArgs e) {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e) {
            SynchronizeExternalPropertyChanges();
            StartPaint(e.Graphics);
        }

        /// <summary>
        /// Dibuja el canal, el progreso y los textos del control.
        /// </summary>
        /// <param name="graphics">Superficie donde se dibujará el control.</param>
        protected virtual void StartPaint(Graphics graphics) {
            if(graphics is null) {
                throw new ArgumentNullException(nameof(graphics));
            }

            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(ResolveBackgroundColor());

            int maxPenWidth = Math.Max(OuterWidth, ProgressWidth);
            float padding = (maxPenWidth / 2f) + 2f;
            RectangleF ringArea = new RectangleF(
                padding,
                padding,
                Width - (padding * 2f),
                Height - (padding * 2f));

            if(ringArea.Width <= 0f || ringArea.Height <= 0f) {
                return;
            }

            if(OuterColor != Color.Transparent && OuterWidth > 0) {
                using Pen outerPen = new Pen(OuterColor, OuterWidth);
                graphics.DrawEllipse(outerPen, ringArea);
            }

            if(InnerColor != Color.Transparent) {
                float innerOffset = (maxPenWidth / 2f) + InnerMargin;
                RectangleF innerArea = RectangleF.Inflate(ringArea, -innerOffset, -innerOffset);

                if(innerArea.Width > 0f && innerArea.Height > 0f) {
                    using SolidBrush innerBrush = new SolidBrush(InnerColor);
                    graphics.FillEllipse(innerBrush, innerArea);
                }
            }

            float sweepAngle = CalculateSweepAngle();

            if(sweepAngle > 0f && ProgressWidth > 0) {
                using Pen progressPen = new Pen(ProgressColor, ProgressWidth) {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round,
                    LineJoin = LineJoin.Round
                };

                graphics.DrawArc(
                    progressPen,
                    ringArea,
                    _animatedStartAngle ?? StartAngle,
                    sweepAngle);
            }

            DrawTexts(graphics);
        }

        /// <summary>
        /// Conserva el punto de extensión utilizado por versiones anteriores para iniciar una transición de valor.
        /// </summary>
        /// <param name="firstTime">Indica si se solicita una inicialización forzada.</param>
        [Obsolete("Asigne la propiedad Value para iniciar la animación.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void InitializeContinues(bool firstTime) => StartValueAnimation(firstTime);

        /// <summary>
        /// Conserva el punto de extensión utilizado por versiones anteriores para iniciar el modo Marquee.
        /// </summary>
        /// <param name="firstTime">Indica si se solicita una inicialización forzada.</param>
        [Obsolete("Asigne ProgressBarStyle.Marquee a la propiedad Style para iniciar la animación.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void InitializeMarquee(bool firstTime) {
            if(firstTime || !_animator.IsRunning) {
                StartMarqueeAnimation();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            if(disposing) {
                _disposingResources = true;
                _animator.Completed -= Animator_Completed;
                _animator.StateChanged -= Animator_StateChanged;
                _animator.Dispose();

                _ownedPrimaryFont?.Dispose();
                _ownedPrimaryFont = null;
                DisposeOwnedSecondaryFont();
                AnimationStateChanged = null;
            }

            base.Dispose(disposing);
        }

        private void ConfigureAnimationForStyle() {
            if(base.Style == ProgressBarStyle.Marquee) {
                StartMarqueeAnimation();
            } else {
                _animatedStartAngle = null;
                StartValueAnimation();
            }
        }

        private void RestartCurrentAnimation() {
            if(_animator.IsRunning || _animator.IsPaused) {
                ConfigureAnimationForStyle();
            }
        }

        private void StartValueAnimation(bool force = false) {
            _animatedStartAngle = null;
            float startingValue = _animatedValue ?? base.Value;

            if(!CanAnimate() || AnimationSpeed == 0 || (!force && Math.Abs(startingValue - base.Value) < 0.001f)) {
                _animator.Stop();
                _animatedValue = base.Value;
                Invalidate();
                return;
            }

            _animator.Start(
                startingValue,
                base.Value,
                value => {
                    _animatedValue = value;
                    Invalidate();
                },
                new SaraAnimationOptions {
                    Duration = AnimationSpeed,
                    FrameInterval = AnimationFrameInterval,
                    Easing = AnimationFunction
                });
        }

        private void StartMarqueeAnimation() {
            _animatedValue = base.Value;
            _animatedStartAngle = NormalizeAngle(StartAngle);

            if(!CanAnimate() || MarqueeAnimationDuration == 0) {
                _animator.Stop();
                Invalidate();
                return;
            }

            _animator.Start(
                0f,
                360f,
                value => {
                    _animatedStartAngle = NormalizeAngle(StartAngle + (int)Math.Round(value));
                    Invalidate();
                },
                new SaraAnimationOptions {
                    Duration = MarqueeAnimationDuration,
                    FrameInterval = AnimationFrameInterval,
                    Easing = AnimationFunction,
                    Repeat = true
                });
        }

        private bool CanAnimate() {
            return AnimationEnabled &&
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

        private void SynchronizeExternalPropertyChanges() {
            if(_lastKnownStyle != base.Style) {
                _lastKnownStyle = base.Style;
                ConfigureAnimationForStyle();
                return;
            }

            if(_lastKnownValue != base.Value) {
                _lastKnownValue = base.Value;

                if(base.Style != ProgressBarStyle.Marquee) {
                    StartValueAnimation();
                }
            }
        }

        private void SynchronizeValueAfterRangeChange() {
            _lastKnownValue = base.Value;

            if(_animatedValue.HasValue &&
                (_animatedValue.Value < base.Minimum || _animatedValue.Value > base.Maximum)) {
                _animatedValue = base.Value;
            }

            Invalidate();
        }

        private float CalculateSweepAngle() {
            int range = base.Maximum - base.Minimum;

            if(range <= 0) {
                return 0f;
            }

            float displayedValue = _animatedValue ?? base.Value;
            float normalizedValue = (displayedValue - base.Minimum) / range;
            return Math.Max(0f, Math.Min(1f, normalizedValue)) * 360f;
        }

        private void DrawTexts(Graphics graphics) {
            using StringFormat format = new StringFormat {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            if(!string.IsNullOrEmpty(Text)) {
                RectangleF textArea = CreateTextArea(TextMargin);
                using SolidBrush textBrush = new SolidBrush(ForeColor);
                graphics.DrawString(Text, Font, textBrush, textArea, format);
            }

            if(!string.IsNullOrEmpty(SuperscriptText)) {
                RectangleF superscriptArea = CreateTextArea(SuperscriptMargin);
                using SolidBrush superscriptBrush = new SolidBrush(SuperscriptColor);
                graphics.DrawString(SuperscriptText, SecondaryFont, superscriptBrush, superscriptArea, format);
            }

            if(!string.IsNullOrEmpty(SubscriptText)) {
                RectangleF subscriptArea = CreateTextArea(SubscriptMargin);
                using SolidBrush subscriptBrush = new SolidBrush(SubscriptColor);
                graphics.DrawString(SubscriptText, SecondaryFont, subscriptBrush, subscriptArea, format);
            }
        }

        private RectangleF CreateTextArea(Padding margin) {
            return new RectangleF(
                margin.Left,
                margin.Top,
                Math.Max(0, Width - margin.Horizontal),
                Math.Max(0, Height - margin.Vertical));
        }

        private Color ResolveBackgroundColor() {
            if(BackColor != Color.Transparent) {
                return BackColor;
            }

            return Parent?.BackColor ?? SystemColors.Control;
        }

        private void Animator_Completed(object? sender, EventArgs e) {
            if(base.Style != ProgressBarStyle.Marquee) {
                _animatedValue = base.Value;
                Invalidate();
            }
        }

        private void Animator_StateChanged(object? sender, EventArgs e) {
            AnimationStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void DisposeOwnedSecondaryFont() {
            if(_ownsSecondaryFont) {
                _secondaryFont.Dispose();
                _ownsSecondaryFont = false;
            }
        }

        private static int NormalizeAngle(int angle) {
            int normalizedAngle = angle % 360;
            return normalizedAngle < 0 ? normalizedAngle + 360 : normalizedAngle;
        }
    }
}

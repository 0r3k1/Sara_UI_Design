using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sara_UI_Design.Animations;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Control de pestañas con selección animada, estados de interacción, navegación
    /// accesible y cierre opcional cancelable.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent(nameof(SelectedIndexChanged))]
    public class SaraUI_TabControl:TabControl {
        /// <summary>Describe el estado de interacción actual del encabezado de pestañas.</summary>
        public enum SaraTabVisualState {
            /// <summary>No existe una interacción activa.</summary>
            Normal,

            /// <summary>El puntero se encuentra sobre una pestaña.</summary>
            Hovered,

            /// <summary>Una pestaña o su botón de cierre se están presionando.</summary>
            Pressed,

            /// <summary>El control tiene el foco de teclado.</summary>
            Focused,

            /// <summary>El control completo está deshabilitado.</summary>
            Disabled
        }

        /// <summary>Proporciona datos para el evento cancelable <see cref="TabClosing"/>.</summary>
        public sealed class SaraTabClosingEventArgs:CancelEventArgs {
            internal SaraTabClosingEventArgs(TabPage tabPage, int tabIndex) {
                TabPage = tabPage;
                TabIndex = tabIndex;
            }

            /// <summary>Obtiene la página que se solicita cerrar.</summary>
            public TabPage TabPage { get; }

            /// <summary>Obtiene el índice que tenía la página al solicitar el cierre.</summary>
            public int TabIndex { get; }
        }

        /// <summary>Proporciona datos para el evento <see cref="TabClosed"/>.</summary>
        public sealed class SaraTabClosedEventArgs:EventArgs {
            internal SaraTabClosedEventArgs(TabPage tabPage, int formerTabIndex) {
                TabPage = tabPage;
                FormerTabIndex = formerTabIndex;
            }

            /// <summary>Obtiene la página retirada de la colección.</summary>
            public TabPage TabPage { get; }

            /// <summary>Obtiene el índice que tenía la página antes de retirarse.</summary>
            public int FormerTabIndex { get; }
        }

        private readonly SaraAnimator _selectionAnimator;
        private readonly SaraAnimator _appearanceAnimator;
        private Color _selectedTabColor = Color.MediumSlateBlue;
        private Color _unselectedTabColor = Color.FromArgb(230, 230, 240);
        private Color _selectedTextColor = Color.White;
        private Color _unselectedTextColor = Color.DimGray;
        private Color _hoverTabColor = Color.FromArgb(215, 215, 235);
        private Color _pressedTabColor = Color.Empty;
        private Color _disabledTabColor = Color.Empty;
        private Color _disabledTextColor = Color.Empty;
        private Color _contentBackColor = Color.White;
        private Color _indicatorColor = Color.White;
        private Color _focusBorderColor = Color.HotPink;
        private Color _closeButtonColor = Color.Empty;
        private Color _closeButtonHoverColor = Color.Firebrick;
        private int _tabRadius = 10;
        private int _tabHeight = 35;
        private int _indicatorThickness = 3;
        private int _indicatorInset = 15;
        private int _closeButtonSize = 12;
        private bool _stretchTabs;
        private bool _showFocusBorder = true;
        private bool _showCloseButtons;
        private bool _animationEnabled = true;
        private int _animationDuration = 220;
        private int _animationFrameInterval = 15;
        private SaraEasing _animationEasing = SaraEasing.EaseOutCubic;
        private int _hoverIndex = -1;
        private int _pressedTabIndex = -1;
        private int _hoverCloseIndex = -1;
        private int _pressedCloseIndex = -1;
        private bool _updatingTabSize;
        private bool _initialized;
        private bool _disposingResources;
        private SaraTabVisualState _visualState;
        private SaraTabVisualState _appearanceVisualState;
        private int _appearanceInteractionIndex = -1;
        private int _appearanceSelectedIndex = -1;
        private Color[] _originTabColors = Array.Empty<Color>();
        private Color[] _originTextColors = Array.Empty<Color>();
        private float _appearanceProgress = 1f;
        private RectangleF _displayedIndicatorBounds = RectangleF.Empty;
        private RectangleF _targetIndicatorBounds = RectangleF.Empty;
        private float _displayedSelectedTabIndex = -1f;
        private SaraAnimationState _animationState;

        /// <summary>Inicializa un control de pestañas con dibujo propio y doble búfer.</summary>
        public SaraUI_TabControl() {
            _selectionAnimator = new SaraAnimator();
            _appearanceAnimator = new SaraAnimator();
            _selectionAnimator.Completed += SelectionAnimator_Completed;
            _selectionAnimator.Canceled += Animator_Canceled;
            _selectionAnimator.StateChanged += Animator_StateChanged;
            _appearanceAnimator.Completed += AppearanceAnimator_Completed;
            _appearanceAnimator.Canceled += Animator_Canceled;
            _appearanceAnimator.StateChanged += Animator_StateChanged;

            DrawMode = TabDrawMode.OwnerDrawFixed;
            Padding = new Point(20, 10);
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.UserPaint,
                true);

            _initialized = true;
            _visualState = DetermineVisualState();
            _appearanceVisualState = _visualState;
            _appearanceInteractionIndex = DetermineInteractionIndex(_visualState);
            _appearanceSelectedIndex = SelectedIndex;
            _displayedSelectedTabIndex = SelectedIndex;
            _animationState = ResolveAnimationState();
        }

        /// <summary>Se produce cuando cambia el estado visual de interacción.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? VisualStateChanged;

        /// <summary>Se produce cuando una transición interna llega a su destino.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationCompleted;

        /// <summary>Se produce cuando una transición interna se cancela o se reemplaza.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationCanceled;

        /// <summary>Se produce cuando cambia el estado agregado de las animaciones internas.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationStateChanged;

        /// <summary>Se produce antes de retirar una página y permite cancelar la operación.</summary>
        [Category("Sara UI Design Logic")]
        public event EventHandler<SaraTabClosingEventArgs>? TabClosing;

        /// <summary>Se produce después de retirar una página de la colección.</summary>
        [Category("Sara UI Design Logic")]
        public event EventHandler<SaraTabClosedEventArgs>? TabClosed;

        /// <summary>Obtiene o establece el color de fondo aplicado a las páginas.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "White")]
        public Color ContentBackColor {
            get => _contentBackColor;
            set {
                if(_contentBackColor == value) {
                    return;
                }

                _contentBackColor = value;
                UpdateTabPagesColor();
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el color de la pestaña seleccionada.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "MediumSlateBlue")]
        public Color SelectedTabColor {
            get => _selectedTabColor;
            set => SetAppearanceColor(ref _selectedTabColor, value);
        }

        /// <summary>Obtiene o establece el color de las pestañas no seleccionadas.</summary>
        [Category("Sara UI Design")]
        public Color UnselectedTabColor {
            get => _unselectedTabColor;
            set => SetAppearanceColor(ref _unselectedTabColor, value);
        }

        /// <summary>Obtiene o establece el color del texto de la pestaña seleccionada.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "White")]
        public Color SelectedTextColor {
            get => _selectedTextColor;
            set => SetAppearanceColor(ref _selectedTextColor, value);
        }

        /// <summary>Obtiene o establece el color del texto de las pestañas no seleccionadas.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "DimGray")]
        public Color UnselectedTextColor {
            get => _unselectedTextColor;
            set => SetAppearanceColor(ref _unselectedTextColor, value);
        }

        /// <summary>Obtiene o establece el color de una pestaña bajo el puntero.</summary>
        [Category("Sara UI Design")]
        public Color HoverTabColor {
            get => _hoverTabColor;
            set => SetAppearanceColor(ref _hoverTabColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color de una pestaña presionada.
        /// <see cref="Color.Empty"/> calcula automáticamente una variante más oscura.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color PressedTabColor {
            get => _pressedTabColor;
            set => SetAppearanceColor(ref _pressedTabColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color de una pestaña deshabilitada.
        /// <see cref="Color.Empty"/> calcula automáticamente un color atenuado.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledTabColor {
            get => _disabledTabColor;
            set => SetAppearanceColor(ref _disabledTabColor, value);
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

        /// <summary>Obtiene o establece el color del indicador de selección.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "White")]
        public Color IndicatorColor {
            get => _indicatorColor;
            set {
                if(_indicatorColor == value) {
                    return;
                }

                _indicatorColor = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el color de la guía de foco.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "HotPink")]
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
        /// Obtiene o establece el color de los botones de cierre.
        /// <see cref="Color.Empty"/> utiliza el color del texto de cada pestaña.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color CloseButtonColor {
            get => _closeButtonColor;
            set {
                if(_closeButtonColor == value) {
                    return;
                }

                _closeButtonColor = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el color del botón de cierre bajo el puntero.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Firebrick")]
        public Color CloseButtonHoverColor {
            get => _closeButtonHoverColor;
            set {
                if(_closeButtonHoverColor == value) {
                    return;
                }

                _closeButtonHoverColor = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el radio de las esquinas de cada pestaña.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(10)]
        public int TabRadius {
            get => _tabRadius;
            set {
                EnsureNonNegative(value, nameof(TabRadius));

                if(_tabRadius == value) {
                    return;
                }

                _tabRadius = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el grosor de los encabezados cuando se usa <see cref="StretchTabs"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor menor que dieciséis.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(35)]
        public int TabHeight {
            get => _tabHeight;
            set {
                if(value < 16) {
                    throw new ArgumentOutOfRangeException(
                        nameof(TabHeight),
                        value,
                        "El grosor del encabezado debe ser de al menos dieciséis píxeles.");
                }

                if(_tabHeight == value) {
                    return;
                }

                _tabHeight = value;
                UpdateTabSize();
                UpdateSelectionIndicator(animate: false);
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el grosor del indicador de selección.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor menor que uno.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(3)]
        public int IndicatorThickness {
            get => _indicatorThickness;
            set {
                if(value < 1) {
                    throw new ArgumentOutOfRangeException(
                        nameof(IndicatorThickness),
                        value,
                        "El indicador debe medir al menos un píxel.");
                }

                if(_indicatorThickness == value) {
                    return;
                }

                _indicatorThickness = value;
                UpdateSelectionIndicator(animate: false);
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el margen del indicador respecto a los extremos de la pestaña.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(15)]
        public int IndicatorInset {
            get => _indicatorInset;
            set {
                EnsureNonNegative(value, nameof(IndicatorInset));

                if(_indicatorInset == value) {
                    return;
                }

                _indicatorInset = value;
                UpdateSelectionIndicator(animate: false);
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el lado de los botones de cierre, expresado en píxeles.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor menor que ocho.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(12)]
        public int CloseButtonSize {
            get => _closeButtonSize;
            set {
                if(value < 8) {
                    throw new ArgumentOutOfRangeException(
                        nameof(CloseButtonSize),
                        value,
                        "El botón de cierre debe medir al menos ocho píxeles.");
                }

                if(_closeButtonSize == value) {
                    return;
                }

                _closeButtonSize = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece si las pestañas deben repartirse el espacio disponible.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(false)]
        public bool StretchTabs {
            get => _stretchTabs;
            set {
                if(_stretchTabs == value) {
                    return;
                }

                _stretchTabs = value;
                SizeMode = value ? TabSizeMode.Fixed : TabSizeMode.Normal;
                UpdateTabSize();
                UpdateSelectionIndicator(animate: false);
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece si se dibuja una guía alrededor de la pestaña enfocada.</summary>
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

        /// <summary>Obtiene o establece si cada pestaña muestra un botón para solicitar su cierre.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(false)]
        public bool ShowCloseButtons {
            get => _showCloseButtons;
            set {
                if(_showCloseButtons == value) {
                    return;
                }

                _showCloseButtons = value;
                ResetPointerState();
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece si los cambios de selección e interacción deben animarse.</summary>
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
                    UpdateSelectionIndicator(animate: true);
                    UpdateAppearance(animate: true);
                } else {
                    ApplySelectionImmediately();
                    ApplyAppearanceImmediately();
                }
            }
        }

        /// <summary>Obtiene o establece la duración de una transición, expresada en milisegundos.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(220)]
        public int AnimationDuration {
            get => _animationDuration;
            set {
                EnsureNonNegative(value, nameof(AnimationDuration));

                if(_animationDuration == value) {
                    return;
                }

                _animationDuration = value;
                RestartActiveAnimations();
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
                RestartActiveAnimations();
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
                RestartActiveAnimations();
            }
        }

        /// <summary>Obtiene el índice de la pestaña que se encuentra bajo el puntero.</summary>
        [Browsable(false)]
        public int HoveredTabIndex => _hoverIndex;

        /// <summary>Obtiene el índice de la pestaña que se está presionando.</summary>
        [Browsable(false)]
        public int PressedTabIndex => _pressedCloseIndex >= 0 ? _pressedCloseIndex : _pressedTabIndex;

        /// <summary>Obtiene el estado visual actual del encabezado.</summary>
        [Browsable(false)]
        public SaraTabVisualState VisualState => _visualState;

        /// <summary>Obtiene el estado agregado de las animaciones internas.</summary>
        [Browsable(false)]
        public SaraAnimationState AnimationState => _animationState;

        /// <summary>Obtiene el estado de la transición del indicador de selección.</summary>
        [Browsable(false)]
        public SaraAnimationState SelectionAnimationState => _selectionAnimator.State;

        /// <summary>Obtiene el estado de la transición de colores.</summary>
        [Browsable(false)]
        public SaraAnimationState AppearanceAnimationState => _appearanceAnimator.State;

        /// <summary>Obtiene la posición interpolada del indicador expresada como índice decimal.</summary>
        [Browsable(false)]
        public float DisplayedSelectedTabIndex => _displayedSelectedTabIndex;

        /// <summary>Solicita el cierre de la página situada en el índice indicado.</summary>
        /// <param name="tabIndex">Índice de la página que debe retirarse.</param>
        /// <returns><see langword="true"/> si la página se retiró de la colección.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Se produce cuando el índice no pertenece a la colección.</exception>
        public bool CloseTab(int tabIndex) {
            if(tabIndex < 0 || tabIndex >= TabCount) {
                throw new ArgumentOutOfRangeException(
                    nameof(tabIndex),
                    tabIndex,
                    "El índice no pertenece a la colección de pestañas.");
            }

            TabPage page = TabPages[tabIndex];
            SaraTabClosingEventArgs closingArguments = new SaraTabClosingEventArgs(page, tabIndex);
            TabClosing?.Invoke(this, closingArguments);

            if(closingArguments.Cancel || !TabPages.Contains(page)) {
                return false;
            }

            TabPages.Remove(page);
            TabClosed?.Invoke(this, new SaraTabClosedEventArgs(page, tabIndex));
            return true;
        }

        /// <summary>Selecciona la siguiente página habilitada en la dirección indicada.</summary>
        /// <param name="forward"><see langword="true"/> para avanzar; <see langword="false"/> para retroceder.</param>
        /// <param name="wrap">Indica si la búsqueda debe continuar desde el extremo opuesto.</param>
        /// <returns><see langword="true"/> si se seleccionó una página diferente.</returns>
        public bool SelectNextTab(bool forward, bool wrap = true) {
            if(!Enabled || TabCount == 0) {
                return false;
            }

            int currentIndex = SelectedIndex;
            int startingIndex = currentIndex >= 0 ? currentIndex : (forward ? -1 : 0);

            for(int step = 1; step <= TabCount; step++) {
                int candidateIndex = forward ? startingIndex + step : startingIndex - step;

                if(wrap) {
                    candidateIndex = ((candidateIndex % TabCount) + TabCount) % TabCount;
                } else if(candidateIndex < 0 || candidateIndex >= TabCount) {
                    return false;
                }

                if(candidateIndex != currentIndex && TabPages[candidateIndex].Enabled) {
                    SelectedIndex = candidateIndex;
                    Focus();
                    return true;
                }
            }

            return false;
        }

        /// <summary>Pausa todas las transiciones activas conservando su progreso.</summary>
        /// <returns><see langword="true"/> si al menos una animación cambió al estado pausado.</returns>
        public bool PauseAnimation() {
            bool selectionPaused = _selectionAnimator.Pause();
            bool appearancePaused = _appearanceAnimator.Pause();
            return selectionPaused || appearancePaused;
        }

        /// <summary>Reanuda todas las transiciones pausadas.</summary>
        /// <returns><see langword="true"/> si al menos una animación volvió a ejecutarse.</returns>
        public bool ResumeAnimation() {
            bool selectionResumed = _selectionAnimator.Resume();
            bool appearanceResumed = _appearanceAnimator.Resume();
            return selectionResumed || appearanceResumed;
        }

        /// <summary>Detiene las transiciones y muestra inmediatamente el estado lógico actual.</summary>
        /// <returns><see langword="true"/> si se detuvo al menos una animación.</returns>
        public bool StopAnimation() {
            bool selectionStopped = StopAnimatorIfActive(_selectionAnimator);
            bool appearanceStopped = StopAnimatorIfActive(_appearanceAnimator);
            ApplySelectionImmediately(stopAnimator: false);
            ApplyAppearanceImmediately(stopAnimator: false);
            return selectionStopped || appearanceStopped;
        }

        /// <inheritdoc/>
        protected override void OnControlAdded(ControlEventArgs e) {
            base.OnControlAdded(e);

            if(e.Control is TabPage page) {
                ConfigureTabPage(page);
            }

            UpdateTabSize();
            UpdateSelectionIndicator(animate: false);
            UpdateAppearance(animate: false);
        }

        /// <inheritdoc/>
        protected override void OnControlRemoved(ControlEventArgs e) {
            if(e.Control is TabPage page) {
                DetachTabPage(page);
            }

            base.OnControlRemoved(e);
            ResetPointerState();
            UpdateTabSize();
            UpdateSelectionIndicator(animate: false);
            UpdateAppearance(animate: false);
        }

        /// <inheritdoc/>
        protected override void OnSelecting(TabControlCancelEventArgs e) {
            base.OnSelecting(e);

            if(e.TabPage != null && !e.TabPage.Enabled) {
                e.Cancel = true;
            }
        }

        /// <inheritdoc/>
        protected override void OnSelectedIndexChanged(EventArgs e) {
            UpdateSelectionIndicator(animate: true);
            UpdateAppearance(animate: true);
            base.OnSelectedIndexChanged(e);
        }

        /// <inheritdoc/>
        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            UpdateTabSize();
            UpdateSelectionIndicator(animate: false);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnFontChanged(EventArgs e) {
            base.OnFontChanged(e);
            UpdateTabSize();
            UpdateSelectionIndicator(animate: false);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnRightToLeftChanged(EventArgs e) {
            base.OnRightToLeftChanged(e);
            UpdateSelectionIndicator(animate: false);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            ResetPointerState();
            UpdateAppearance(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnGotFocus(EventArgs e) {
            base.OnGotFocus(e);
            UpdateAppearance(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnLostFocus(EventArgs e) {
            base.OnLostFocus(e);
            ResetPointerState();
            UpdateAppearance(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            int newHoverIndex = HitTestTab(e.Location);
            int newCloseHoverIndex = -1;

            if(newHoverIndex >= 0 && !TabPages[newHoverIndex].Enabled) {
                newHoverIndex = -1;
            }

            if(_showCloseButtons && newHoverIndex >= 0 &&
                GetCloseButtonBounds(newHoverIndex).Contains(e.Location)) {
                newCloseHoverIndex = newHoverIndex;
            }

            if(_hoverIndex == newHoverIndex && _hoverCloseIndex == newCloseHoverIndex) {
                return;
            }

            _hoverIndex = newHoverIndex;
            _hoverCloseIndex = newCloseHoverIndex;
            UpdateAppearance(animate: true);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            _hoverIndex = -1;
            _hoverCloseIndex = -1;
            UpdateAppearance(animate: true);
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(MouseEventArgs e) {
            if(e.Button != MouseButtons.Left || !Enabled) {
                base.OnMouseDown(e);
                return;
            }

            int tabIndex = HitTestTab(e.Location);

            if(tabIndex < 0 || !TabPages[tabIndex].Enabled) {
                return;
            }

            Focus();
            _hoverIndex = tabIndex;

            if(_showCloseButtons && GetCloseButtonBounds(tabIndex).Contains(e.Location)) {
                _pressedCloseIndex = tabIndex;
                _pressedTabIndex = -1;
                Capture = true;
                UpdateAppearance(animate: true);
                Invalidate();
                return;
            }

            _pressedTabIndex = tabIndex;
            _pressedCloseIndex = -1;
            UpdateAppearance(animate: true);
            base.OnMouseDown(e);
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(MouseEventArgs e) {
            if(e.Button == MouseButtons.Left && _pressedCloseIndex >= 0) {
                int closingIndex = _pressedCloseIndex;
                bool shouldClose = closingIndex < TabCount &&
                    GetCloseButtonBounds(closingIndex).Contains(e.Location);
                _pressedCloseIndex = -1;
                _pressedTabIndex = -1;
                Capture = false;
                UpdateAppearance(animate: true);
                Invalidate();

                if(shouldClose) {
                    CloseTab(closingIndex);
                }

                return;
            }

            base.OnMouseUp(e);

            if(e.Button == MouseButtons.Left) {
                _pressedTabIndex = -1;
                UpdateAppearance(animate: true);
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseCaptureChanged(EventArgs e) {
            base.OnMouseCaptureChanged(e);

            if(Control.MouseButtons == MouseButtons.None &&
                (_pressedTabIndex >= 0 || _pressedCloseIndex >= 0)) {
                _pressedTabIndex = -1;
                _pressedCloseIndex = -1;
                UpdateAppearance(animate: true);
                Invalidate();
            }
        }

        /// <inheritdoc/>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if(Enabled && keyData == (Keys.Control | Keys.Tab)) {
                return SelectNextTab(forward: true) || base.ProcessCmdKey(ref msg, keyData);
            }

            if(Enabled && keyData == (Keys.Control | Keys.Shift | Keys.Tab)) {
                return SelectNextTab(forward: false) || base.ProcessCmdKey(ref msg, keyData);
            }

            if(Enabled && _showCloseButtons && keyData == (Keys.Control | Keys.W) &&
                SelectedIndex >= 0) {
                CloseTab(SelectedIndex);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <inheritdoc/>
        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            UpdateTabSize();
            ApplySelectionImmediately();
            ApplyAppearanceImmediately();
        }

        /// <inheritdoc/>
        protected override void OnHandleDestroyed(EventArgs e) {
            ResetPointerState();

            if(!_disposingResources) {
                StopAnimatorIfActive(_selectionAnimator);
                StopAnimatorIfActive(_appearanceAnimator);
            }

            base.OnHandleDestroyed(e);
        }

        /// <inheritdoc/>
        protected override void OnVisibleChanged(EventArgs e) {
            base.OnVisibleChanged(e);

            if(_initialized && !_disposingResources && !IsDisposed) {
                ApplySelectionImmediately();
                ApplyAppearanceImmediately();
            }
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

            using(SolidBrush backgroundBrush = new SolidBrush(ResolveBackgroundColor())) {
                graphics.FillRectangle(backgroundBrush, ClientRectangle);
            }

            Rectangle contentBounds = DisplayRectangle;
            if(contentBounds.Width > 0 && contentBounds.Height > 0) {
                using SolidBrush contentBrush = new SolidBrush(_contentBackColor);
                graphics.FillRectangle(contentBrush, contentBounds);
            }

            for(int index = 0; index < TabCount; index++) {
                DrawTab(graphics, index);
            }

            if(!_selectionAnimator.IsRunning && !_selectionAnimator.IsPaused) {
                _displayedIndicatorBounds = CreateIndicatorBounds(SelectedIndex);
                _targetIndicatorBounds = _displayedIndicatorBounds;
                _displayedSelectedTabIndex = SelectedIndex;
            }

            DrawSelectionIndicator(graphics);
            DrawFocusBorder(graphics);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            if(disposing && !_disposingResources) {
                _disposingResources = true;

                foreach(TabPage page in TabPages) {
                    DetachTabPage(page);
                }

                _selectionAnimator.Completed -= SelectionAnimator_Completed;
                _selectionAnimator.Canceled -= Animator_Canceled;
                _selectionAnimator.StateChanged -= Animator_StateChanged;
                _appearanceAnimator.Completed -= AppearanceAnimator_Completed;
                _appearanceAnimator.Canceled -= Animator_Canceled;
                _appearanceAnimator.StateChanged -= Animator_StateChanged;
                _selectionAnimator.Dispose();
                _appearanceAnimator.Dispose();

                VisualStateChanged = null;
                AnimationCompleted = null;
                AnimationCanceled = null;
                AnimationStateChanged = null;
                TabClosing = null;
                TabClosed = null;
            }

            base.Dispose(disposing);
        }

        private void ConfigureTabPage(TabPage page) {
            DetachTabPage(page);
            page.UseVisualStyleBackColor = false;
            page.BackColor = _contentBackColor;
            page.EnabledChanged += TabPage_EnabledChanged;
            page.TextChanged += TabPage_TextChanged;
        }

        private void DetachTabPage(TabPage page) {
            page.EnabledChanged -= TabPage_EnabledChanged;
            page.TextChanged -= TabPage_TextChanged;
        }

        private void UpdateTabPagesColor() {
            foreach(TabPage page in TabPages) {
                page.UseVisualStyleBackColor = false;
                page.BackColor = _contentBackColor;
            }
        }

        private void UpdateTabSize() {
            if(!_stretchTabs || _updatingTabSize || TabCount <= 0 ||
                ClientSize.Width <= 0 || ClientSize.Height <= 0) {
                return;
            }

            _updatingTabSize = true;

            try {
                Size newSize;

                if(Alignment == TabAlignment.Left || Alignment == TabAlignment.Right) {
                    int availableHeight = Math.Max(1, ClientSize.Height - 2);
                    int tabHeight = Math.Max(1, (availableHeight / TabCount) - 1);
                    newSize = new Size(_tabHeight, tabHeight);
                } else {
                    int availableWidth = Math.Max(1, ClientSize.Width - 2);
                    int tabWidth = Math.Max(1, (availableWidth / TabCount) - 1);
                    newSize = new Size(tabWidth, _tabHeight);
                }

                if(ItemSize != newSize) {
                    ItemSize = newSize;
                }
            } finally {
                _updatingTabSize = false;
            }
        }

        private SaraTabVisualState DetermineVisualState() {
            if(!Enabled) {
                return SaraTabVisualState.Disabled;
            }

            bool closePressed = _pressedCloseIndex >= 0 && _pressedCloseIndex == _hoverCloseIndex;
            bool tabPressed = _pressedTabIndex >= 0 && _pressedTabIndex == _hoverIndex;

            if(closePressed || tabPressed) {
                return SaraTabVisualState.Pressed;
            }

            if(_hoverIndex >= 0) {
                return SaraTabVisualState.Hovered;
            }

            if(Focused) {
                return SaraTabVisualState.Focused;
            }

            return SaraTabVisualState.Normal;
        }

        private int DetermineInteractionIndex(SaraTabVisualState state) {
            if(state == SaraTabVisualState.Pressed) {
                return _pressedCloseIndex >= 0 ? _pressedCloseIndex : _pressedTabIndex;
            }

            return state == SaraTabVisualState.Hovered ? _hoverIndex : -1;
        }

        private void UpdateAppearance(bool animate) {
            if(!_initialized || _disposingResources || IsDisposed) {
                return;
            }

            SaraTabVisualState newState = DetermineVisualState();
            int newInteractionIndex = DetermineInteractionIndex(newState);
            int newSelectedIndex = SelectedIndex;
            bool stateChanged = _visualState != newState;
            bool targetChanged = stateChanged ||
                _appearanceInteractionIndex != newInteractionIndex ||
                _appearanceSelectedIndex != newSelectedIndex;

            if(!targetChanged) {
                Invalidate();
                return;
            }

            CaptureDisplayedAppearance();
            _visualState = newState;
            _appearanceVisualState = newState;
            _appearanceInteractionIndex = newInteractionIndex;
            _appearanceSelectedIndex = newSelectedIndex;

            if(stateChanged) {
                VisualStateChanged?.Invoke(this, EventArgs.Empty);
            }

            if(!animate || !CanAnimate() || _animationDuration == 0 ||
                !HasAppearanceDifference()) {
                ApplyAppearanceImmediately();
                return;
            }

            _appearanceProgress = 0f;
            _appearanceAnimator.Start(
                0f,
                1f,
                progress => {
                    _appearanceProgress = progress;
                    Invalidate();
                },
                CreateAnimationOptions());
        }

        private void CaptureDisplayedAppearance() {
            int count = TabCount;
            Color[] currentTabColors = new Color[count];
            Color[] currentTextColors = new Color[count];

            for(int index = 0; index < count; index++) {
                currentTabColors[index] = GetDisplayedTabColor(index);
                currentTextColors[index] = GetDisplayedTextColor(index);
            }

            _originTabColors = currentTabColors;
            _originTextColors = currentTextColors;
        }

        private bool HasAppearanceDifference() {
            for(int index = 0; index < TabCount; index++) {
                if(index >= _originTabColors.Length || index >= _originTextColors.Length ||
                    _originTabColors[index] != ResolveTargetTabColor(index) ||
                    _originTextColors[index] != ResolveTargetTextColor(index)) {
                    return true;
                }
            }

            return false;
        }

        private Color GetDisplayedTabColor(int index) {
            Color destination = ResolveTargetTabColor(index);

            if(_appearanceProgress >= 1f || index < 0 || index >= _originTabColors.Length) {
                return destination;
            }

            return Blend(_originTabColors[index], destination, _appearanceProgress);
        }

        private Color GetDisplayedTextColor(int index) {
            Color destination = ResolveTargetTextColor(index);

            if(_appearanceProgress >= 1f || index < 0 || index >= _originTextColors.Length) {
                return destination;
            }

            return Blend(_originTextColors[index], destination, _appearanceProgress);
        }

        private Color ResolveTargetTabColor(int index) {
            bool selected = index == _appearanceSelectedIndex;
            Color baseColor = selected ? _selectedTabColor : _unselectedTabColor;

            if(!Enabled || index < 0 || index >= TabCount || !TabPages[index].Enabled) {
                return _disabledTabColor.IsEmpty
                    ? Blend(baseColor, ResolveBackgroundColor(), 0.62f)
                    : _disabledTabColor;
            }

            if(index != _appearanceInteractionIndex) {
                return baseColor;
            }

            if(_appearanceVisualState == SaraTabVisualState.Pressed) {
                Color pressedColor = _pressedTabColor.IsEmpty
                    ? Blend(baseColor, Color.Black, 0.14f)
                    : _pressedTabColor;
                return selected ? Blend(baseColor, pressedColor, 0.55f) : pressedColor;
            }

            if(_appearanceVisualState == SaraTabVisualState.Hovered) {
                Color hoverColor = _hoverTabColor.IsEmpty
                    ? Blend(baseColor, Color.White, 0.14f)
                    : _hoverTabColor;
                return selected ? Blend(baseColor, hoverColor, 0.35f) : hoverColor;
            }

            return baseColor;
        }

        private Color ResolveTargetTextColor(int index) {
            bool selected = index == _appearanceSelectedIndex;

            if(!Enabled || index < 0 || index >= TabCount || !TabPages[index].Enabled) {
                return _disabledTextColor.IsEmpty ? SystemColors.GrayText : _disabledTextColor;
            }

            return selected ? _selectedTextColor : _unselectedTextColor;
        }

        private void ApplyAppearanceImmediately(bool stopAnimator = true) {
            if(stopAnimator) {
                StopAnimatorIfActive(_appearanceAnimator);
            }

            _visualState = DetermineVisualState();
            _appearanceVisualState = _visualState;
            _appearanceInteractionIndex = DetermineInteractionIndex(_visualState);
            _appearanceSelectedIndex = SelectedIndex;
            _appearanceProgress = 1f;
            _originTabColors = Array.Empty<Color>();
            _originTextColors = Array.Empty<Color>();
            Invalidate();
        }

        private void UpdateSelectionIndicator(bool animate) {
            if(!_initialized || _disposingResources || IsDisposed) {
                return;
            }

            RectangleF destination = CreateIndicatorBounds(SelectedIndex);
            float destinationIndex = SelectedIndex;

            if(!animate || !CanAnimate() || _animationDuration == 0 ||
                _displayedIndicatorBounds.IsEmpty || destination.IsEmpty ||
                (_displayedIndicatorBounds == destination &&
                 Math.Abs(_displayedSelectedTabIndex - destinationIndex) < 0.001f)) {
                ApplySelectionImmediately();
                return;
            }

            RectangleF origin = _displayedIndicatorBounds;
            float originIndex = _displayedSelectedTabIndex;
            _targetIndicatorBounds = destination;

            _selectionAnimator.Start(
                0f,
                1f,
                progress => {
                    _displayedIndicatorBounds = InterpolateRectangle(origin, destination, progress);
                    _displayedSelectedTabIndex =
                        originIndex + ((destinationIndex - originIndex) * progress);
                    Invalidate();
                },
                CreateAnimationOptions());
        }

        private void ApplySelectionImmediately(bool stopAnimator = true) {
            if(stopAnimator) {
                StopAnimatorIfActive(_selectionAnimator);
            }

            _targetIndicatorBounds = CreateIndicatorBounds(SelectedIndex);
            _displayedIndicatorBounds = _targetIndicatorBounds;
            _displayedSelectedTabIndex = SelectedIndex;
            Invalidate();
        }

        private void RestartActiveAnimations() {
            bool selectionActive = _selectionAnimator.IsRunning || _selectionAnimator.IsPaused;
            bool appearanceActive = _appearanceAnimator.IsRunning || _appearanceAnimator.IsPaused;

            if(selectionActive) {
                UpdateSelectionIndicator(animate: true);
            }

            if(appearanceActive) {
                RestartAppearanceAnimation();
            }
        }

        private void RestartAppearanceAnimation() {
            CaptureDisplayedAppearance();

            if(!CanAnimate() || _animationDuration == 0 || !HasAppearanceDifference()) {
                ApplyAppearanceImmediately();
                return;
            }

            _appearanceProgress = 0f;
            _appearanceAnimator.Start(
                0f,
                1f,
                progress => {
                    _appearanceProgress = progress;
                    Invalidate();
                },
                CreateAnimationOptions());
        }

        private void DrawTab(Graphics graphics, int index) {
            Rectangle tabBounds = GetSafeTabBounds(index);

            if(tabBounds.Width <= 0 || tabBounds.Height <= 0 ||
                !ClientRectangle.IntersectsWith(tabBounds)) {
                return;
            }

            using GraphicsPath tabPath = CreateRoundedPath(tabBounds, _tabRadius);
            using SolidBrush tabBrush = new SolidBrush(GetDisplayedTabColor(index));
            graphics.FillPath(tabBrush, tabPath);

            DrawTabContent(graphics, index, tabBounds, GetDisplayedTextColor(index));
        }

        private void DrawTabContent(Graphics graphics, int index, Rectangle tabBounds, Color textColor) {
            TabPage page = TabPages[index];
            bool rightToLeft = RightToLeft == RightToLeft.Yes;
            Rectangle contentBounds = Rectangle.Inflate(tabBounds, -10, -4);
            Rectangle closeBounds = GetCloseButtonBounds(index);

            if(!closeBounds.IsEmpty) {
                if(rightToLeft) {
                    contentBounds.X = closeBounds.Right + 6;
                    contentBounds.Width = Math.Max(0, tabBounds.Right - 10 - contentBounds.X);
                } else {
                    contentBounds.Width = Math.Max(0, closeBounds.Left - 6 - contentBounds.Left);
                }
            }

            Image? image = ResolveTabImage(page);

            if(image != null && contentBounds.Width > 18) {
                int side = Math.Min(16, Math.Max(8, contentBounds.Height));
                int imageX = rightToLeft
                    ? contentBounds.Right - side
                    : contentBounds.Left;
                Rectangle imageBounds = new Rectangle(
                    imageX,
                    contentBounds.Top + ((contentBounds.Height - side) / 2),
                    side,
                    side);
                graphics.DrawImage(image, imageBounds);

                if(rightToLeft) {
                    contentBounds.Width = Math.Max(0, imageBounds.Left - 6 - contentBounds.Left);
                } else {
                    int newLeft = imageBounds.Right + 6;
                    contentBounds.Width = Math.Max(0, contentBounds.Right - newLeft);
                    contentBounds.X = newLeft;
                }
            }

            if(contentBounds.Width > 0 && contentBounds.Height > 0) {
                TextFormatFlags flags = TextFormatFlags.EndEllipsis |
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.PreserveGraphicsClipping |
                    TextFormatFlags.SingleLine |
                    TextFormatFlags.VerticalCenter;

                if(rightToLeft) {
                    flags |= TextFormatFlags.RightToLeft;
                }

                if(!ShowKeyboardCues) {
                    flags |= TextFormatFlags.HidePrefix;
                }

                TextRenderer.DrawText(
                    graphics,
                    page.Text,
                    Font,
                    contentBounds,
                    textColor,
                    flags);
            }

            if(!closeBounds.IsEmpty) {
                DrawCloseButton(graphics, index, closeBounds, textColor);
            }
        }

        private void DrawCloseButton(Graphics graphics, int index, Rectangle bounds, Color textColor) {
            Color closeColor = _closeButtonColor.IsEmpty ? textColor : _closeButtonColor;

            if(index == _hoverCloseIndex) {
                closeColor = _closeButtonHoverColor;
            }

            if(index == _pressedCloseIndex && index == _hoverCloseIndex) {
                closeColor = Blend(closeColor, Color.Black, 0.24f);
            }

            if(!Enabled || !TabPages[index].Enabled) {
                closeColor = _disabledTextColor.IsEmpty ? SystemColors.GrayText : _disabledTextColor;
            }

            float penWidth = Math.Max(1.2f, bounds.Width / 8f);
            int inset = Math.Max(1, bounds.Width / 5);
            using Pen closePen = new Pen(closeColor, penWidth) {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            graphics.DrawLine(
                closePen,
                bounds.Left + inset,
                bounds.Top + inset,
                bounds.Right - inset,
                bounds.Bottom - inset);
            graphics.DrawLine(
                closePen,
                bounds.Right - inset,
                bounds.Top + inset,
                bounds.Left + inset,
                bounds.Bottom - inset);
        }

        private void DrawSelectionIndicator(Graphics graphics) {
            if(_displayedIndicatorBounds.IsEmpty || _indicatorColor.A == 0) {
                return;
            }

            Rectangle indicatorBounds = Rectangle.Round(_displayedIndicatorBounds);

            if(indicatorBounds.Width <= 0 || indicatorBounds.Height <= 0) {
                return;
            }

            using GraphicsPath indicatorPath = CreateRoundedPath(
                indicatorBounds,
                Math.Min(indicatorBounds.Width, indicatorBounds.Height) / 2);
            using SolidBrush indicatorBrush = new SolidBrush(_indicatorColor);
            graphics.FillPath(indicatorBrush, indicatorPath);
        }

        private void DrawFocusBorder(Graphics graphics) {
            if(!_showFocusBorder || !Enabled || !Focused || !ShowFocusCues ||
                SelectedIndex < 0 || SelectedIndex >= TabCount) {
                return;
            }

            Rectangle tabBounds = GetSafeTabBounds(SelectedIndex);

            if(tabBounds.Width <= 0 || tabBounds.Height <= 0) {
                return;
            }

            using GraphicsPath focusPath = CreateRoundedPath(
                Rectangle.Inflate(tabBounds, -2, -2),
                Math.Max(0, _tabRadius - 2));
            Color focusColor = _focusBorderColor.IsEmpty
                ? SystemColors.Highlight
                : _focusBorderColor;
            using Pen focusPen = new Pen(focusColor, 1f) {
                Alignment = PenAlignment.Inset,
                DashStyle = DashStyle.Dot
            };
            graphics.DrawPath(focusPen, focusPath);
        }

        private RectangleF CreateIndicatorBounds(int index) {
            Rectangle tabBounds = GetSafeTabBounds(index);

            if(tabBounds.Width <= 0 || tabBounds.Height <= 0) {
                return RectangleF.Empty;
            }

            int thickness = Math.Min(
                _indicatorThickness,
                Math.Max(1, Math.Min(tabBounds.Width, tabBounds.Height)));

            if(Alignment == TabAlignment.Left || Alignment == TabAlignment.Right) {
                int inset = Math.Min(_indicatorInset, Math.Max(0, (tabBounds.Height - 1) / 2));
                int height = Math.Max(1, tabBounds.Height - (inset * 2));
                int x = Alignment == TabAlignment.Left
                    ? tabBounds.Right - thickness
                    : tabBounds.Left;
                return new RectangleF(x, tabBounds.Top + inset, thickness, height);
            }

            int horizontalInset = Math.Min(
                _indicatorInset,
                Math.Max(0, (tabBounds.Width - 1) / 2));
            int width = Math.Max(1, tabBounds.Width - (horizontalInset * 2));
            int y = Alignment == TabAlignment.Bottom
                ? tabBounds.Top
                : tabBounds.Bottom - thickness;
            return new RectangleF(tabBounds.Left + horizontalInset, y, width, thickness);
        }

        private Rectangle GetCloseButtonBounds(int index) {
            if(!_showCloseButtons) {
                return Rectangle.Empty;
            }

            Rectangle tabBounds = GetSafeTabBounds(index);

            if(tabBounds.Width < _closeButtonSize + 18 || tabBounds.Height < _closeButtonSize + 4) {
                return Rectangle.Empty;
            }

            bool rightToLeft = RightToLeft == RightToLeft.Yes;
            int x = rightToLeft
                ? tabBounds.Left + 8
                : tabBounds.Right - 8 - _closeButtonSize;
            int y = tabBounds.Top + ((tabBounds.Height - _closeButtonSize) / 2);
            return new Rectangle(x, y, _closeButtonSize, _closeButtonSize);
        }

        private Rectangle GetSafeTabBounds(int index) {
            if(index < 0 || index >= TabCount || !IsHandleCreated) {
                return Rectangle.Empty;
            }

            Rectangle bounds = GetTabRect(index);

            if(bounds.Width > 0) {
                bounds.Width--;
            }

            if(bounds.Height > 0) {
                bounds.Height--;
            }

            return bounds;
        }

        private int HitTestTab(Point location) {
            for(int index = 0; index < TabCount; index++) {
                if(GetSafeTabBounds(index).Contains(location)) {
                    return index;
                }
            }

            return -1;
        }

        private Image? ResolveTabImage(TabPage page) {
            if(ImageList == null) {
                return null;
            }

            if(!string.IsNullOrEmpty(page.ImageKey) && ImageList.Images.ContainsKey(page.ImageKey)) {
                return ImageList.Images[page.ImageKey];
            }

            int imageIndex = page.ImageIndex;
            return imageIndex >= 0 && imageIndex < ImageList.Images.Count
                ? ImageList.Images[imageIndex]
                : null;
        }

        private void ResetPointerState() {
            _hoverIndex = -1;
            _pressedTabIndex = -1;
            _hoverCloseIndex = -1;
            _pressedCloseIndex = -1;

            if(Capture) {
                Capture = false;
            }
        }

        private void SetAppearanceColor(ref Color field, Color value) {
            if(field == value) {
                return;
            }

            field = value;
            ApplyAppearanceImmediately();
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

        private Color ResolveBackgroundColor() {
            return Parent?.BackColor ?? SystemColors.Control;
        }

        private SaraAnimationOptions CreateAnimationOptions() {
            return new SaraAnimationOptions {
                Duration = _animationDuration,
                FrameInterval = _animationFrameInterval,
                Easing = _animationEasing
            };
        }

        private void SelectionAnimator_Completed(object? sender, EventArgs e) {
            _displayedIndicatorBounds = _targetIndicatorBounds;
            _displayedSelectedTabIndex = SelectedIndex;
            Invalidate();
            AnimationCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void AppearanceAnimator_Completed(object? sender, EventArgs e) {
            _appearanceProgress = 1f;
            _originTabColors = Array.Empty<Color>();
            _originTextColors = Array.Empty<Color>();
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
            if(_selectionAnimator.State == SaraAnimationState.Running ||
                _appearanceAnimator.State == SaraAnimationState.Running) {
                return SaraAnimationState.Running;
            }

            if(_selectionAnimator.State == SaraAnimationState.Paused ||
                _appearanceAnimator.State == SaraAnimationState.Paused) {
                return SaraAnimationState.Paused;
            }

            if(changedAnimator != null) {
                return changedAnimator.State;
            }

            if(_selectionAnimator.State == SaraAnimationState.Completed ||
                _appearanceAnimator.State == SaraAnimationState.Completed) {
                return SaraAnimationState.Completed;
            }

            return SaraAnimationState.Stopped;
        }

        private void TabPage_EnabledChanged(object? sender, EventArgs e) {
            RestartAppearanceAnimation();
            Invalidate();
        }

        private void TabPage_TextChanged(object? sender, EventArgs e) {
            Invalidate();
        }

        private static bool StopAnimatorIfActive(SaraAnimator animator) {
            return (animator.IsRunning || animator.IsPaused) && animator.Stop();
        }

        private static RectangleF InterpolateRectangle(
            RectangleF origin,
            RectangleF destination,
            float progress) {
            float amount = Math.Max(0f, Math.Min(1f, progress));
            return new RectangleF(
                origin.X + ((destination.X - origin.X) * amount),
                origin.Y + ((destination.Y - origin.Y) * amount),
                origin.Width + ((destination.Width - origin.Width) * amount),
                origin.Height + ((destination.Height - origin.Height) * amount));
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
    }
}

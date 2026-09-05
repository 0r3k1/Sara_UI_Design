using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sara_UI_Design.Animations;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Combina una lista desplegable nativa con una superficie personalizable, accesible
    /// y animada, conservando las capacidades de datos de <see cref="ComboBox"/>.
    /// </summary>
    [DefaultEvent(nameof(SelectedIndexChanged))]
    [DefaultProperty(nameof(Items))]
    [ToolboxItem(true)]
    public class SaraUI_ComboBox:UserControl {
        /// <summary>Describe el estado de interacción que gobierna la apariencia del control.</summary>
        public enum SaraComboBoxVisualState {
            /// <summary>El control no tiene una interacción activa.</summary>
            Normal,

            /// <summary>El puntero se encuentra sobre la superficie del control.</summary>
            Hovered,

            /// <summary>La superficie se está presionando con el ratón.</summary>
            Pressed,

            /// <summary>El control o su lista interna tienen el foco.</summary>
            Focused,

            /// <summary>La lista desplegable se encuentra abierta.</summary>
            DroppedDown,

            /// <summary>El control está deshabilitado.</summary>
            Disabled
        }

        private readonly ComboBox _comboBox;
        private readonly Label _textSurface;
        private readonly Panel _iconSurface;
        private readonly SaraAnimator _animator;
        private Color _iconColor = Color.MediumSlateBlue;
        private Color _listBackColor = Color.FromArgb(230, 228, 245);
        private Color _listTextColor = Color.DimGray;
        private Color _borderColor = Color.MediumSlateBlue;
        private Color _borderFocusColor = Color.HotPink;
        private Color _hoverBackColor = Color.Empty;
        private Color _hoverBorderColor = Color.Empty;
        private Color _pressedBackColor = Color.Empty;
        private Color _disabledBackColor = Color.Empty;
        private Color _disabledForeColor = Color.Empty;
        private Color _disabledBorderColor = Color.Empty;
        private int _borderSize = 2;
        private int _iconSize = 12;
        private int _iconAreaWidth = 30;
        private ContentAlignment _textAlign = ContentAlignment.MiddleLeft;
        private string _placeholderText = string.Empty;
        private bool _animationEnabled = true;
        private int _animationDuration = 180;
        private int _animationFrameInterval = 15;
        private SaraEasing _animationEasing = SaraEasing.EaseOutCubic;
        private bool _isMouseOver;
        private bool _isPressed;
        private bool _isDroppedDown;
        private bool _initialized;
        private bool _disposingResources;
        private SaraComboBoxVisualState _visualState;
        private ComboBoxAppearance _displayAppearance;
        private ComboBoxAppearance _targetAppearance;

        /// <summary>Inicializa una lista desplegable compuesta, accesible y animada.</summary>
        public SaraUI_ComboBox() {
            _animator = new SaraAnimator();
            _comboBox = new ComboBox();
            _textSurface = new Label();
            _iconSurface = new Panel();

            _animator.Completed += Animator_Completed;
            _animator.Canceled += Animator_Canceled;
            _animator.StateChanged += Animator_StateChanged;

            _comboBox.BackColor = _listBackColor;
            _comboBox.ForeColor = _listTextColor;
            _comboBox.TabStop = false;
            _comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            _comboBox.SelectionChangeCommitted += ComboBox_SelectionChangeCommitted;
            _comboBox.TextChanged += ComboBox_TextChanged;
            _comboBox.Enter += ComboBox_Enter;
            _comboBox.Leave += ComboBox_Leave;
            _comboBox.DropDown += ComboBox_DropDown;
            _comboBox.DropDownClosed += ComboBox_DropDownClosed;

            _textSurface.AutoEllipsis = true;
            _textSurface.AutoSize = false;
            _textSurface.Cursor = Cursors.Hand;
            _textSurface.Padding = new Padding(8, 0, 8, 0);
            _textSurface.TabStop = false;
            _textSurface.TextAlign = _textAlign;
            _textSurface.UseMnemonic = false;
            SubscribeSurfaceEvents(_textSurface);

            _iconSurface.Cursor = Cursors.Hand;
            _iconSurface.TabStop = false;
            _iconSurface.Paint += IconSurface_Paint;
            SubscribeSurfaceEvents(_iconSurface);

            SuspendLayout();
            Controls.Add(_comboBox);
            Controls.Add(_textSurface);
            Controls.Add(_iconSurface);

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable |
                ControlStyles.UserPaint,
                true);

            AccessibleRole = AccessibleRole.ComboBox;
            Cursor = Cursors.Hand;
            MinimumSize = new Size(200, 30);
            Padding = new Padding(_borderSize);
            Size = new Size(200, 30);
            TabStop = true;

            _comboBox.Font = Font;
            _textSurface.Font = Font;
            _comboBox.RightToLeft = RightToLeft;

            _initialized = true;
            _visualState = DetermineVisualState();
            _targetAppearance = ResolveAppearance(_visualState);
            _displayAppearance = _targetAppearance;
            UpdateChildBounds();
            UpdateDisplayedText();
            ApplyAppearance(_displayAppearance);
            ResumeLayout(false);
        }

        /// <summary>
        /// Evento histórico que se produce cuando cambia el índice seleccionado.
        /// El código nuevo puede utilizar <see cref="SelectedIndexChanged"/>.
        /// </summary>
        [Category("Sara UI Data")]
        public event EventHandler? OnSelectedIndexChanged;

        /// <summary>Se produce cuando cambia el índice seleccionado.</summary>
        [Category("Sara UI Data")]
        public event EventHandler? SelectedIndexChanged;

        /// <summary>Se produce cuando el usuario confirma una selección en la lista.</summary>
        [Category("Sara UI Data")]
        public event EventHandler? SelectionChangeCommitted;

        /// <summary>Se produce cuando la lista desplegable termina de abrirse.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? DropDownOpened;

        /// <summary>Se produce cuando la lista desplegable se cierra.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? DropDownClosed;

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

        /// <summary>Obtiene o establece el color de fondo normal de la superficie.</summary>
        [Category("Sara UI Design")]
        public override Color BackColor {
            get => base.BackColor;
            set => base.BackColor = value;
        }

        /// <summary>Obtiene o establece el color normal del texto mostrado.</summary>
        [Category("Sara UI Design")]
        public override Color ForeColor {
            get => base.ForeColor;
            set => base.ForeColor = value;
        }

        /// <summary>Obtiene o establece la fuente utilizada por la superficie y la lista interna.</summary>
        [Category("Sara UI Design")]
#if NET8_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
        public override Font Font {
            get => base.Font;
            set => base.Font = value;
        }

        /// <summary>Obtiene o establece el color de la flecha indicadora.</summary>
        [Category("Sara UI Design")]
        public Color IconColor {
            get => _iconColor;
            set => SetAppearanceColor(ref _iconColor, value);
        }

        /// <summary>Obtiene o establece el color de fondo de la lista desplegable.</summary>
        [Category("Sara UI Design")]
        public Color ListBackColor {
            get => _listBackColor;
            set {
                if(_listBackColor == value) {
                    return;
                }

                _listBackColor = value;
                _comboBox.BackColor = value;
            }
        }

        /// <summary>Obtiene o establece el color del texto dentro de la lista desplegable.</summary>
        [Category("Sara UI Design")]
        public Color ListTextColor {
            get => _listTextColor;
            set {
                if(_listTextColor == value) {
                    return;
                }

                _listTextColor = value;
                _comboBox.ForeColor = value;
            }
        }

        /// <summary>Obtiene o establece el color normal del borde exterior.</summary>
        [Category("Sara UI Design")]
        public Color BorderColor {
            get => _borderColor;
            set => SetAppearanceColor(ref _borderColor, value);
        }

        /// <summary>Obtiene o establece el color del borde cuando el control tiene foco o está abierto.</summary>
        [Category("Sara UI Design")]
        public Color BorderFocusColor {
            get => _borderFocusColor;
            set => SetAppearanceColor(ref _borderFocusColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color de fondo bajo el puntero.
        /// <see cref="Color.Empty"/> calcula una variante a partir de los colores actuales.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color HoverBackColor {
            get => _hoverBackColor;
            set => SetAppearanceColor(ref _hoverBackColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del borde bajo el puntero.
        /// <see cref="Color.Empty"/> interpola hacia <see cref="BorderFocusColor"/>.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color HoverBorderColor {
            get => _hoverBorderColor;
            set => SetAppearanceColor(ref _hoverBorderColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color de fondo mientras se presiona la superficie.
        /// <see cref="Color.Empty"/> calcula una variante oscura del fondo normal.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color PressedBackColor {
            get => _pressedBackColor;
            set => SetAppearanceColor(ref _pressedBackColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color de fondo cuando el control está deshabilitado.
        /// <see cref="Color.Empty"/> calcula una variante atenuada.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledBackColor {
            get => _disabledBackColor;
            set => SetAppearanceColor(ref _disabledBackColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del texto cuando el control está deshabilitado.
        /// <see cref="Color.Empty"/> utiliza <see cref="SystemColors.GrayText"/>.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledForeColor {
            get => _disabledForeColor;
            set => SetAppearanceColor(ref _disabledForeColor, value);
        }

        /// <summary>
        /// Obtiene o establece el color del borde cuando el control está deshabilitado.
        /// <see cref="Color.Empty"/> calcula una variante atenuada.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledBorderColor {
            get => _disabledBorderColor;
            set => SetAppearanceColor(ref _disabledBorderColor, value);
        }

        /// <summary>Obtiene o establece si los cambios de interacción y apertura deben animarse.</summary>
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

        /// <summary>Obtiene o establece el grosor del borde exterior.</summary>
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
                UpdateChildBounds();
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el tamaño solicitado para la flecha indicadora.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor menor que uno.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(12)]
        public int IconSize {
            get => _iconSize;
            set {
                EnsurePositive(value, nameof(IconSize));

                if(_iconSize == value) {
                    return;
                }

                _iconSize = value;
                _iconSurface.Invalidate();
            }
        }

        /// <summary>Obtiene o establece el ancho reservado para la flecha indicadora.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor menor que uno.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(30)]
        public int IconAreaWidth {
            get => _iconAreaWidth;
            set {
                EnsurePositive(value, nameof(IconAreaWidth));

                if(_iconAreaWidth == value) {
                    return;
                }

                _iconAreaWidth = value;
                UpdateChildBounds();
            }
        }

        /// <summary>Obtiene o establece la alineación del texto mostrado en la superficie.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment TextAlign {
            get => _textAlign;
            set {
                if(!Enum.IsDefined(typeof(ContentAlignment), value)) {
                    throw new ArgumentOutOfRangeException(
                        nameof(TextAlign), value, "La alineación indicada no es compatible.");
                }

                if(_textAlign == value) {
                    return;
                }

                _textAlign = value;
                _textSurface.TextAlign = value;
            }
        }

        /// <summary>Obtiene o establece el texto mostrado cuando no existe una selección o entrada.</summary>
        [Category("Sara UI Design")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PlaceholderText {
            get => _placeholderText;
            set {
                string nextValue = value ?? string.Empty;
                if(_placeholderText == nextValue) {
                    return;
                }

                _placeholderText = nextValue;
                UpdateDisplayedText();
            }
        }

        /// <summary>
        /// Obtiene o establece el texto visible en la superficie. Se conserva para compatibilidad;
        /// <see cref="PlaceholderText"/> expresa mejor la intención del valor inicial.
        /// </summary>
        [Category("Sara UI Design")]
        [Localizable(true)]
        public string Texts {
            get => _textSurface.Text;
            set {
                _placeholderText = value ?? string.Empty;
                _textSurface.Text = _placeholderText;
            }
        }

        /// <summary>Obtiene o establece el estilo de edición y apertura de la lista.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor no definido.</exception>
        /// <exception cref="NotSupportedException">
        /// Se produce al utilizar <see cref="ComboBoxStyle.Simple"/>, que no puede ocultarse dentro de esta superficie.
        /// </exception>
        [Category("Sara UI Design")]
        [DefaultValue(ComboBoxStyle.DropDown)]
        public ComboBoxStyle DropDownStyle {
            get => _comboBox.DropDownStyle;
            set {
                if(!Enum.IsDefined(typeof(ComboBoxStyle), value)) {
                    throw new ArgumentOutOfRangeException(
                        nameof(DropDownStyle), value, "El estilo indicado no es compatible.");
                }

                if(value == ComboBoxStyle.Simple) {
                    throw new NotSupportedException(
                        "SaraUI_ComboBox admite los estilos DropDown y DropDownList.");
                }

                if(_comboBox.DropDownStyle == value) {
                    return;
                }

                _comboBox.DropDownStyle = value;
                UpdateChildBounds();
                UpdateDisplayedText();
            }
        }

        /// <summary>Obtiene o establece si la lista se encuentra desplegada.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DroppedDown {
            get => _comboBox.DroppedDown;
            set {
                if(value) {
                    OpenDropDown();
                } else {
                    CloseDropDown();
                }
            }
        }

        /// <summary>Obtiene la colección de elementos administrada por la lista interna.</summary>
        [Category("Sara UI Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [Localizable(true)]
        [MergableProperty(false)]
        public ComboBox.ObjectCollection Items => _comboBox.Items;

        /// <summary>Obtiene o establece el origen de datos de la lista.</summary>
        [Category("Sara UI Data")]
        [AttributeProvider(typeof(IListSource))]
        [DefaultValue(null)]
        public object? DataSource {
            get => _comboBox.DataSource;
            set {
                _comboBox.DataSource = value;
                UpdateDisplayedText();
            }
        }

        /// <summary>Obtiene o establece la colección utilizada para autocompletar entradas.</summary>
        [Category("Sara UI Data")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Localizable(true)]
        public AutoCompleteStringCollection AutoCompleteCustomSource {
            get => _comboBox.AutoCompleteCustomSource;
            set => _comboBox.AutoCompleteCustomSource = value;
        }

        /// <summary>Obtiene o establece el origen utilizado para autocompletar entradas.</summary>
        [Category("Sara UI Data")]
        [Browsable(true)]
        [DefaultValue(AutoCompleteSource.None)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public AutoCompleteSource AutoCompleteSource {
            get => _comboBox.AutoCompleteSource;
            set => _comboBox.AutoCompleteSource = value;
        }

        /// <summary>Obtiene o establece el comportamiento utilizado para autocompletar entradas.</summary>
        [Category("Sara UI Data")]
        [Browsable(true)]
        [DefaultValue(AutoCompleteMode.None)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public AutoCompleteMode AutoCompleteMode {
            get => _comboBox.AutoCompleteMode;
            set => _comboBox.AutoCompleteMode = value;
        }

        /// <summary>Obtiene o establece el elemento seleccionado actualmente.</summary>
        [Category("Sara UI Data")]
        [Bindable(true)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object? SelectedItem {
            get => _comboBox.SelectedItem;
            set => _comboBox.SelectedItem = value;
        }

        /// <summary>Obtiene o establece el índice seleccionado, o -1 cuando no existe selección.</summary>
        [Category("Sara UI Data")]
        [Browsable(false)]
        [DefaultValue(-1)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex {
            get => _comboBox.SelectedIndex;
            set => _comboBox.SelectedIndex = value;
        }

        /// <summary>Obtiene o establece la propiedad mostrada para cada elemento enlazado.</summary>
        [Category("Sara UI Data")]
        [DefaultValue("")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string DisplayMember {
            get => _comboBox.DisplayMember;
            set => _comboBox.DisplayMember = value;
        }

        /// <summary>Obtiene o establece la propiedad utilizada como valor para cada elemento enlazado.</summary>
        [Category("Sara UI Data")]
        [DefaultValue("")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ValueMember {
            get => _comboBox.ValueMember;
            set => _comboBox.ValueMember = value;
        }

        /// <summary>Obtiene o establece el valor del elemento seleccionado actualmente.</summary>
        [Category("Sara UI Data")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object? SelectedValue {
            get => _comboBox.SelectedValue;
            set => _comboBox.SelectedValue = value;
        }

        /// <summary>Obtiene el estado visual de interacción actual.</summary>
        [Browsable(false)]
        public SaraComboBoxVisualState VisualState => _visualState;

        /// <summary>Obtiene el estado actual del motor de animación interno.</summary>
        [Browsable(false)]
        public SaraAnimationState AnimationState => _animator.State;

        /// <summary>Obtiene el progreso interpolado de apertura utilizado para orientar la flecha.</summary>
        [Browsable(false)]
        public float DisplayedDropDownProgress => _displayAppearance.DropDownProgress;

        /// <summary>Abre la lista y transfiere el foco al ComboBox interno.</summary>
        /// <returns><see langword="true"/> si la lista está abierta después de la operación.</returns>
        public bool OpenDropDown() {
            if(!Enabled || _disposingResources || IsDisposed || IsInDesignMode()) {
                return false;
            }

            if(!IsHandleCreated) {
                CreateControl();
            }

            if(!_comboBox.IsHandleCreated) {
                _comboBox.CreateControl();
            }

            _comboBox.Select();

            if(!_comboBox.DroppedDown) {
                _comboBox.DroppedDown = true;
            }

            return _comboBox.DroppedDown;
        }

        /// <summary>Cierra la lista desplegable si se encuentra abierta.</summary>
        /// <returns><see langword="true"/> si una lista abierta cambió a cerrada.</returns>
        public bool CloseDropDown() {
            if(_disposingResources || IsDisposed || !_comboBox.DroppedDown) {
                return false;
            }

            _comboBox.DroppedDown = false;
            return true;
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
            ApplyAppearance(_targetAppearance);
            return stopped;
        }

        /// <inheritdoc/>
        protected override void OnClick(EventArgs e) {
            base.OnClick(e);
            OpenDropDown();
        }

        /// <inheritdoc/>
        protected override void OnGotFocus(EventArgs e) {
            base.OnGotFocus(e);

            if(_initialized && Enabled && !_disposingResources) {
                _comboBox.Select();
            }

            UpdateVisualState(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnMouseEnter(EventArgs e) {
            base.OnMouseEnter(e);

            if(!_isMouseOver) {
                _isMouseOver = true;
                UpdateVisualState(animate: true);
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);

            if(_isMouseOver) {
                _isMouseOver = false;
                UpdateVisualState(animate: true);
            }
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
            base.OnMouseUp(e);

            if(e.Button == MouseButtons.Left && _isPressed) {
                _isPressed = false;
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
        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);

            if(!Enabled) {
                _isPressed = false;
                CloseDropDown();
            }

            UpdateVisualState(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnBackColorChanged(EventArgs e) {
            base.OnBackColorChanged(e);
            RefreshAppearance();
        }

        /// <inheritdoc/>
        protected override void OnForeColorChanged(EventArgs e) {
            base.OnForeColorChanged(e);
            RefreshAppearance();
        }

        /// <inheritdoc/>
        protected override void OnFontChanged(EventArgs e) {
            base.OnFontChanged(e);

            if(_initialized) {
                _comboBox.Font = Font;
                _textSurface.Font = Font;
                UpdateChildBounds();
            }
        }

        /// <inheritdoc/>
        protected override void OnPaddingChanged(EventArgs e) {
            base.OnPaddingChanged(e);
            UpdateChildBounds();
        }

        /// <inheritdoc/>
        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            UpdateChildBounds();
            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnRightToLeftChanged(EventArgs e) {
            base.OnRightToLeftChanged(e);

            if(_initialized) {
                _comboBox.RightToLeft = RightToLeft;
                UpdateChildBounds();
            }
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
            } else {
                CloseDropDown();
                if(_animator.IsRunning) {
                    _animator.Pause();
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e) {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(_displayAppearance.BackColor);

            if(_borderSize > 0 && ClientSize.Width > 0 && ClientSize.Height > 0) {
                float maximumBorder = Math.Min(ClientSize.Width, ClientSize.Height) / 2f;
                float effectiveBorder = Math.Min(_borderSize, maximumBorder);
                float inset = effectiveBorder / 2f;
                RectangleF borderBounds = new RectangleF(
                    inset,
                    inset,
                    Math.Max(0f, ClientSize.Width - effectiveBorder),
                    Math.Max(0f, ClientSize.Height - effectiveBorder));

                if(borderBounds.Width > 0f && borderBounds.Height > 0f) {
                    using Pen borderPen = new Pen(_displayAppearance.BorderColor, effectiveBorder) {
                        Alignment = PenAlignment.Center
                    };
                    graphics.DrawRectangle(
                        borderPen,
                        borderBounds.X,
                        borderBounds.Y,
                        borderBounds.Width,
                        borderBounds.Height);
                }
            }

            base.OnPaint(e);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            if(disposing && !_disposingResources) {
                _disposingResources = true;

                _animator.Completed -= Animator_Completed;
                _animator.Canceled -= Animator_Canceled;
                _animator.StateChanged -= Animator_StateChanged;
                _animator.Dispose();

                _comboBox.SelectedIndexChanged -= ComboBox_SelectedIndexChanged;
                _comboBox.SelectionChangeCommitted -= ComboBox_SelectionChangeCommitted;
                _comboBox.TextChanged -= ComboBox_TextChanged;
                _comboBox.Enter -= ComboBox_Enter;
                _comboBox.Leave -= ComboBox_Leave;
                _comboBox.DropDown -= ComboBox_DropDown;
                _comboBox.DropDownClosed -= ComboBox_DropDownClosed;

                UnsubscribeSurfaceEvents(_textSurface);
                _iconSurface.Paint -= IconSurface_Paint;
                UnsubscribeSurfaceEvents(_iconSurface);

                OnSelectedIndexChanged = null;
                SelectedIndexChanged = null;
                SelectionChangeCommitted = null;
                DropDownOpened = null;
                DropDownClosed = null;
                VisualStateChanged = null;
                AnimationCompleted = null;
                AnimationCanceled = null;
                AnimationStateChanged = null;
            }

            base.Dispose(disposing);
        }

        private void SubscribeSurfaceEvents(Control surface) {
            surface.Click += Surface_Click;
            surface.MouseEnter += Surface_MouseEnter;
            surface.MouseLeave += Surface_MouseLeave;
            surface.MouseDown += Surface_MouseDown;
            surface.MouseUp += Surface_MouseUp;
            surface.MouseCaptureChanged += Surface_MouseCaptureChanged;
        }

        private void UnsubscribeSurfaceEvents(Control surface) {
            surface.Click -= Surface_Click;
            surface.MouseEnter -= Surface_MouseEnter;
            surface.MouseLeave -= Surface_MouseLeave;
            surface.MouseDown -= Surface_MouseDown;
            surface.MouseUp -= Surface_MouseUp;
            surface.MouseCaptureChanged -= Surface_MouseCaptureChanged;
        }

        private void Surface_Click(object? sender, EventArgs e) {
            OnClick(e);
        }

        private void Surface_MouseEnter(object? sender, EventArgs e) {
            if(!_isMouseOver) {
                _isMouseOver = true;
                base.OnMouseEnter(e);
                UpdateVisualState(animate: true);
            }
        }

        private void Surface_MouseLeave(object? sender, EventArgs e) {
            if(!IsHandleCreated || _disposingResources || IsDisposed) {
                return;
            }

            BeginInvoke(new Action(UpdatePointerPresence));
        }

        private void Surface_MouseDown(object? sender, MouseEventArgs e) {
            OnMouseDown(e);
        }

        private void Surface_MouseUp(object? sender, MouseEventArgs e) {
            OnMouseUp(e);
        }

        private void Surface_MouseCaptureChanged(object? sender, EventArgs e) {
            if(sender is Control surface && !surface.Capture && _isPressed) {
                _isPressed = false;
                UpdateVisualState(animate: true);
            }
        }

        private void UpdatePointerPresence() {
            if(_disposingResources || IsDisposed || !IsHandleCreated) {
                return;
            }

            bool containsPointer = ClientRectangle.Contains(PointToClient(MousePosition));
            if(!containsPointer && _isMouseOver) {
                _isMouseOver = false;
                base.OnMouseLeave(EventArgs.Empty);
                UpdateVisualState(animate: true);
            }
        }

        private void ComboBox_SelectedIndexChanged(object? sender, EventArgs e) {
            UpdateDisplayedText();
            SelectedIndexChanged?.Invoke(this, e);
            OnSelectedIndexChanged?.Invoke(this, e);
        }

        private void ComboBox_SelectionChangeCommitted(object? sender, EventArgs e) {
            SelectionChangeCommitted?.Invoke(this, e);
        }

        private void ComboBox_TextChanged(object? sender, EventArgs e) {
            UpdateDisplayedText();
        }

        private void ComboBox_Enter(object? sender, EventArgs e) {
            UpdateVisualState(animate: true);
        }

        private void ComboBox_Leave(object? sender, EventArgs e) {
            _isPressed = false;
            UpdateVisualState(animate: true);
        }

        private void ComboBox_DropDown(object? sender, EventArgs e) {
            _isDroppedDown = true;
            UpdateVisualState(animate: true);
            DropDownOpened?.Invoke(this, e);
        }

        private void ComboBox_DropDownClosed(object? sender, EventArgs e) {
            _isDroppedDown = false;
            UpdateVisualState(animate: true);
            DropDownClosed?.Invoke(this, e);
        }

        private void IconSurface_Paint(object? sender, PaintEventArgs e) {
            int size = Math.Min(_iconSize, Math.Min(_iconSurface.Width, _iconSurface.Height));
            if(size <= 0) {
                return;
            }

            Rectangle iconBounds = new Rectangle(
                (_iconSurface.Width - size) / 2,
                (_iconSurface.Height - size) / 2,
                size,
                size);
            float centerX = iconBounds.Left + (iconBounds.Width / 2f);
            float centerY = iconBounds.Top + (iconBounds.Height / 2f);
            GraphicsState state = e.Graphics.Save();

            try {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TranslateTransform(centerX, centerY);
                e.Graphics.RotateTransform(180f * _displayAppearance.DropDownProgress);
                e.Graphics.TranslateTransform(-centerX, -centerY);
                SaraUI_IconLibrary.DrawIcon(
                    "ChevronDown",
                    e.Graphics,
                    iconBounds,
                    _displayAppearance.IconColor);
            } finally {
                e.Graphics.Restore(state);
            }
        }

        private void UpdateDisplayedText() {
            if(!_initialized) {
                return;
            }

            _textSurface.Text = string.IsNullOrEmpty(_comboBox.Text)
                ? _placeholderText
                : _comboBox.Text;
        }

        private void UpdateChildBounds() {
            if(!_initialized || _disposingResources || IsDisposed) {
                return;
            }

            int left = Math.Max(Padding.Left, _borderSize);
            int top = Math.Max(Padding.Top, _borderSize);
            int right = Math.Min(ClientSize.Width, ClientSize.Width - Math.Max(Padding.Right, _borderSize));
            int bottom = Math.Min(ClientSize.Height, ClientSize.Height - Math.Max(Padding.Bottom, _borderSize));
            Rectangle contentBounds = Rectangle.FromLTRB(
                left,
                top,
                Math.Max(left, right),
                Math.Max(top, bottom));
            int iconWidth = Math.Min(_iconAreaWidth, contentBounds.Width);

            if(RightToLeft == RightToLeft.Yes) {
                _iconSurface.Bounds = new Rectangle(
                    contentBounds.Left,
                    contentBounds.Top,
                    iconWidth,
                    contentBounds.Height);
                _textSurface.Bounds = Rectangle.FromLTRB(
                    _iconSurface.Right,
                    contentBounds.Top,
                    contentBounds.Right,
                    contentBounds.Bottom);
            } else {
                _iconSurface.Bounds = new Rectangle(
                    contentBounds.Right - iconWidth,
                    contentBounds.Top,
                    iconWidth,
                    contentBounds.Height);
                _textSurface.Bounds = Rectangle.FromLTRB(
                    contentBounds.Left,
                    contentBounds.Top,
                    _iconSurface.Left,
                    contentBounds.Bottom);
            }

            int comboHeight = Math.Max(_comboBox.PreferredHeight, contentBounds.Height);
            _comboBox.Bounds = new Rectangle(
                contentBounds.Left,
                contentBounds.Bottom - comboHeight,
                contentBounds.Width,
                comboHeight);
            _comboBox.SendToBack();
            _textSurface.BringToFront();
            _iconSurface.BringToFront();
        }

        private SaraComboBoxVisualState DetermineVisualState() {
            if(!Enabled) {
                return SaraComboBoxVisualState.Disabled;
            }

            if(_isDroppedDown || _comboBox.DroppedDown) {
                return SaraComboBoxVisualState.DroppedDown;
            }

            if(_isPressed) {
                return SaraComboBoxVisualState.Pressed;
            }

            if(ContainsFocus) {
                return SaraComboBoxVisualState.Focused;
            }

            if(_isMouseOver) {
                return SaraComboBoxVisualState.Hovered;
            }

            return SaraComboBoxVisualState.Normal;
        }

        private ComboBoxAppearance ResolveAppearance(SaraComboBoxVisualState state) {
            Color backColor = BackColor;
            Color foreColor = ForeColor;
            Color borderColor = _borderColor;
            Color iconColor = _iconColor;
            float dropDownProgress = _isDroppedDown || _comboBox.DroppedDown ? 1f : 0f;

            switch(state) {
                case SaraComboBoxVisualState.Hovered:
                    backColor = _hoverBackColor.IsEmpty
                        ? Blend(backColor, _borderColor, 0.06f)
                        : _hoverBackColor;
                    borderColor = _hoverBorderColor.IsEmpty
                        ? Blend(_borderColor, _borderFocusColor, 0.38f)
                        : _hoverBorderColor;
                    break;
                case SaraComboBoxVisualState.Pressed:
                    backColor = _pressedBackColor.IsEmpty
                        ? Blend(backColor, Color.Black, 0.07f)
                        : _pressedBackColor;
                    borderColor = _borderFocusColor;
                    break;
                case SaraComboBoxVisualState.Focused:
                case SaraComboBoxVisualState.DroppedDown:
                    borderColor = _borderFocusColor;
                    backColor = _hoverBackColor.IsEmpty
                        ? Blend(backColor, _borderFocusColor, 0.04f)
                        : _hoverBackColor;
                    break;
                case SaraComboBoxVisualState.Disabled:
                    backColor = _disabledBackColor.IsEmpty
                        ? Blend(backColor, SystemColors.Control, 0.58f)
                        : _disabledBackColor;
                    foreColor = _disabledForeColor.IsEmpty
                        ? SystemColors.GrayText
                        : _disabledForeColor;
                    borderColor = _disabledBorderColor.IsEmpty
                        ? Blend(_borderColor, SystemColors.ControlDark, 0.55f)
                        : _disabledBorderColor;
                    iconColor = foreColor;
                    break;
            }

            return new ComboBoxAppearance(
                backColor,
                foreColor,
                borderColor,
                iconColor,
                dropDownProgress);
        }

        private void UpdateVisualState(bool animate) {
            if(!_initialized || _disposingResources || IsDisposed) {
                return;
            }

            SaraComboBoxVisualState nextState = DetermineVisualState();
            bool stateChanged = _visualState != nextState;
            _visualState = nextState;
            _targetAppearance = ResolveAppearance(nextState);

            if(stateChanged) {
                VisualStateChanged?.Invoke(this, EventArgs.Empty);
            }

            if(!animate || !CanAnimate() || _animationDuration == 0 ||
                _displayAppearance.Equals(_targetAppearance)) {
                ApplyTargetImmediately();
                return;
            }

            ComboBoxAppearance origin = _displayAppearance;
            ComboBoxAppearance destination = _targetAppearance;

            _animator.Start(
                0f,
                1f,
                progress => ApplyAppearance(
                    ComboBoxAppearance.Interpolate(origin, destination, progress)),
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
            ApplyAppearance(_targetAppearance);
        }

        private void ApplyAppearance(ComboBoxAppearance appearance) {
            _displayAppearance = appearance;
            _textSurface.BackColor = appearance.BackColor;
            _textSurface.ForeColor = appearance.ForeColor;
            _iconSurface.BackColor = appearance.BackColor;
            _textSurface.Invalidate();
            _iconSurface.Invalidate();
            Invalidate();
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

        private void RestartActiveAnimation() {
            if(_animator.IsRunning || _animator.IsPaused) {
                UpdateVisualState(animate: true);
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
            ApplyAppearance(_targetAppearance);
            AnimationCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void Animator_Canceled(object? sender, EventArgs e) {
            AnimationCanceled?.Invoke(this, EventArgs.Empty);
        }

        private void Animator_StateChanged(object? sender, EventArgs e) {
            AnimationStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private readonly struct ComboBoxAppearance:IEquatable<ComboBoxAppearance> {
            public ComboBoxAppearance(
                Color backColor,
                Color foreColor,
                Color borderColor,
                Color iconColor,
                float dropDownProgress) {
                BackColor = backColor;
                ForeColor = foreColor;
                BorderColor = borderColor;
                IconColor = iconColor;
                DropDownProgress = dropDownProgress;
            }

            public Color BackColor { get; }

            public Color ForeColor { get; }

            public Color BorderColor { get; }

            public Color IconColor { get; }

            public float DropDownProgress { get; }

            public bool Equals(ComboBoxAppearance other) {
                return BackColor == other.BackColor &&
                    ForeColor == other.ForeColor &&
                    BorderColor == other.BorderColor &&
                    IconColor == other.IconColor &&
                    Math.Abs(DropDownProgress - other.DropDownProgress) < 0.001f;
            }

            public override bool Equals(object? obj) {
                return obj is ComboBoxAppearance other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    int hashCode = BackColor.GetHashCode();
                    hashCode = (hashCode * 397) ^ ForeColor.GetHashCode();
                    hashCode = (hashCode * 397) ^ BorderColor.GetHashCode();
                    hashCode = (hashCode * 397) ^ IconColor.GetHashCode();
                    hashCode = (hashCode * 397) ^ DropDownProgress.GetHashCode();
                    return hashCode;
                }
            }

            public static ComboBoxAppearance Interpolate(
                ComboBoxAppearance origin,
                ComboBoxAppearance destination,
                float progress) {
                float amount = Math.Max(0f, Math.Min(1f, progress));
                return new ComboBoxAppearance(
                    Blend(origin.BackColor, destination.BackColor, amount),
                    Blend(origin.ForeColor, destination.ForeColor, amount),
                    Blend(origin.BorderColor, destination.BorderColor, amount),
                    Blend(origin.IconColor, destination.IconColor, amount),
                    origin.DropDownProgress +
                        ((destination.DropDownProgress - origin.DropDownProgress) * amount));
            }
        }
    }
}

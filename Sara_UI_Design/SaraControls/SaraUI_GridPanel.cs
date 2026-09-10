using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Sara_UI_Design.Animations;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Organiza controles en una cuadrícula de filas y columnas fijas o fraccionales,
    /// con posiciones, spans, alineación y reorganización animada opcional.
    /// </summary>
    /// <remarks>
    /// Los controles cuyo <see cref="Control.Dock"/> sea distinto de
    /// <see cref="DockStyle.None"/> permanecen bajo el motor de diseño nativo de
    /// Windows Forms. Las configuraciones aceptan valores como <c>120</c>,
    /// <c>120px</c> y <c>1.5fr</c>, separados por comas.
    /// </remarks>
    [ToolboxItem(true)]
    [DefaultEvent(nameof(Layout))]
    public class SaraUI_GridPanel:Panel {
        /// <summary>Define cómo se utiliza el espacio disponible dentro de una celda.</summary>
        public enum GridItemAlignment {
            /// <summary>Expande el control hasta ocupar el espacio disponible.</summary>
            Stretch,

            /// <summary>Conserva el tamaño del control y lo coloca al inicio.</summary>
            Start,

            /// <summary>Conserva el tamaño del control y lo centra.</summary>
            Center,

            /// <summary>Conserva el tamaño del control y lo coloca al final.</summary>
            End
        }

        private const int MaximumGap = 1000000;
        private const int MaximumBorderRadius = 1000000;
        private const int MaximumTrackCount = 1024;
        private const double MaximumTrackValue = 1000000d;
        private static readonly ConditionalWeakTable<Control, GridPlacement> Placements =
            new ConditionalWeakTable<Control, GridPlacement>();

        private readonly SaraAnimator _layoutAnimator;
        private readonly List<LayoutTransition> _activeTransitions = new List<LayoutTransition>();
        private readonly List<Control> _unplacedControls = new List<Control>();
        private string _columnsConfig = "1fr, 1fr";
        private string _rowsConfig = "1fr";
        private GridTrack[] _columnTracks = ParseConfiguration("1fr, 1fr", nameof(ColumnsConfig));
        private GridTrack[] _rowTracks = ParseConfiguration("1fr", nameof(RowsConfig));
        private int _columnGap = 10;
        private int _rowGap = 10;
        private int _borderRadius;
        private GridItemAlignment _justifyItems = GridItemAlignment.Stretch;
        private GridItemAlignment _alignItems = GridItemAlignment.Stretch;
        private bool _allowFormDrag;
        private bool _isDraggingForm;
        private Point _dragStartCursor;
        private Point _dragStartFormLocation;
        private Form? _draggedForm;
        private bool _animationEnabled;
        private int _animationDuration = 300;
        private int _animationFrameInterval = 15;
        private SaraEasing _animationEasing = SaraEasing.EaseInOutCubic;
        private bool _applyingLayout;
        private bool _hasAppliedLayout;
        private bool _disposingResources;
        private Region? _managedRegion;
        private Rectangle _managedRegionBounds = Rectangle.Empty;
        private int _managedRegionRadius = -1;
        private int[] _calculatedColumnSizes = Array.Empty<int>();
        private int[] _calculatedRowSizes = Array.Empty<int>();
        private int _unplacedControlCount;

        /// <summary>Inicializa una cuadrícula de dos columnas con espacio interno de diez píxeles.</summary>
        public SaraUI_GridPanel() {
            _layoutAnimator = new SaraAnimator();
            _layoutAnimator.Completed += LayoutAnimator_Completed;
            _layoutAnimator.Canceled += LayoutAnimator_Canceled;
            _layoutAnimator.StateChanged += LayoutAnimator_StateChanged;

            DoubleBuffered = true;
            Size = new Size(300, 200);
            base.Padding = new Padding(10);
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);
        }

        /// <summary>Se produce cuando una reorganización animada llega a su destino.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationCompleted;

        /// <summary>Se produce cuando una reorganización activa se cancela o se reemplaza.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationCanceled;

        /// <summary>Se produce cuando cambia el estado de la reorganización.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationStateChanged;

        /// <summary>Obtiene o establece las columnas fijas o fraccionales separadas por comas.</summary>
        /// <exception cref="ArgumentNullException">Se produce al asignar <see langword="null"/>.</exception>
        /// <exception cref="FormatException">Se produce si la sintaxis contiene una definición inválida.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Se produce si una definición supera los límites admitidos.</exception>
        [Category("Sara UI Design")]
        [DefaultValue("1fr, 1fr")]
        public string ColumnsConfig {
            get => _columnsConfig;
            set {
                string normalized = NormalizeConfiguration(value, nameof(ColumnsConfig));
                GridTrack[] parsed = ParseConfiguration(normalized, nameof(ColumnsConfig));

                if(string.Equals(_columnsConfig, normalized, StringComparison.Ordinal)) {
                    return;
                }

                _columnsConfig = normalized;
                _columnTracks = parsed;
                RequestGridLayout();
            }
        }

        /// <summary>Obtiene o establece las filas fijas o fraccionales separadas por comas.</summary>
        /// <exception cref="ArgumentNullException">Se produce al asignar <see langword="null"/>.</exception>
        /// <exception cref="FormatException">Se produce si la sintaxis contiene una definición invalida.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Se produce si una definición supera los límites admitidos.</exception>
        [Category("Sara UI Design")]
        [DefaultValue("1fr")]
        public string RowsConfig {
            get => _rowsConfig;
            set {
                string normalized = NormalizeConfiguration(value, nameof(RowsConfig));
                GridTrack[] parsed = ParseConfiguration(normalized, nameof(RowsConfig));

                if(string.Equals(_rowsConfig, normalized, StringComparison.Ordinal)) {
                    return;
                }

                _rowsConfig = normalized;
                _rowTracks = parsed;
                RequestGridLayout();
            }
        }

        /// <summary>Actualiza filas y columnas después de validar ambas configuraciones.</summary>
        /// <param name="columns">Nueva configuración de columnas.</param>
        /// <param name="rows">Nueva configuración de filas.</param>
        public void SetGridTemplate(string columns, string rows) {
            string normalizedColumns = NormalizeConfiguration(columns, nameof(columns));
            string normalizedRows = NormalizeConfiguration(rows, nameof(rows));
            GridTrack[] parsedColumns = ParseConfiguration(normalizedColumns, nameof(columns));
            GridTrack[] parsedRows = ParseConfiguration(normalizedRows, nameof(rows));

            if(string.Equals(_columnsConfig, normalizedColumns, StringComparison.Ordinal) &&
                string.Equals(_rowsConfig, normalizedRows, StringComparison.Ordinal)) {
                return;
            }

            _columnsConfig = normalizedColumns;
            _rowsConfig = normalizedRows;
            _columnTracks = parsedColumns;
            _rowTracks = parsedRows;
            RequestGridLayout();
        }

        /// <summary>Obtiene o establece la separación horizontal entre columnas.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor fuera del rango admitido.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(10)]
        public int ColumnGap {
            get => _columnGap;
            set => SetGaps(value, _rowGap);
        }

        /// <summary>Obtiene o establece la separación vertical entre filas.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor fuera del rango admitido.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(10)]
        public int RowGap {
            get => _rowGap;
            set => SetGaps(_columnGap, value);
        }

        /// <summary>Actualiza las separaciones de ambos ejes en una sola operación de diseño.</summary>
        /// <param name="columnGap">Separación horizontal entre cero y 1000000.</param>
        /// <param name="rowGap">Separación vertical entre cero y 1000000.</param>
        public void SetGaps(int columnGap, int rowGap) {
            EnsureRange(columnGap, 0, MaximumGap, nameof(columnGap));
            EnsureRange(rowGap, 0, MaximumGap, nameof(rowGap));

            if(_columnGap == columnGap && _rowGap == rowGap) {
                return;
            }

            _columnGap = columnGap;
            _rowGap = rowGap;
            RequestGridLayout();
        }

        /// <summary>Obtiene o establece la alineación horizontal dentro de cada celda.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar una alineación desconocida.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(GridItemAlignment.Stretch)]
        public GridItemAlignment JustifyItems {
            get => _justifyItems;
            set {
                EnsureDefined(value, nameof(JustifyItems));

                if(_justifyItems == value) {
                    return;
                }

                _justifyItems = value;
                RequestGridLayout();
            }
        }

        /// <summary>Obtiene o establece la alineación vertical dentro de cada celda.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar una alineación desconocida.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(GridItemAlignment.Stretch)]
        public GridItemAlignment AlignItems {
            get => _alignItems;
            set {
                EnsureDefined(value, nameof(AlignItems));

                if(_alignItems == value) {
                    return;
                }

                _alignItems = value;
                RequestGridLayout();
            }
        }

        /// <summary>Obtiene o establece el radio aplicado a la región exterior del panel.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor fuera del rango admitido.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(0)]
        public int BorderRadius {
            get => _borderRadius;
            set {
                EnsureRange(value, 0, MaximumBorderRadius, nameof(BorderRadius));

                if(_borderRadius == value) {
                    return;
                }

                _borderRadius = value;
                _managedRegionRadius = -1;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece si el fondo del panel permite arrastrar el formulario contenedor.</summary>
        [Category("Sara UI Design")]
        [Description("Permite mover la ventana completa arrastrando el fondo del Grid.")]
        [DefaultValue(false)]
        public bool AllowFormDrag {
            get => _allowFormDrag;
            set {
                if(_allowFormDrag == value) {
                    return;
                }

                _allowFormDrag = value;

                if(!value) {
                    EndFormDrag();
                }
            }
        }

        /// <summary>Obtiene o establece el espacio interior utilizado por la cuadrícula.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce si algún lado es negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Padding), "10, 10, 10, 10")]
        public new Padding Padding {
            get => base.Padding;
            set {
                EnsurePadding(value);

                if(base.Padding != value) {
                    base.Padding = value;
                }
            }
        }

        /// <summary>Obtiene o establece si deben animarse las reorganizaciones.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(false)]
        public bool AnimationEnabled {
            get => _animationEnabled;
            set {
                if(_animationEnabled == value) {
                    return;
                }

                _animationEnabled = value;

                if(!value) {
                    StopAnimation();
                }

                RequestGridLayout();
            }
        }

        /// <summary>Obtiene o establece la duración de la reorganización, en milisegundos.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(300)]
        public int AnimationDuration {
            get => _animationDuration;
            set {
                EnsureRange(value, 0, int.MaxValue, nameof(AnimationDuration));

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
                EnsureRange(value, 1, int.MaxValue, nameof(AnimationFrameInterval));

                if(_animationFrameInterval == value) {
                    return;
                }

                _animationFrameInterval = value;
                RestartActiveAnimation();
            }
        }

        /// <summary>Obtiene o establece la curva aplicada a la reorganización.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar una curva desconocida.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(SaraEasing.EaseInOutCubic)]
        public SaraEasing AnimationEasing {
            get => _animationEasing;
            set {
                EnsureDefined(value, nameof(AnimationEasing));

                if(_animationEasing == value) {
                    return;
                }

                _animationEasing = value;
                RestartActiveAnimation();
            }
        }

        /// <summary>Obtiene la cantidad de columnas configuradas.</summary>
        [Browsable(false)]
        public int ColumnCount => _columnTracks.Length;

        /// <summary>Obtiene la cantidad de filas configuradas.</summary>
        [Browsable(false)]
        public int RowCount => _rowTracks.Length;

        /// <summary>Obtiene la cantidad de controles visibles que no encontraron una celda disponible.</summary>
        [Browsable(false)]
        public int UnplacedControlCount => _unplacedControlCount;

        /// <summary>Obtiene si actualmente se está arrastrando el formulario contenedor.</summary>
        [Browsable(false)]
        public bool IsDraggingForm => _isDraggingForm;

        /// <summary>Obtiene el estado actual de la reorganización animada.</summary>
        [Browsable(false)]
        public SaraAnimationState AnimationState => _layoutAnimator.State;

        /// <summary>Devuelve una copia de los anchos calculados en el último layout.</summary>
        public int[] GetCalculatedColumnSizes() {
            return (int[])_calculatedColumnSizes.Clone();
        }

        /// <summary>Devuelve una copia de los altos calculados en el último layout.</summary>
        public int[] GetCalculatedRowSizes() {
            return (int[])_calculatedRowSizes.Clone();
        }

        /// <summary>Pausa la reorganización activa conservando los límites actuales.</summary>
        /// <returns><see langword="true"/> si la animación cambió al estado pausado.</returns>
        public bool PauseAnimation() {
            return _layoutAnimator.Pause();
        }

        /// <summary>Reanuda una reorganización pausada.</summary>
        /// <returns><see langword="true"/> si la animación volvió a ejecutarse.</returns>
        public bool ResumeAnimation() {
            return _layoutAnimator.Resume();
        }

        /// <summary>Detiene la animación y aplica inmediatamente los límites calculados.</summary>
        /// <returns><see langword="true"/> si existía una animación activa.</returns>
        public bool StopAnimation() {
            bool stopped = StopAnimatorIfActive();
            ApplyActiveDestinations();
            _activeTransitions.Clear();
            return stopped;
        }

        /// <summary>Asocia un control con una fila y una columna sin modificar su propiedad <see cref="Control.Tag"/>.</summary>
        public static void SetGridPosition(Control control, int row, int column) {
            SetGridPosition(control, row, column, 1, 1);
        }

        /// <summary>Asocia un control con una celda y permite que ocupe varias filas o columnas.</summary>
        /// <exception cref="ArgumentNullException">Se produce si el control es <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Se produce si los índices o spans no son válidos.</exception>
        public static void SetGridPosition(
            Control control,
            int row,
            int column,
            int rowSpan,
            int columnSpan) {
            if(control is null) {
                throw new ArgumentNullException(nameof(control));
            }

            EnsureRange(row, 0, int.MaxValue, nameof(row));
            EnsureRange(column, 0, int.MaxValue, nameof(column));
            EnsureRange(rowSpan, 1, int.MaxValue, nameof(rowSpan));
            EnsureRange(columnSpan, 1, int.MaxValue, nameof(columnSpan));

            Placements.Remove(control);
            Placements.Add(control, new GridPlacement(row, column, rowSpan, columnSpan));
            RequestParentGridLayout(control);
        }

        /// <summary>Elimina la posición asociada y devuelve el control al flujo automático.</summary>
        /// <returns><see langword="true"/> si existía una posición asociada.</returns>
        public static bool ClearGridPosition(Control control) {
            if(control is null) {
                throw new ArgumentNullException(nameof(control));
            }

            bool removed = Placements.Remove(control);

            if(removed) {
                RequestParentGridLayout(control);
            }

            return removed;
        }

        /// <summary>Obtiene la fila asociada o cero cuando el control usa colocación automática.</summary>
        public static int GetGridRow(Control control) => GetPlacementOrDefault(control).Row;

        /// <summary>Obtiene la columna asociada o cero cuando el control usa colocación automática.</summary>
        public static int GetGridColumn(Control control) => GetPlacementOrDefault(control).Column;

        /// <summary>Obtiene el span de filas asociado o uno cuando el control usa colocación automática.</summary>
        public static int GetGridRowSpan(Control control) => GetPlacementOrDefault(control).RowSpan;

        /// <summary>Obtiene el span de columnas asociado o uno cuando el control usa colocación automática.</summary>
        public static int GetGridColumnSpan(Control control) => GetPlacementOrDefault(control).ColumnSpan;

        /// <summary>Indica si el control posee una posición asociada o una coordenada histórica válida en <see cref="Control.Tag"/>.</summary>
        public static bool HasGridPosition(Control control) {
            if(control is null) {
                throw new ArgumentNullException(nameof(control));
            }

            return TryGetPlacement(control, out _);
        }

        /// <inheritdoc/>
        protected override void OnLayout(LayoutEventArgs levent) {
            if(_applyingLayout) {
                return;
            }

            base.OnLayout(levent);

            if(_disposingResources || IsDisposed) {
                return;
            }

            List<LayoutTarget> targets = CalculateLayoutTargets();
            ApplyCalculatedLayout(targets);
            _hasAppliedLayout = true;
        }

        /// <inheritdoc/>
        protected override void OnRightToLeftChanged(EventArgs e) {
            base.OnRightToLeftChanged(e);
            RequestGridLayout();
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            if(!_allowFormDrag || e.Button != MouseButtons.Left || _isDraggingForm) {
                return;
            }

            Form? parentForm = FindForm();

            if(parentForm == null || parentForm.WindowState == FormWindowState.Maximized) {
                return;
            }

            _draggedForm = parentForm;
            _dragStartCursor = Cursor.Position;
            _dragStartFormLocation = parentForm.Location;
            _isDraggingForm = true;
            Capture = true;
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);

            if(!_isDraggingForm || _draggedForm == null || _draggedForm.IsDisposed) {
                return;
            }

            Point cursorPosition = Cursor.Position;
            _draggedForm.Location = new Point(
                _dragStartFormLocation.X + cursorPosition.X - _dragStartCursor.X,
                _dragStartFormLocation.Y + cursorPosition.Y - _dragStartCursor.Y);
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);

            if(e.Button == MouseButtons.Left) {
                EndFormDrag();
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseCaptureChanged(EventArgs e) {
            base.OnMouseCaptureChanged(e);

            if(!Capture) {
                EndFormDrag();
            }
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e) {
            UpdateRoundedRegion();
            base.OnPaint(e);
        }

        /// <inheritdoc/>
        protected override void OnHandleDestroyed(EventArgs e) {
            EndFormDrag();
            StopAnimatorIfActive();
            _activeTransitions.Clear();
            _hasAppliedLayout = false;
            base.OnHandleDestroyed(e);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            if(disposing && !_disposingResources) {
                _disposingResources = true;
                EndFormDrag();
                _layoutAnimator.Completed -= LayoutAnimator_Completed;
                _layoutAnimator.Canceled -= LayoutAnimator_Canceled;
                _layoutAnimator.StateChanged -= LayoutAnimator_StateChanged;
                _layoutAnimator.Dispose();
                _activeTransitions.Clear();
                _unplacedControls.Clear();
                ReleaseManagedRegion();
                AnimationCompleted = null;
                AnimationCanceled = null;
                AnimationStateChanged = null;
            }

            base.Dispose(disposing);
        }

        private void RequestGridLayout() {
            if(_disposingResources || IsDisposed || _applyingLayout) {
                return;
            }

            PerformLayout();
            Invalidate();
        }

        private List<LayoutTarget> CalculateLayoutTargets() {
            Rectangle contentBounds = GetGridContentBounds();
            _calculatedColumnSizes = CalculateTrackSizes(
                _columnTracks,
                contentBounds.Width,
                _columnGap,
                out int effectiveColumnGap);
            _calculatedRowSizes = CalculateTrackSizes(
                _rowTracks,
                contentBounds.Height,
                _rowGap,
                out int effectiveRowGap);

            List<LayoutTarget> targets = new List<LayoutTarget>();
            _unplacedControlCount = 0;
            _unplacedControls.Clear();

            if(_calculatedColumnSizes.Length == 0 || _calculatedRowSizes.Length == 0) {
                return targets;
            }

            List<GridItem> explicitItems = new List<GridItem>();
            List<GridItem> automaticItems = new List<GridItem>();

            for(int controlIndex = 0; controlIndex < Controls.Count; controlIndex++) {
                Control control = Controls[controlIndex];

                if(!IsManagedControl(control)) {
                    continue;
                }

                if(TryGetPlacement(control, out GridPlacement placement)) {
                    explicitItems.Add(new GridItem(control, placement));
                } else {
                    automaticItems.Add(new GridItem(control, null));
                }
            }

            bool[,] occupied = new bool[RowCount, ColumnCount];

            foreach(GridItem item in explicitItems) {
                if(!TryCreateTarget(
                    item.Control,
                    item.Placement!,
                    contentBounds,
                    effectiveColumnGap,
                    effectiveRowGap,
                    out LayoutTarget? target)) {
                    _unplacedControlCount++;
                    _unplacedControls.Add(item.Control);
                    continue;
                }

                MarkOccupied(occupied, item.Placement!);
                targets.Add(target!);
            }

            foreach(GridItem item in automaticItems) {
                if(!TryFindAutomaticPlacement(occupied, out GridPlacement? placement) ||
                    !TryCreateTarget(
                        item.Control,
                        placement!,
                        contentBounds,
                        effectiveColumnGap,
                        effectiveRowGap,
                        out LayoutTarget? target)) {
                    _unplacedControlCount++;
                    _unplacedControls.Add(item.Control);
                    continue;
                }

                MarkOccupied(occupied, placement!);
                targets.Add(target!);
            }

            return targets;
        }

        private Rectangle GetGridContentBounds() {
            int left = Math.Min(ClientSize.Width, base.Padding.Left);
            int top = Math.Min(ClientSize.Height, base.Padding.Top);
            int right = Math.Max(left, ClientSize.Width - base.Padding.Right);
            int bottom = Math.Max(top, ClientSize.Height - base.Padding.Bottom);

            foreach(Control control in Controls) {
                if(!control.Visible || control.Dock == DockStyle.None) {
                    continue;
                }

                switch(control.Dock) {
                    case DockStyle.Top:
                    top = Math.Min(bottom, Math.Max(top, control.Bottom));
                    break;

                    case DockStyle.Bottom:
                    bottom = Math.Max(top, Math.Min(bottom, control.Top));
                    break;

                    case DockStyle.Left:
                    left = Math.Min(right, Math.Max(left, control.Right));
                    break;

                    case DockStyle.Right:
                    right = Math.Max(left, Math.Min(right, control.Left));
                    break;

                    case DockStyle.Fill:
                    return Rectangle.Empty;
                }
            }

            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        private bool TryCreateTarget(
            Control control,
            GridPlacement placement,
            Rectangle contentBounds,
            int effectiveColumnGap,
            int effectiveRowGap,
            out LayoutTarget? target) {
            target = null;

            if(placement.Row >= RowCount || placement.Column >= ColumnCount) {
                return false;
            }

            int rowSpan = Math.Min(placement.RowSpan, RowCount - placement.Row);
            int columnSpan = Math.Min(placement.ColumnSpan, ColumnCount - placement.Column);
            int relativeX = SumTracks(_calculatedColumnSizes, 0, placement.Column, effectiveColumnGap);
            int relativeY = SumTracks(_calculatedRowSizes, 0, placement.Row, effectiveRowGap);
            int cellWidth = SumTracks(
                _calculatedColumnSizes,
                placement.Column,
                columnSpan,
                effectiveColumnGap);
            int cellHeight = SumTracks(
                _calculatedRowSizes,
                placement.Row,
                rowSpan,
                effectiveRowGap);
            int x = RightToLeft == RightToLeft.Yes
                ? contentBounds.Right - relativeX - cellWidth
                : contentBounds.Left + relativeX;
            int y = contentBounds.Top + relativeY;
            target = new LayoutTarget(
                control,
                AlignControl(control, new Rectangle(x, y, cellWidth, cellHeight)));
            return true;
        }

        private Rectangle AlignControl(Control control, Rectangle cellBounds) {
            Padding margin = control.Margin;
            int availableWidth = SubtractNonNegative(cellBounds.Width, margin.Left, margin.Right);
            int availableHeight = SubtractNonNegative(cellBounds.Height, margin.Top, margin.Bottom);
            int width = _justifyItems == GridItemAlignment.Stretch
                ? availableWidth
                : Math.Min(Math.Max(0, control.Width), availableWidth);
            int height = _alignItems == GridItemAlignment.Stretch
                ? availableHeight
                : Math.Min(Math.Max(0, control.Height), availableHeight);
            int x = ResolveHorizontalPosition(
                cellBounds,
                margin,
                width,
                availableWidth,
                _justifyItems,
                RightToLeft == RightToLeft.Yes);
            int y = ResolveVerticalPosition(
                cellBounds,
                margin,
                height,
                availableHeight,
                _alignItems);
            return new Rectangle(x, y, width, height);
        }

        private static int ResolveHorizontalPosition(
            Rectangle cellBounds,
            Padding margin,
            int width,
            int availableWidth,
            GridItemAlignment alignment,
            bool rightToLeft) {
            int start = cellBounds.Left + margin.Left;
            int remaining = Math.Max(0, availableWidth - width);

            if(alignment == GridItemAlignment.Center) {
                return start + (remaining / 2);
            }

            bool placeAtEnd = alignment == GridItemAlignment.End;

            if(rightToLeft && alignment != GridItemAlignment.Stretch) {
                placeAtEnd = !placeAtEnd;
            }

            return placeAtEnd ? start + remaining : start;
        }

        private static int ResolveVerticalPosition(
            Rectangle cellBounds,
            Padding margin,
            int height,
            int availableHeight,
            GridItemAlignment alignment) {
            int start = cellBounds.Top + margin.Top;
            int remaining = Math.Max(0, availableHeight - height);

            if(alignment == GridItemAlignment.Center) {
                return start + (remaining / 2);
            }

            return alignment == GridItemAlignment.End ? start + remaining : start;
        }

        private static int[] CalculateTrackSizes(
            GridTrack[] tracks,
            int availableSize,
            int configuredGap,
            out int effectiveGap) {
            int safeAvailable = Math.Max(0, availableSize);
            int gapCount = Math.Max(0, tracks.Length - 1);
            effectiveGap = gapCount == 0
                ? 0
                : Math.Min(configuredGap, safeAvailable / gapCount);
            int trackSpace = Math.Max(0, safeAvailable - (effectiveGap * gapCount));
            double fixedTotal = 0d;
            double fractionTotal = 0d;

            foreach(GridTrack track in tracks) {
                if(track.IsFractional) {
                    fractionTotal += track.Value;
                } else {
                    fixedTotal += track.Value;
                }
            }

            double remainingForFractions = Math.Max(0d, trackSpace - fixedTotal);
            double[] desiredSizes = new double[tracks.Length];
            double desiredTotal = 0d;

            for(int index = 0; index < tracks.Length; index++) {
                GridTrack track = tracks[index];
                double desired = track.IsFractional
                    ? (fractionTotal > 0d
                        ? remainingForFractions * track.Value / fractionTotal
                        : 0d)
                    : track.Value;
                desiredSizes[index] = desired;
                desiredTotal += desired;
            }

            double scale = desiredTotal > trackSpace && desiredTotal > 0d
                ? trackSpace / desiredTotal
                : 1d;
            int[] sizes = new int[tracks.Length];
            double cumulative = 0d;
            int assigned = 0;

            for(int index = 0; index < desiredSizes.Length; index++) {
                cumulative += desiredSizes[index] * scale;
                int roundedCumulative = Math.Min(
                    trackSpace,
                    Math.Max(assigned, ToInt32(cumulative)));
                sizes[index] = roundedCumulative - assigned;
                assigned = roundedCumulative;
            }

            return sizes;
        }

        private static int SumTracks(int[] sizes, int start, int count, int gap) {
            long total = 0L;

            for(int index = 0; index < count; index++) {
                if(index > 0) {
                    total += gap;
                }

                total += sizes[start + index];
            }

            return total >= int.MaxValue ? int.MaxValue : (int)total;
        }

        private static int SubtractNonNegative(int value, int first, int second) {
            long result = (long)value - first - second;

            if(result <= 0L) {
                return 0;
            }

            return result >= int.MaxValue ? int.MaxValue : (int)result;
        }

        private static void MarkOccupied(bool[,] occupied, GridPlacement placement) {
            int rowLimit = Math.Min(occupied.GetLength(0), SafeAdd(placement.Row, placement.RowSpan));
            int columnLimit = Math.Min(
                occupied.GetLength(1),
                SafeAdd(placement.Column, placement.ColumnSpan));

            for(int row = placement.Row; row < rowLimit; row++) {
                for(int column = placement.Column; column < columnLimit; column++) {
                    occupied[row, column] = true;
                }
            }
        }

        private static bool TryFindAutomaticPlacement(
            bool[,] occupied,
            out GridPlacement? placement) {
            for(int row = 0; row < occupied.GetLength(0); row++) {
                for(int column = 0; column < occupied.GetLength(1); column++) {
                    if(!occupied[row, column]) {
                        placement = new GridPlacement(row, column, 1, 1);
                        return true;
                    }
                }
            }

            placement = null;
            return false;
        }

        private void ApplyCalculatedLayout(List<LayoutTarget> targets) {
            MoveUnplacedControlsOutsideView();

            if(TargetsMatchActiveAnimation(targets)) {
                return;
            }

            List<LayoutTransition> transitions = CreateTransitions(targets);

            if(!CanAnimateLayout() || transitions.Count == 0) {
                StopAnimatorIfActive();
                _activeTransitions.Clear();
                ApplyTargetsImmediately(targets);
                return;
            }

            StartAnimation(transitions);
        }

        private static List<LayoutTransition> CreateTransitions(List<LayoutTarget> targets) {
            List<LayoutTransition> transitions = new List<LayoutTransition>();

            foreach(LayoutTarget target in targets) {
                Rectangle origin = target.Control.Bounds;

                if(origin != target.Destination) {
                    transitions.Add(new LayoutTransition(target.Control, origin, target.Destination));
                }
            }

            return transitions;
        }

        private void StartAnimation(List<LayoutTransition> transitions) {
            _activeTransitions.Clear();
            _activeTransitions.AddRange(transitions);
            LayoutTransition[] snapshot = transitions.ToArray();

            _layoutAnimator.Start(
                0f,
                1f,
                progress => ApplyAnimationFrame(snapshot, progress),
                new SaraAnimationOptions {
                    Duration = _animationDuration,
                    FrameInterval = _animationFrameInterval,
                    Easing = _animationEasing
                });
        }

        private void RestartActiveAnimation() {
            if(!_layoutAnimator.IsRunning && !_layoutAnimator.IsPaused) {
                return;
            }

            List<LayoutTransition> transitions = new List<LayoutTransition>();

            foreach(LayoutTransition active in _activeTransitions) {
                if(IsManagedControl(active.Control) && active.Control.Bounds != active.Destination) {
                    transitions.Add(new LayoutTransition(
                        active.Control,
                        active.Control.Bounds,
                        active.Destination));
                }
            }

            if(!CanAnimateLayout() || transitions.Count == 0) {
                StopAnimation();
                return;
            }

            StartAnimation(transitions);
        }

        private bool TargetsMatchActiveAnimation(List<LayoutTarget> targets) {
            if(!_layoutAnimator.IsRunning && !_layoutAnimator.IsPaused) {
                return false;
            }

            foreach(LayoutTransition transition in _activeTransitions) {
                LayoutTarget? target = FindTarget(targets, transition.Control);

                if(target == null || target.Destination != transition.Destination) {
                    return false;
                }
            }

            foreach(LayoutTarget target in targets) {
                LayoutTransition? transition = FindTransition(_activeTransitions, target.Control);

                if(transition == null && target.Control.Bounds != target.Destination) {
                    return false;
                }
            }

            return true;
        }

        private static LayoutTarget? FindTarget(List<LayoutTarget> targets, Control control) {
            foreach(LayoutTarget target in targets) {
                if(target.Control == control) {
                    return target;
                }
            }

            return null;
        }

        private static LayoutTransition? FindTransition(
            List<LayoutTransition> transitions,
            Control control) {
            foreach(LayoutTransition transition in transitions) {
                if(transition.Control == control) {
                    return transition;
                }
            }

            return null;
        }

        private void ApplyAnimationFrame(LayoutTransition[] transitions, float progress) {
            _applyingLayout = true;

            try {
                foreach(LayoutTransition transition in transitions) {
                    if(IsManagedControl(transition.Control)) {
                        transition.Control.Bounds = InterpolateRectangle(
                            transition.Origin,
                            transition.Destination,
                            progress);
                    }
                }
            } finally {
                _applyingLayout = false;
            }
        }

        private void ApplyTargetsImmediately(List<LayoutTarget> targets) {
            _applyingLayout = true;

            try {
                foreach(LayoutTarget target in targets) {
                    if(IsManagedControl(target.Control) && target.Control.Bounds != target.Destination) {
                        target.Control.Bounds = target.Destination;
                    }
                }
            } finally {
                _applyingLayout = false;
            }
        }

        private void MoveUnplacedControlsOutsideView() {
            if(_unplacedControls.Count == 0) {
                return;
            }

            Point outsideLocation = new Point(ClientSize.Width, ClientSize.Height);
            _applyingLayout = true;

            try {
                foreach(Control control in _unplacedControls) {
                    if(IsManagedControl(control) && control.Location != outsideLocation) {
                        control.Location = outsideLocation;
                    }
                }
            } finally {
                _applyingLayout = false;
            }
        }

        private void ApplyActiveDestinations() {
            _applyingLayout = true;

            try {
                foreach(LayoutTransition transition in _activeTransitions) {
                    if(IsManagedControl(transition.Control) &&
                        transition.Control.Bounds != transition.Destination) {
                        transition.Control.Bounds = transition.Destination;
                    }
                }
            } finally {
                _applyingLayout = false;
            }
        }

        private bool CanAnimateLayout() {
            return _animationEnabled &&
                _animationDuration > 0 &&
                _hasAppliedLayout &&
                IsHandleCreated &&
                Visible &&
                !IsInDesignMode() &&
                !_disposingResources &&
                !Disposing &&
                !IsDisposed;
        }

        private bool IsInDesignMode() {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
                (Site?.DesignMode ?? false);
        }

        private void UpdateRoundedRegion() {
            Rectangle bounds = ClientRectangle;
            int radius = Math.Min(
                _borderRadius,
                Math.Max(0, Math.Min(bounds.Width, bounds.Height) / 2));

            if(_managedRegionBounds == bounds && _managedRegionRadius == radius) {
                return;
            }

            _managedRegionBounds = bounds;
            _managedRegionRadius = radius;
            Region? nextRegion = null;

            if(radius > 0 && bounds.Width > 0 && bounds.Height > 0) {
                using GraphicsPath path = CreateRoundedPath(bounds, radius);
                nextRegion = new Region(path);
            }

            Region = nextRegion;
            _managedRegion?.Dispose();
            _managedRegion = nextRegion;
        }

        private void ReleaseManagedRegion() {
            if(_managedRegion == null) {
                return;
            }

            Region = null;
            _managedRegion.Dispose();
            _managedRegion = null;
            _managedRegionBounds = Rectangle.Empty;
            _managedRegionRadius = -1;
        }

        private void EndFormDrag() {
            _isDraggingForm = false;
            _draggedForm = null;

            if(Capture) {
                Capture = false;
            }
        }

        private bool IsManagedControl(Control control) {
            return !control.IsDisposed &&
                control.Parent == this &&
                control.Visible &&
                control.Dock == DockStyle.None;
        }

        private bool StopAnimatorIfActive() {
            return (_layoutAnimator.IsRunning || _layoutAnimator.IsPaused) &&
                _layoutAnimator.Stop();
        }

        private void LayoutAnimator_Completed(object? sender, EventArgs e) {
            ApplyActiveDestinations();
            _activeTransitions.Clear();
            AnimationCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void LayoutAnimator_Canceled(object? sender, EventArgs e) {
            AnimationCanceled?.Invoke(this, EventArgs.Empty);
        }

        private void LayoutAnimator_StateChanged(object? sender, EventArgs e) {
            AnimationStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private static void RequestParentGridLayout(Control control) {
            if(control.Parent is SaraUI_GridPanel panel) {
                panel.RequestGridLayout();
            }
        }

        private static GridPlacement GetPlacementOrDefault(Control control) {
            if(control is null) {
                throw new ArgumentNullException(nameof(control));
            }

            return TryGetPlacement(control, out GridPlacement placement)
                ? placement
                : new GridPlacement(0, 0, 1, 1);
        }

        private static bool TryGetPlacement(Control control, out GridPlacement placement) {
            if(Placements.TryGetValue(control, out GridPlacement? attached) && attached != null) {
                placement = attached;
                return true;
            }

            if(TryParseLegacyTag(control.Tag, out GridPlacement? legacy) && legacy != null) {
                placement = legacy;
                return true;
            }

            placement = new GridPlacement(0, 0, 1, 1);
            return false;
        }

        private static bool TryParseLegacyTag(object? tag, out GridPlacement? placement) {
            placement = null;
            string? text = tag?.ToString();

            if(text is null || text.Trim().Length == 0) {
                return false;
            }

            string[] parts = text.Split(',');

            if(parts.Length != 2 ||
                !int.TryParse(parts[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int row) ||
                !int.TryParse(parts[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int column) ||
                row < 0 || column < 0) {
                return false;
            }

            placement = new GridPlacement(row, column, 1, 1);
            return true;
        }

        private static string NormalizeConfiguration(string value, string propertyName) {
            if(value is null) {
                throw new ArgumentNullException(propertyName);
            }

            string normalized = value.Trim();
            return normalized.Length == 0 ? "1fr" : normalized;
        }

        private static GridTrack[] ParseConfiguration(string configuration, string propertyName) {
            string[] parts = configuration.Split(',');

            if(parts.Length > MaximumTrackCount) {
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    configuration,
                    $"La configuración no puede contener más de {MaximumTrackCount} elementos.");
            }

            GridTrack[] tracks = new GridTrack[parts.Length];

            for(int index = 0; index < parts.Length; index++) {
                string token = parts[index].Trim();

                if(token.Length == 0) {
                    throw new FormatException(
                        $"La definición {index + 1} de {propertyName} está vacía.");
                }

                bool isFractional = token.EndsWith("fr", StringComparison.OrdinalIgnoreCase);
                bool usesPixels = token.EndsWith("px", StringComparison.OrdinalIgnoreCase);
                string numericText = isFractional || usesPixels
                    ? token.Substring(0, token.Length - 2).Trim()
                    : token;

                if(!double.TryParse(
                    numericText,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double value) ||
                    double.IsNaN(value) ||
                    double.IsInfinity(value)) {
                    throw new FormatException(
                        $"'{token}' no es una definición válida en {propertyName}.");
                }

                if((isFractional && value <= 0d) || (!isFractional && value < 0d)) {
                    throw new ArgumentOutOfRangeException(
                        propertyName,
                        configuration,
                        "Las fracciones deben ser mayores que cero y los píxeles no pueden ser negativos.");
                }

                if(value > MaximumTrackValue) {
                    throw new ArgumentOutOfRangeException(
                        propertyName,
                        configuration,
                        $"Cada definición debe ser menor o igual que {MaximumTrackValue}.");
                }

                tracks[index] = new GridTrack(value, isFractional);
            }

            return tracks;
        }

        private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius) {
            GraphicsPath path = new GraphicsPath();

            if(bounds.Width <= 0 || bounds.Height <= 0) {
                return path;
            }

            int safeRadius = Math.Min(
                Math.Max(0, radius),
                Math.Min(bounds.Width, bounds.Height) / 2);

            if(safeRadius == 0) {
                path.AddRectangle(bounds);
                return path;
            }

            int diameter = safeRadius * 2;
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

        private static Rectangle InterpolateRectangle(
            Rectangle origin,
            Rectangle destination,
            float progress) {
            float amount = Math.Max(0f, Math.Min(1f, progress));
            return new Rectangle(
                ToInt32(origin.X + ((destination.X - origin.X) * amount)),
                ToInt32(origin.Y + ((destination.Y - origin.Y) * amount)),
                ToInt32(origin.Width + ((destination.Width - origin.Width) * amount)),
                ToInt32(origin.Height + ((destination.Height - origin.Height) * amount)));
        }

        private static int ToInt32(double value) {
            if(value >= int.MaxValue) {
                return int.MaxValue;
            }

            if(value <= int.MinValue) {
                return int.MinValue;
            }

            return (int)Math.Round(value, MidpointRounding.AwayFromZero);
        }

        private static int SafeAdd(int first, int second) {
            long result = (long)first + second;
            return result >= int.MaxValue ? int.MaxValue : (int)result;
        }

        private static void EnsureRange(int value, int minimum, int maximum, string propertyName) {
            if(value < minimum || value > maximum) {
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    value,
                    $"El valor debe encontrarse entre {minimum} y {maximum}.");
            }
        }

        private static void EnsurePadding(Padding value) {
            EnsureRange(value.Left, 0, MaximumGap, nameof(Padding));
            EnsureRange(value.Top, 0, MaximumGap, nameof(Padding));
            EnsureRange(value.Right, 0, MaximumGap, nameof(Padding));
            EnsureRange(value.Bottom, 0, MaximumGap, nameof(Padding));
        }

        private static void EnsureDefined<TEnum>(TEnum value, string propertyName)
            where TEnum:struct, Enum {
            if(!Enum.IsDefined(typeof(TEnum), value)) {
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    value,
                    "El valor indicado no pertenece a la enumeración.");
            }
        }

        private readonly struct GridTrack {
            public GridTrack(double value, bool isFractional) {
                Value = value;
                IsFractional = isFractional;
            }

            public double Value { get; }

            public bool IsFractional { get; }
        }

        private sealed class GridPlacement {
            public GridPlacement(int row, int column, int rowSpan, int columnSpan) {
                Row = row;
                Column = column;
                RowSpan = rowSpan;
                ColumnSpan = columnSpan;
            }

            public int Row { get; }
            public int Column { get; }
            public int RowSpan { get; }
            public int ColumnSpan { get; }
        }

        private sealed class GridItem {
            public GridItem(Control control, GridPlacement? placement) {
                Control = control;
                Placement = placement;
            }

            public Control Control { get; }
            public GridPlacement? Placement { get; }
        }

        private sealed class LayoutTarget {
            public LayoutTarget(Control control, Rectangle destination) {
                Control = control;
                Destination = destination;
            }

            public Control Control { get; }
            public Rectangle Destination { get; }
        }

        private sealed class LayoutTransition {
            public LayoutTransition(Control control, Rectangle origin, Rectangle destination) {
                Control = control;
                Origin = origin;
                Destination = destination;
            }

            public Control Control { get; }
            public Rectangle Origin { get; }
            public Rectangle Destination { get; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sara_UI_Design.Animations;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Organiza controles visibles en filas o columnas mediante un modelo inspirado en
    /// Flexbox, con separación, márgenes, envoltura y reorganización animada opcional.
    /// </summary>
    /// <remarks>
    /// Los controles cuyo <see cref="Control.Dock"/> sea distinto de
    /// <see cref="DockStyle.None"/> permanecen bajo el motor de diseño nativo de Windows
    /// Forms y no participan en la distribución flexible.
    /// </remarks>
    [ToolboxItem(true)]
    [DefaultEvent(nameof(Layout))]
    public class SaraUI_FlexPanel:Panel {
        /// <summary>Define el eje principal utilizado para distribuir los controles.</summary>
        public enum FlexDirection {
            /// <summary>Distribuye los controles horizontalmente.</summary>
            Row,

            /// <summary>Distribuye los controles verticalmente.</summary>
            Column
        }

        /// <summary>Define cómo se reparte el espacio disponible sobre el eje principal.</summary>
        public enum JustifyContent {
            /// <summary>Coloca los controles al inicio del eje principal.</summary>
            Start,

            /// <summary>Centra el conjunto de controles.</summary>
            Center,

            /// <summary>Coloca los controles al final del eje principal.</summary>
            End,

            /// <summary>Reparte el espacio adicional únicamente entre los controles.</summary>
            SpaceBetween,

            /// <summary>Reparte espacio alrededor de cada control.</summary>
            SpaceAround,

            /// <summary>Reparte espacios iguales entre controles y extremos.</summary>
            SpaceEvenly
        }

        /// <summary>Define si el contenido puede continuar en una nueva fila o columna.</summary>
        public enum FlexWrap {
            /// <summary>Mantiene todos los controles sobre una sola línea.</summary>
            NoWrap,

            /// <summary>Crea líneas adicionales cuando el contenido supera el espacio disponible.</summary>
            Wrap
        }

        /// <summary>Define la alineación de los controles sobre el eje transversal.</summary>
        public enum FlexAlignment {
            /// <summary>Conserva la alineación histórica: inicio en filas y centro en columnas.</summary>
            Auto,

            /// <summary>Coloca los controles al inicio del eje transversal.</summary>
            Start,

            /// <summary>Centra los controles sobre el eje transversal.</summary>
            Center,

            /// <summary>Coloca los controles al final del eje transversal.</summary>
            End
        }

        private readonly SaraAnimator _layoutAnimator;
        private readonly List<LayoutTransition> _activeTransitions = new List<LayoutTransition>();
        private FlexDirection _direction = FlexDirection.Row;
        private JustifyContent _justify = JustifyContent.Start;
        private FlexWrap _wrap = FlexWrap.NoWrap;
        private FlexAlignment _alignItems = FlexAlignment.Auto;
        private int _childSpacing = 10;
        private int _borderRadius;
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

        /// <summary>Inicializa un panel flexible con doble búfer.</summary>
        public SaraUI_FlexPanel() {
            _layoutAnimator = new SaraAnimator();
            _layoutAnimator.Completed += LayoutAnimator_Completed;
            _layoutAnimator.Canceled += LayoutAnimator_Canceled;
            _layoutAnimator.StateChanged += LayoutAnimator_StateChanged;

            DoubleBuffered = true;
            Size = new Size(300, 200);
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

        /// <summary>Se produce cuando cambia el estado del motor de reorganización.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationStateChanged;

        /// <summary>Obtiene o establece la orientación del eje principal.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar una orientación desconocida.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(FlexDirection.Row)]
        public FlexDirection Direction {
            get => _direction;
            set {
                EnsureDefined(value, nameof(Direction));

                if(_direction == value) {
                    return;
                }

                _direction = value;
                RequestFlexLayout();
            }
        }

        /// <summary>Obtiene o establece la distribución sobre el eje principal.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar una distribución desconocida.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(JustifyContent.Start)]
        public JustifyContent Justify {
            get => _justify;
            set {
                EnsureDefined(value, nameof(Justify));

                if(_justify == value) {
                    return;
                }

                _justify = value;
                RequestFlexLayout();
            }
        }

        /// <summary>Obtiene o establece si los controles pueden continuar en líneas adicionales.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un modo desconocido.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(FlexWrap.NoWrap)]
        public FlexWrap WrapContents {
            get => _wrap;
            set {
                EnsureDefined(value, nameof(WrapContents));

                if(_wrap == value) {
                    return;
                }

                _wrap = value;
                RequestFlexLayout();
            }
        }

        /// <summary>Obtiene o establece la alineación sobre el eje transversal.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar una alineación desconocida.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(FlexAlignment.Auto)]
        public FlexAlignment AlignItems {
            get => _alignItems;
            set {
                EnsureDefined(value, nameof(AlignItems));

                if(_alignItems == value) {
                    return;
                }

                _alignItems = value;
                RequestFlexLayout();
            }
        }

        /// <summary>Obtiene o establece la separación mínima entre controles y líneas.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(10)]
        public int ChildSpacing {
            get => _childSpacing;
            set {
                EnsureNonNegative(value, nameof(ChildSpacing));

                if(_childSpacing == value) {
                    return;
                }

                _childSpacing = value;
                RequestFlexLayout();
            }
        }

        /// <summary>Obtiene o establece el radio aplicado a la región del panel.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(0)]
        public int BorderRadius {
            get => _borderRadius;
            set {
                EnsureNonNegative(value, nameof(BorderRadius));

                if(_borderRadius == value) {
                    return;
                }

                _borderRadius = value;
                _managedRegionRadius = -1;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece si las reorganizaciones deben animarse.</summary>
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

                RequestFlexLayout();
            }
        }

        /// <summary>Obtiene o establece la duración de la reorganización, en milisegundos.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(300)]
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

        /// <summary>Obtiene o establece la curva aplicada a la reorganización.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar una curva desconocida.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(SaraEasing.EaseInOutCubic)]
        public SaraEasing AnimationEasing {
            get => _animationEasing;
            set {
                if(!Enum.IsDefined(typeof(SaraEasing), value)) {
                    throw new ArgumentOutOfRangeException(
                        nameof(AnimationEasing),
                        value,
                        "La curva indicada no es compatible.");
                }

                if(_animationEasing == value) {
                    return;
                }

                _animationEasing = value;
                RestartActiveAnimation();
            }
        }

        /// <summary>Obtiene el estado actual de la reorganización animada.</summary>
        [Browsable(false)]
        public SaraAnimationState AnimationState => _layoutAnimator.State;

        /// <summary>
        /// Obtiene si el panel debe recortar su superficie exterior mediante
        /// <see cref="BorderRadius"/>.
        /// </summary>
        /// <remarks>Los controles derivados que dibujan fuera de su superficie pueden desactivarlo.</remarks>
        [Browsable(false)]
        protected virtual bool ApplyRoundedControlRegion => true;

        /// <summary>Pausa la reorganización activa conservando las posiciones actuales.</summary>
        /// <returns><see langword="true"/> si la animación cambió al estado pausado.</returns>
        public bool PauseAnimation() {
            return _layoutAnimator.Pause();
        }

        /// <summary>Reanuda una reorganización pausada.</summary>
        /// <returns><see langword="true"/> si la animación volvió a ejecutarse.</returns>
        public bool ResumeAnimation() {
            return _layoutAnimator.Resume();
        }

        /// <summary>Detiene la animación y aplica inmediatamente el diseño calculado.</summary>
        /// <returns><see langword="true"/> si existía una animación activa.</returns>
        public bool StopAnimation() {
            bool stopped = StopAnimatorIfActive();
            ApplyActiveDestinations();
            _activeTransitions.Clear();
            return stopped;
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
            RequestFlexLayout();
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e) {
            UpdateRoundedRegion();
            base.OnPaint(e);
        }

        /// <inheritdoc/>
        protected override void OnHandleDestroyed(EventArgs e) {
            StopAnimatorIfActive();
            _activeTransitions.Clear();
            _hasAppliedLayout = false;
            base.OnHandleDestroyed(e);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            if(disposing && !_disposingResources) {
                _disposingResources = true;
                _layoutAnimator.Completed -= LayoutAnimator_Completed;
                _layoutAnimator.Canceled -= LayoutAnimator_Canceled;
                _layoutAnimator.StateChanged -= LayoutAnimator_StateChanged;
                _layoutAnimator.Dispose();
                _activeTransitions.Clear();

                ReleaseManagedRegion();

                AnimationCompleted = null;
                AnimationCanceled = null;
                AnimationStateChanged = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>Crea la geometría redondeada utilizada por este panel y sus derivados.</summary>
        /// <param name="rect">Límites exteriores de la figura.</param>
        /// <param name="radius">Radio solicitado para las esquinas.</param>
        /// <returns>Una ruta nueva cuya liberación corresponde al llamador.</returns>
        protected GraphicsPath GetFigurePath(Rectangle rect, int radius) {
            GraphicsPath path = new GraphicsPath();

            if(rect.Width <= 0 || rect.Height <= 0) {
                return path;
            }

            int safeRadius = Math.Min(
                Math.Max(0, radius),
                Math.Min(rect.Width, rect.Height) / 2);

            if(safeRadius == 0) {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = safeRadius * 2;
            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));
            path.StartFigure();
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void RequestFlexLayout() {
            if(_disposingResources || IsDisposed || _applyingLayout) {
                return;
            }

            PerformLayout();
            Invalidate();
        }

        private List<LayoutTarget> CalculateLayoutTargets() {
            bool rowDirection = _direction == FlexDirection.Row;
            Rectangle contentBounds = GetFlexContentBounds();
            int availableMain = rowDirection
                ? contentBounds.Width
                : contentBounds.Height;
            List<FlexItem> items = CreateFlexItems(rowDirection);
            List<FlexLine> lines = CreateFlexLines(items, availableMain);
            return rowDirection
                ? CreateRowTargets(lines, contentBounds)
                : CreateColumnTargets(lines, contentBounds);
        }

        private Rectangle GetFlexContentBounds() {
            int left = Math.Min(ClientSize.Width, Padding.Left);
            int top = Math.Min(ClientSize.Height, Padding.Top);
            int right = Math.Max(left, ClientSize.Width - Padding.Right);
            int bottom = Math.Max(top, ClientSize.Height - Padding.Bottom);

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

        private List<FlexItem> CreateFlexItems(bool rowDirection) {
            List<FlexItem> items = new List<FlexItem>();

            foreach(Control control in Controls) {
                if(!control.Visible || control.Dock != DockStyle.None) {
                    continue;
                }

                Padding margin = control.Margin;
                items.Add(new FlexItem(
                    control,
                    rowDirection ? control.Width : control.Height,
                    rowDirection ? control.Height : control.Width,
                    rowDirection ? margin.Left : margin.Top,
                    rowDirection ? margin.Right : margin.Bottom,
                    rowDirection ? margin.Top : margin.Left,
                    rowDirection ? margin.Bottom : margin.Right));
            }

            return items;
        }

        private List<FlexLine> CreateFlexLines(List<FlexItem> items, int availableMain) {
            List<FlexLine> lines = new List<FlexLine>();
            FlexLine currentLine = new FlexLine();

            foreach(FlexItem item in items) {
                long requiredMain = currentLine.Items.Count == 0
                    ? item.OuterMain
                    : currentLine.UsedMain + _childSpacing + item.OuterMain;

                if(_wrap == FlexWrap.Wrap && currentLine.Items.Count > 0 &&
                    requiredMain > availableMain) {
                    lines.Add(currentLine);
                    currentLine = new FlexLine();
                }

                currentLine.Add(item, _childSpacing);
            }

            if(currentLine.Items.Count > 0) {
                lines.Add(currentLine);
            }

            return lines;
        }

        private List<LayoutTarget> CreateRowTargets(
            List<FlexLine> lines,
            Rectangle contentBounds) {
            List<LayoutTarget> targets = new List<LayoutTarget>();
            double lineTop = contentBounds.Top;
            bool rightToLeft = RightToLeft == RightToLeft.Yes;

            foreach(FlexLine line in lines) {
                ResolveMainDistribution(
                    line,
                    contentBounds.Width,
                    out double offset,
                    out double gap);
                double cursor = rightToLeft
                    ? contentBounds.Right - offset
                    : contentBounds.Left + offset;

                foreach(FlexItem item in line.Items) {
                    double x;

                    if(rightToLeft) {
                        x = cursor - item.MainAfter - item.MainSize;
                        cursor -= item.OuterMain + gap;
                    } else {
                        x = cursor + item.MainBefore;
                        cursor += item.OuterMain + gap;
                    }

                    double y = ResolveRowCrossPosition(item, lineTop, line.CrossSize);
                    targets.Add(CreateTarget(item.Control, x, y));
                }

                lineTop += line.CrossSize + _childSpacing;
            }

            return targets;
        }

        private List<LayoutTarget> CreateColumnTargets(
            List<FlexLine> lines,
            Rectangle contentBounds) {
            List<LayoutTarget> targets = new List<LayoutTarget>();
            bool rightToLeft = RightToLeft == RightToLeft.Yes;
            double lineEdge = rightToLeft
                ? contentBounds.Right
                : contentBounds.Left;

            foreach(FlexLine line in lines) {
                double lineLeft = rightToLeft ? lineEdge - line.CrossSize : lineEdge;
                ResolveMainDistribution(
                    line,
                    contentBounds.Height,
                    out double offset,
                    out double gap);
                double cursor = contentBounds.Top + offset;

                foreach(FlexItem item in line.Items) {
                    double x = ResolveColumnCrossPosition(
                        item,
                        lineLeft,
                        line.CrossSize,
                        rightToLeft);
                    double y = cursor + item.MainBefore;
                    targets.Add(CreateTarget(item.Control, x, y));
                    cursor += item.OuterMain + gap;
                }

                lineEdge += rightToLeft
                    ? -(line.CrossSize + _childSpacing)
                    : line.CrossSize + _childSpacing;
            }

            return targets;
        }

        private void ResolveMainDistribution(
            FlexLine line,
            int availableMain,
            out double offset,
            out double gap) {
            double remaining = Math.Max(0d, availableMain - line.UsedMain);
            offset = 0d;
            gap = _childSpacing;

            switch(_justify) {
                case JustifyContent.Center:
                offset = remaining / 2d;
                break;

                case JustifyContent.End:
                offset = remaining;
                break;

                case JustifyContent.SpaceBetween:
                if(line.Items.Count > 1) {
                    gap += remaining / (line.Items.Count - 1d);
                }
                break;

                case JustifyContent.SpaceAround:
                double around = remaining / line.Items.Count;
                offset = around / 2d;
                gap += around;
                break;

                case JustifyContent.SpaceEvenly:
                double evenly = remaining / (line.Items.Count + 1d);
                offset = evenly;
                gap += evenly;
                break;
            }
        }

        private double ResolveRowCrossPosition(FlexItem item, double lineTop, long lineCrossSize) {
            FlexAlignment alignment = ResolveEffectiveAlignment();

            if(alignment == FlexAlignment.Center) {
                return lineTop + item.CrossBefore +
                    ((lineCrossSize - item.OuterCross) / 2d);
            }

            if(alignment == FlexAlignment.End) {
                return lineTop + lineCrossSize - item.CrossAfter - item.CrossSize;
            }

            return lineTop + item.CrossBefore;
        }

        private double ResolveColumnCrossPosition(
            FlexItem item,
            double lineLeft,
            long lineCrossSize,
            bool rightToLeft) {
            FlexAlignment alignment = ResolveEffectiveAlignment();

            if(alignment == FlexAlignment.Center) {
                return lineLeft + item.CrossBefore +
                    ((lineCrossSize - item.OuterCross) / 2d);
            }

            bool placeAtRight = alignment == FlexAlignment.Start
                ? rightToLeft
                : !rightToLeft;

            if(placeAtRight) {
                return lineLeft + lineCrossSize - item.CrossAfter - item.CrossSize;
            }

            return lineLeft + item.CrossBefore;
        }

        private FlexAlignment ResolveEffectiveAlignment() {
            if(_alignItems != FlexAlignment.Auto) {
                return _alignItems;
            }

            return _direction == FlexDirection.Row
                ? FlexAlignment.Start
                : FlexAlignment.Center;
        }

        private void ApplyCalculatedLayout(List<LayoutTarget> targets) {
            if(TargetsMatchActiveAnimation(targets)) {
                return;
            }

            bool animate = CanAnimateLayout();
            List<LayoutTransition> transitions = CreateTransitions(targets);

            if(!animate || transitions.Count == 0) {
                StopAnimatorIfActive();
                _activeTransitions.Clear();
                ApplyTargetsImmediately(targets);
                return;
            }

            StartAnimation(transitions);
        }

        private List<LayoutTransition> CreateTransitions(List<LayoutTarget> targets) {
            List<LayoutTransition> transitions = new List<LayoutTransition>();

            foreach(LayoutTarget target in targets) {
                Rectangle origin = target.Control.Bounds;

                if(origin != target.Destination) {
                    transitions.Add(new LayoutTransition(
                        target.Control,
                        origin,
                        target.Destination));
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
                CreateAnimationOptions());
        }

        private void RestartActiveAnimation() {
            if(!_layoutAnimator.IsRunning && !_layoutAnimator.IsPaused) {
                return;
            }

            List<LayoutTransition> transitions = new List<LayoutTransition>();

            foreach(LayoutTransition active in _activeTransitions) {
                if(IsManagedControl(active.Control) &&
                    active.Control.Bounds != active.Destination) {
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
                LayoutTransition? transition = FindTransition(
                    _activeTransitions,
                    target.Control);

                if(transition == null && target.Control.Bounds != target.Destination) {
                    return false;
                }
            }

            return true;
        }

        private static LayoutTarget? FindTarget(
            List<LayoutTarget> targets,
            Control control) {
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
                    if(!IsManagedControl(transition.Control)) {
                        continue;
                    }

                    transition.Control.Bounds = InterpolateRectangle(
                        transition.Origin,
                        transition.Destination,
                        progress);
                }
            } finally {
                _applyingLayout = false;
            }
        }

        private void ApplyTargetsImmediately(List<LayoutTarget> targets) {
            _applyingLayout = true;

            try {
                foreach(LayoutTarget target in targets) {
                    if(IsManagedControl(target.Control) &&
                        target.Control.Bounds != target.Destination) {
                        target.Control.Bounds = target.Destination;
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
            if(!ApplyRoundedControlRegion) {
                ReleaseManagedRegion();
                return;
            }

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
                using GraphicsPath path = GetFigurePath(bounds, radius);
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

        private SaraAnimationOptions CreateAnimationOptions() {
            return new SaraAnimationOptions {
                Duration = _animationDuration,
                FrameInterval = _animationFrameInterval,
                Easing = _animationEasing
            };
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

        private static LayoutTarget CreateTarget(Control control, double x, double y) {
            return new LayoutTarget(
                control,
                new Rectangle(
                    ToInt32(x),
                    ToInt32(y),
                    control.Width,
                    control.Height));
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

        private static void EnsureNonNegative(int value, string propertyName) {
            if(value < 0) {
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    value,
                    "El valor no puede ser negativo.");
            }
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

        private sealed class FlexItem {
            public FlexItem(
                Control control,
                int mainSize,
                int crossSize,
                int mainBefore,
                int mainAfter,
                int crossBefore,
                int crossAfter) {
                Control = control;
                MainSize = Math.Max(0, mainSize);
                CrossSize = Math.Max(0, crossSize);
                MainBefore = Math.Max(0, mainBefore);
                MainAfter = Math.Max(0, mainAfter);
                CrossBefore = Math.Max(0, crossBefore);
                CrossAfter = Math.Max(0, crossAfter);
            }

            public Control Control { get; }

            public int MainSize { get; }

            public int CrossSize { get; }

            public int MainBefore { get; }

            public int MainAfter { get; }

            public int CrossBefore { get; }

            public int CrossAfter { get; }

            public long OuterMain => (long)MainBefore + MainSize + MainAfter;

            public long OuterCross => (long)CrossBefore + CrossSize + CrossAfter;
        }

        private sealed class FlexLine {
            public List<FlexItem> Items { get; } = new List<FlexItem>();

            public long UsedMain { get; private set; }

            public long CrossSize { get; private set; }

            public void Add(FlexItem item, int spacing) {
                if(Items.Count > 0) {
                    UsedMain += spacing;
                }

                Items.Add(item);
                UsedMain += item.OuterMain;
                CrossSize = Math.Max(CrossSize, item.OuterCross);
            }
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Sara_UI_Design.Animations;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Representa un panel lateral capaz de expandirse y contraerse mediante animaciones temporales.
    /// </summary>
    /// <remarks>
    /// El control puede utilizarse con <see cref="DockStyle.Left"/> o <see cref="DockStyle.Right"/>.
    /// Cuando <see cref="AutoHideButtonText"/> está habilitado, el texto de los botones
    /// <see cref="SaraUI_Button"/> se conserva internamente mientras la barra está contraída.
    /// </remarks>
    [ToolboxItem(true)]
    [DefaultEvent(nameof(IsExpandedChanged))]
    [DefaultProperty(nameof(IsExpanded))]
    public class SaraUI_SideBar:Panel {
        private readonly SaraAnimator _animator = new SaraAnimator();
        private readonly Dictionary<SaraUI_Button, string> _buttonTexts =
            new Dictionary<SaraUI_Button, string>();
        private int _expandedWidth = 250;
        private int _collapsedWidth = 60;
        private bool _isExpanded = true;
        private bool _animationTargetExpanded = true;
        private int _animationDuration = 300;
        private int _animationFrameInterval = 15;
        private SaraEasing _animationEasing = SaraEasing.EaseInOutCubic;
        private bool _animationEnabled = true;
        private bool _autoHideButtonText = true;
        private bool _synchronizingButtonText;
        private bool _disposingResources;

        /// <summary>
        /// Inicializa una nueva instancia de la barra lateral.
        /// </summary>
        public SaraUI_SideBar() {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            _animator.Completed += Animator_Completed;
            _animator.Canceled += Animator_Canceled;
            _animator.StateChanged += Animator_StateChanged;

            BackColor = Color.FromArgb(45, 45, 65);
            Dock = DockStyle.Left;
            Size = new Size(_expandedWidth, 200);
        }

        /// <summary>
        /// Se produce cuando cambia el destino lógico entre expandido y contraído.
        /// </summary>
        [Category("Sara UI Design")]
        public event EventHandler? IsExpandedChanged;

        /// <summary>
        /// Se produce antes de comenzar la expansión.
        /// </summary>
        [Category("Sara UI Design")]
        public event EventHandler? Expanding;

        /// <summary>
        /// Se produce cuando la barra alcanza su ancho expandido.
        /// </summary>
        [Category("Sara UI Design")]
        public event EventHandler? Expanded;

        /// <summary>
        /// Se produce antes de comenzar la contracción.
        /// </summary>
        [Category("Sara UI Design")]
        public event EventHandler? Collapsing;

        /// <summary>
        /// Se produce cuando la barra alcanza su ancho contraído.
        /// </summary>
        [Category("Sara UI Design")]
        public event EventHandler? Collapsed;

        /// <summary>
        /// Se produce cuando una animación activa se detiene o se reemplaza.
        /// </summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationCanceled;

        /// <summary>
        /// Se produce cuando cambia el estado de la animación interna.
        /// </summary>
        [Category("Sara UI Design")]
        public event EventHandler? AnimationStateChanged;

        /// <summary>
        /// Obtiene o establece el ancho final de la barra expandida.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce cuando el valor no es mayor que <see cref="CollapsedWidth"/>.
        /// </exception>
        [Category("Sara UI Design")]
        [DefaultValue(250)]
        public int ExpandedWidth {
            get => _expandedWidth;
            set {
                if(value <= _collapsedWidth) {
                    throw new ArgumentOutOfRangeException(
                        nameof(ExpandedWidth),
                        value,
                        "El ancho expandido debe ser mayor que el ancho contraído.");
                }

                if(_expandedWidth == value) {
                    return;
                }

                _expandedWidth = value;
                ApplyConfiguredWidthWhenIdle(expanded: true);
            }
        }

        /// <summary>
        /// Obtiene o establece el ancho final de la barra contraída.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce cuando el valor es negativo o no es menor que <see cref="ExpandedWidth"/>.
        /// </exception>
        [Category("Sara UI Design")]
        [DefaultValue(60)]
        public int CollapsedWidth {
            get => _collapsedWidth;
            set {
                if(value < 0 || value >= _expandedWidth) {
                    throw new ArgumentOutOfRangeException(
                        nameof(CollapsedWidth),
                        value,
                        "El ancho contraído no puede ser negativo y debe ser menor que el ancho expandido.");
                }

                if(_collapsedWidth == value) {
                    return;
                }

                _collapsedWidth = value;
                ApplyConfiguredWidthWhenIdle(expanded: false);
            }
        }

        /// <summary>
        /// Obtiene o establece si el destino lógico de la barra es el estado expandido.
        /// </summary>
        /// <remarks>
        /// Durante una transición, esta propiedad representa el destino solicitado. Para conocer
        /// el avance temporal debe consultarse <see cref="AnimationState"/>.
        /// </remarks>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool IsExpanded {
            get => _isExpanded;
            set => SetExpanded(value, animate: true);
        }

        /// <summary>
        /// Obtiene o establece la duración de la expansión o contracción, expresada en milisegundos.
        /// Un valor de cero aplica inmediatamente el ancho final.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(300)]
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

                _animationFrameInterval = value;
            }
        }

        /// <summary>
        /// Obtiene o establece la curva utilizada durante la expansión y la contracción.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce al asignar un valor que no representa una curva compatible.
        /// </exception>
        [Category("Sara UI Design")]
        [DefaultValue(SaraEasing.EaseInOutCubic)]
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

        /// <summary>
        /// Obtiene o establece si los cambios de estado deben interpolarse.
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

                if(!value) {
                    bool mustComplete = IsAnimationActive || Width != GetTargetWidth(_isExpanded);
                    _animator.Stop();
                    CompleteVisualState(_isExpanded, mustComplete);
                }
            }
        }

        /// <summary>
        /// Obtiene o establece si el texto de los botones Sara UI debe ocultarse al contraer la barra.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool AutoHideButtonText {
            get => _autoHideButtonText;
            set {
                if(_autoHideButtonText == value) {
                    return;
                }

                _autoHideButtonText = value;

                if(value && !_isExpanded) {
                    HideButtonTexts(this);
                } else if(!value) {
                    RestoreButtonTexts();
                }
            }
        }

        /// <summary>
        /// Obtiene el estado actual de la animación interna.
        /// </summary>
        [Browsable(false)]
        public SaraAnimationState AnimationState => _animator.State;

        /// <summary>
        /// Obtiene un valor que indica si la barra está avanzando hacia su destino.
        /// </summary>
        [Browsable(false)]
        public bool IsAnimating => _animator.IsRunning;

        /// <summary>
        /// Expande la barra utilizando la configuración de animación actual.
        /// </summary>
        public void Expand() {
            SetExpanded(expanded: true, animate: true);
        }

        /// <summary>
        /// Contrae la barra utilizando la configuración de animación actual.
        /// </summary>
        public void Collapse() {
            SetExpanded(expanded: false, animate: true);
        }

        /// <summary>
        /// Cambia entre los estados expandido y contraído.
        /// </summary>
        public void Toggle() {
            SetExpanded(!_isExpanded, animate: true);
        }

        /// <summary>
        /// Establece el destino de la barra y permite decidir si el cambio debe animarse.
        /// </summary>
        /// <param name="expanded"><see langword="true"/> para expandir; <see langword="false"/> para contraer.</param>
        /// <param name="animate"><see langword="true"/> para utilizar el motor de animaciones.</param>
        /// <exception cref="ObjectDisposedException">Se produce cuando el control ya fue liberado.</exception>
        /// <exception cref="InvalidOperationException">
        /// Se produce cuando el método se invoca desde un hilo diferente al de la interfaz.
        /// </exception>
        public void SetExpanded(bool expanded, bool animate) {
            ThrowIfDisposed();
            EnsureUiThread();

            bool stateChanged = _isExpanded != expanded;
            bool active = IsAnimationActive;

            if(!stateChanged && active && _animationTargetExpanded == expanded && animate) {
                return;
            }

            if(!stateChanged && !active && Width == GetTargetWidth(expanded)) {
                SynchronizeButtonText(expanded);
                return;
            }

            if(active) {
                _animator.Stop();
            }

            _isExpanded = expanded;
            _animationTargetExpanded = expanded;

            if(stateChanged) {
                IsExpandedChanged?.Invoke(this, EventArgs.Empty);
            }

            if(expanded) {
                Expanding?.Invoke(this, EventArgs.Empty);
            } else {
                HideButtonTexts(this);
                Collapsing?.Invoke(this, EventArgs.Empty);
            }

            int targetWidth = GetTargetWidth(expanded);
            bool shouldAnimate = animate &&
                _animationEnabled &&
                _animationDuration > 0 &&
                !IsInDesignMode &&
                Width != targetWidth;

            if(!shouldAnimate) {
                CompleteVisualState(expanded, raiseCompletionEvent: true);
                return;
            }

            int originWidth = Width;
            _animator.Start(
                originWidth,
                targetWidth,
                value => {
                    int nextWidth = (int)Math.Round(value);

                    if(Width != nextWidth) {
                        Width = nextWidth;
                    }
                },
                new SaraAnimationOptions {
                    Duration = _animationDuration,
                    Easing = _animationEasing,
                    FrameInterval = _animationFrameInterval
                });
        }

        /// <summary>
        /// Pausa la transición activa conservando el ancho alcanzado.
        /// </summary>
        /// <returns><see langword="true"/> si la transición cambió al estado pausado.</returns>
        public bool PauseAnimation() {
            ThrowIfDisposed();
            EnsureUiThread();
            return _animator.Pause();
        }

        /// <summary>
        /// Reanuda una transición pausada desde el ancho conservado.
        /// </summary>
        /// <returns><see langword="true"/> si la transición volvió a ejecutarse.</returns>
        public bool ResumeAnimation() {
            ThrowIfDisposed();
            EnsureUiThread();
            return _animator.Resume();
        }

        /// <summary>
        /// Detiene la transición activa y conserva el ancho alcanzado.
        /// </summary>
        /// <returns><see langword="true"/> si se canceló una transición activa.</returns>
        public bool StopAnimation() {
            ThrowIfDisposed();
            EnsureUiThread();
            return _animator.Stop();
        }

        /// <inheritdoc/>
        protected override void OnControlAdded(ControlEventArgs e) {
            base.OnControlAdded(e);

            if(!_isExpanded && _autoHideButtonText && e.Control is not null) {
                HideButtonTexts(e.Control);
            }
        }

        /// <inheritdoc/>
        protected override void OnControlRemoved(ControlEventArgs e) {
            if(e.Control is not null) {
                RemoveButtonBackups(e.Control);
            }

            base.OnControlRemoved(e);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            if(disposing && !_disposingResources) {
                _disposingResources = true;

                _animator.Completed -= Animator_Completed;
                _animator.Canceled -= Animator_Canceled;
                _animator.StateChanged -= Animator_StateChanged;
                _animator.Dispose();

                foreach(SaraUI_Button button in _buttonTexts.Keys) {
                    button.TextChanged -= Button_TextChanged;
                    button.Disposed -= Button_Disposed;
                }

                _buttonTexts.Clear();

                IsExpandedChanged = null;
                Expanding = null;
                Expanded = null;
                Collapsing = null;
                Collapsed = null;
                AnimationCanceled = null;
                AnimationStateChanged = null;
            }

            base.Dispose(disposing);
        }

        private bool IsAnimationActive => _animator.IsRunning || _animator.IsPaused;

        private bool IsInDesignMode =>
            DesignMode ||
            LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
            (Site?.DesignMode ?? false);

        private int GetTargetWidth(bool expanded) {
            return expanded ? _expandedWidth : _collapsedWidth;
        }

        private void ApplyConfiguredWidthWhenIdle(bool expanded) {
            if(_isExpanded == expanded && !IsAnimationActive) {
                Width = GetTargetWidth(expanded);
            }
        }

        private void CompleteVisualState(bool expanded, bool raiseCompletionEvent) {
            Width = GetTargetWidth(expanded);
            SynchronizeButtonText(expanded);

            if(!raiseCompletionEvent) {
                return;
            }

            if(expanded) {
                Expanded?.Invoke(this, EventArgs.Empty);
            } else {
                Collapsed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SynchronizeButtonText(bool expanded) {
            if(expanded || !_autoHideButtonText) {
                RestoreButtonTexts();
            } else {
                HideButtonTexts(this);
            }
        }

        private void HideButtonTexts(Control root) {
            if(!_autoHideButtonText) {
                return;
            }

            if(root is SaraUI_Button button) {
                RememberAndHideButtonText(button);
            }

            foreach(Control child in root.Controls) {
                HideButtonTexts(child);
            }
        }

        private void RestoreButtonTexts() {
            foreach(KeyValuePair<SaraUI_Button, string> item in _buttonTexts) {
                SaraUI_Button button = item.Key;
                button.TextChanged -= Button_TextChanged;
                button.Disposed -= Button_Disposed;

                if(!button.IsDisposed && IsDescendant(button)) {
                    _synchronizingButtonText = true;

                    try {
                        button.Text = item.Value;
                    } finally {
                        _synchronizingButtonText = false;
                    }
                }
            }

            _buttonTexts.Clear();
        }

        private void RememberAndHideButtonText(SaraUI_Button button) {
            if(button.IsDisposed) {
                return;
            }

            if(!_buttonTexts.ContainsKey(button)) {
                button.TextChanged += Button_TextChanged;
                button.Disposed += Button_Disposed;
                _buttonTexts.Add(button, button.Text);
            }

            if(string.IsNullOrEmpty(button.Text)) {
                return;
            }

            _buttonTexts[button] = button.Text;
            _synchronizingButtonText = true;

            try {
                button.Text = string.Empty;
            } finally {
                _synchronizingButtonText = false;
            }
        }

        private void RemoveButtonBackups(Control root) {
            if(root is SaraUI_Button button && _buttonTexts.Remove(button)) {
                button.TextChanged -= Button_TextChanged;
                button.Disposed -= Button_Disposed;
            }

            foreach(Control child in root.Controls) {
                RemoveButtonBackups(child);
            }
        }

        private bool IsDescendant(Control control) {
            Control? parent = control.Parent;

            while(parent is not null) {
                if(ReferenceEquals(parent, this)) {
                    return true;
                }

                parent = parent.Parent;
            }

            return false;
        }

        private void Animator_Completed(object? sender, EventArgs e) {
            CompleteVisualState(_animationTargetExpanded, raiseCompletionEvent: true);
        }

        private void Animator_Canceled(object? sender, EventArgs e) {
            AnimationCanceled?.Invoke(this, EventArgs.Empty);
        }

        private void Animator_StateChanged(object? sender, EventArgs e) {
            AnimationStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Button_TextChanged(object? sender, EventArgs e) {
            if(_synchronizingButtonText ||
                _isExpanded ||
                !_autoHideButtonText ||
                sender is not SaraUI_Button button) {
                return;
            }

            if(string.IsNullOrEmpty(button.Text)) {
                _buttonTexts[button] = string.Empty;
                return;
            }

            RememberAndHideButtonText(button);
        }

        private void Button_Disposed(object? sender, EventArgs e) {
            if(sender is SaraUI_Button button) {
                button.TextChanged -= Button_TextChanged;
                button.Disposed -= Button_Disposed;
                _buttonTexts.Remove(button);
            }
        }

        private void EnsureUiThread() {
            if(InvokeRequired) {
                throw new InvalidOperationException(
                    "La barra lateral debe controlarse desde el hilo que creó su interfaz.");
            }
        }

        private void ThrowIfDisposed() {
            if(_disposingResources || IsDisposed || Disposing) {
                throw new ObjectDisposedException(nameof(SaraUI_SideBar));
            }
        }
    }
}
using System;
using System.ComponentModel;
using System.Diagnostics;
using FormsTimer = System.Windows.Forms.Timer;

namespace Sara_UI_Design.Animations {
    /// <summary>
    /// Interpola valores numéricos en el hilo de interfaz de Windows Forms.
    /// </summary>
    /// <remarks>
    /// Las operaciones de esta clase deben iniciarse y controlarse desde el mismo hilo que creó el componente.
    /// El temporizador programa las actualizaciones y un <see cref="Stopwatch"/> determina el progreso real.
    /// </remarks>
    public sealed class SaraAnimator:Component {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly FormsTimer _timer;
        private Action<float>? _updateValue;
        private float _from;
        private float _to;
        private float _currentValue;
        private int _duration;
        private bool _repeat;
        private bool _autoReverse;
        private bool _isReversed;
        private SaraEasing _easing;
        private SaraAnimationState _state;
        private long _animationVersion;
        private bool _disposed;

        /// <summary>
        /// Inicializa una nueva instancia del motor de animaciones.
        /// </summary>
        public SaraAnimator() {
            _timer = new FormsTimer();
            _timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// Inicializa una nueva instancia y la agrega al contenedor de componentes indicado.
        /// </summary>
        /// <param name="container">Contenedor que administrará el ciclo de vida del motor.</param>
        /// <exception cref="ArgumentNullException">
        /// Se produce cuando <paramref name="container"/> es <see langword="null"/>.
        /// </exception>
        public SaraAnimator(IContainer container):this() {
            if(container is null) {
                throw new ArgumentNullException(nameof(container));
            }

            container.Add(this);
        }

        /// <summary>
        /// Se produce cuando la animación finaliza todos sus recorridos.
        /// </summary>
        public event EventHandler? Completed;

        /// <summary>
        /// Se produce cuando una animación activa se cancela o se reemplaza por otra.
        /// </summary>
        public event EventHandler? Canceled;

        /// <summary>
        /// Se produce cuando cambia el estado de la animación.
        /// </summary>
        public event EventHandler? StateChanged;

        /// <summary>
        /// Obtiene el estado actual del motor.
        /// </summary>
        [Browsable(false)]
        public SaraAnimationState State => _state;

        /// <summary>
        /// Obtiene un valor que indica si la animación está avanzando.
        /// </summary>
        [Browsable(false)]
        public bool IsRunning => _state == SaraAnimationState.Running;

        /// <summary>
        /// Obtiene un valor que indica si la animación está pausada.
        /// </summary>
        [Browsable(false)]
        public bool IsPaused => _state == SaraAnimationState.Paused;

        /// <summary>
        /// Obtiene el último valor entregado por la animación.
        /// </summary>
        [Browsable(false)]
        public float CurrentValue => _currentValue;

        /// <summary>
        /// Inicia una animación entre dos valores.
        /// </summary>
        /// <param name="from">Valor inicial.</param>
        /// <param name="to">Valor final.</param>
        /// <param name="updateValue">Acción que recibe cada valor interpolado.</param>
        /// <param name="options">Configuración de la animación. Si se omite, se utilizan los valores predeterminados.</param>
        /// <exception cref="ArgumentNullException">
        /// Se produce cuando <paramref name="updateValue"/> es <see langword="null"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Se produce cuando el motor ya fue liberado.
        /// </exception>
        public void Start(
            float from,
            float to,
            Action<float> updateValue,
            SaraAnimationOptions? options = null) {
            ThrowIfDisposed();

            if(updateValue is null) {
                throw new ArgumentNullException(nameof(updateValue));
            }

            if(IsRunning || IsPaused) {
                StopInternal(raiseCanceled: true);
            }

            SaraAnimationOptions animationOptions = options ?? new SaraAnimationOptions();

            _animationVersion++;
            _from = from;
            _to = to;
            _currentValue = from;
            _duration = animationOptions.Duration;
            _repeat = animationOptions.Repeat;
            _autoReverse = animationOptions.AutoReverse;
            _isReversed = false;
            _easing = animationOptions.Easing;
            _updateValue = updateValue;
            _timer.Interval = animationOptions.FrameInterval;
            _stopwatch.Reset();
            long animationVersion = _animationVersion;
            SetState(SaraAnimationState.Running);

            if(animationVersion != _animationVersion || !IsRunning) {
                return;
            }

            ApplyValue(from);

            if(animationVersion != _animationVersion || !IsRunning) {
                return;
            }

            if(_duration == 0) {
                ApplyValue(to);

                if(animationVersion == _animationVersion && IsRunning) {
                    CompleteAnimation();
                }

                return;
            }

            _stopwatch.Start();
            _timer.Start();
        }

        /// <summary>
        /// Pausa la animación conservando el progreso alcanzado.
        /// </summary>
        /// <returns><see langword="true"/> si la animación cambió al estado pausado.</returns>
        /// <exception cref="ObjectDisposedException">Se produce cuando el motor ya fue liberado.</exception>
        public bool Pause() {
            ThrowIfDisposed();

            if(!IsRunning) {
                return false;
            }

            _timer.Stop();
            _stopwatch.Stop();
            SetState(SaraAnimationState.Paused);
            return true;
        }

        /// <summary>
        /// Reanuda una animación pausada desde el progreso conservado.
        /// </summary>
        /// <returns><see langword="true"/> si la animación volvió a ejecutarse.</returns>
        /// <exception cref="ObjectDisposedException">Se produce cuando el motor ya fue liberado.</exception>
        public bool Resume() {
            ThrowIfDisposed();

            if(!IsPaused) {
                return false;
            }

            _stopwatch.Start();
            _timer.Start();
            SetState(SaraAnimationState.Running);
            return true;
        }

        /// <summary>
        /// Cancela la animación activa y conserva el último valor entregado.
        /// </summary>
        /// <returns><see langword="true"/> si se canceló una animación en ejecución o pausada.</returns>
        /// <exception cref="ObjectDisposedException">Se produce cuando el motor ya fue liberado.</exception>
        public bool Stop() {
            ThrowIfDisposed();

            if(!IsRunning && !IsPaused) {
                return false;
            }

            StopInternal(raiseCanceled: true);
            return true;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            if(disposing && !_disposed) {
                _disposed = true;
                _animationVersion++;
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
                _timer.Dispose();
                _stopwatch.Stop();
                _updateValue = null;
                _state = SaraAnimationState.Stopped;
                Completed = null;
                Canceled = null;
                StateChanged = null;
            }

            base.Dispose(disposing);
        }

        private void Timer_Tick(object? sender, EventArgs e) {
            if(!IsRunning || _updateValue is null) {
                return;
            }

            long animationVersion = _animationVersion;
            double rawProgress = _stopwatch.Elapsed.TotalMilliseconds / _duration;
            float progress = rawProgress >= 1d ? 1f : (float)rawProgress;
            float directedProgress = _isReversed ? 1f - progress : progress;
            float easedProgress = SaraEasingFunctions.Evaluate(_easing, directedProgress);
            float value = _from + ((_to - _from) * easedProgress);

            ApplyValue(value);

            if(animationVersion != _animationVersion || !IsRunning || rawProgress < 1d) {
                return;
            }

            CompleteCurrentCycle(animationVersion);
        }

        private void CompleteCurrentCycle(long animationVersion) {
            if(_autoReverse && !_isReversed) {
                _isReversed = true;
                _stopwatch.Restart();
                return;
            }

            if(_repeat) {
                _isReversed = false;
                _stopwatch.Restart();

                if(!_autoReverse) {
                    ApplyValue(_from);
                }

                return;
            }

            if(animationVersion == _animationVersion) {
                CompleteAnimation();
            }
        }

        private void CompleteAnimation() {
            _timer.Stop();
            _stopwatch.Stop();
            _updateValue = null;
            SetState(SaraAnimationState.Completed);
            Completed?.Invoke(this, EventArgs.Empty);
        }

        private void ApplyValue(float value) {
            long animationVersion = _animationVersion;
            Action<float>? updateValue = _updateValue;
            _currentValue = value;

            try {
                updateValue?.Invoke(value);
            } catch {
                if(animationVersion == _animationVersion) {
                    StopInternal(raiseCanceled: false);
                }

                throw;
            }
        }

        private void StopInternal(bool raiseCanceled) {
            bool wasActive = IsRunning || IsPaused;

            _animationVersion++;
            _timer.Stop();
            _stopwatch.Reset();
            _updateValue = null;
            _isReversed = false;
            SetState(SaraAnimationState.Stopped);

            if(wasActive && raiseCanceled) {
                Canceled?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetState(SaraAnimationState state) {
            if(_state == state) {
                return;
            }

            _state = state;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ThrowIfDisposed() {
            if(_disposed) {
                throw new ObjectDisposedException(nameof(SaraAnimator));
            }
        }
    }
}

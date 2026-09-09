using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sara_UI_Design.Animations {
    /// <summary>
    /// Aplica transiciones animadas a las propiedades visuales de un control de Windows Forms.
    /// </summary>
    /// <remarks>
    /// Cada instancia administra una transición a la vez. Al iniciar una nueva transición,
    /// la animación anterior se cancela. Todos los métodos deben utilizarse desde el hilo de interfaz.
    /// </remarks>
    [ToolboxItem(true)]
    [DefaultProperty(nameof(Target))]
    public sealed class SaraControlTransitions:Component {
        private readonly SaraAnimator _animator = new SaraAnimator();
        private Control? _target;
        private bool _disposed;

        /// <summary>
        /// Inicializa una nueva instancia del componente de transiciones.
        /// </summary>
        public SaraControlTransitions() {
            _animator.Completed += Animator_Completed;
            _animator.Canceled += Animator_Canceled;
            _animator.StateChanged += Animator_StateChanged;
        }

        /// <summary>
        /// Inicializa una nueva instancia y la agrega al contenedor indicado.
        /// </summary>
        /// <param name="container">Contenedor que administrará el ciclo de vida del componente.</param>
        /// <exception cref="ArgumentNullException">
        /// Se produce cuando <paramref name="container"/> es <see langword="null"/>.
        /// </exception>
        public SaraControlTransitions(IContainer container):this() {
            if(container is null) {
                throw new ArgumentNullException(nameof(container));
            }

            container.Add(this);
        }

        /// <summary>
        /// Se produce cuando cambia el control asociado al componente.
        /// </summary>
        public event EventHandler? TargetChanged;

        /// <summary>
        /// Se produce cuando una transición llega correctamente a su destino.
        /// </summary>
        public event EventHandler? Completed;

        /// <summary>
        /// Se produce cuando una transición activa se detiene o es reemplazada.
        /// </summary>
        public event EventHandler? Canceled;

        /// <summary>
        /// Se produce cuando cambia el estado de la transición.
        /// </summary>
        public event EventHandler? StateChanged;

        /// <summary>
        /// Obtiene o establece el control al que se aplicarán las transiciones.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Se produce cuando el componente ya fue liberado.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Se produce al asignar un control que ya fue liberado.
        /// </exception>
        [Category("Sara UI Design")]
        [DefaultValue(null)]
        public Control? Target {
            get => _target;
            set {
                ThrowIfDisposed();

                if(ReferenceEquals(_target, value)) {
                    return;
                }

                if(value?.IsDisposed == true) {
                    throw new ArgumentException(
                        "No se puede animar un control que ya fue liberado.",
                        nameof(Target));
                }

                _animator.Stop();
                DetachTarget();
                _target = value;

                if(_target is not null) {
                    _target.Disposed += Target_Disposed;
                }

                TargetChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Obtiene el estado actual de la transición.
        /// </summary>
        [Browsable(false)]
        public SaraAnimationState State => _animator.State;

        /// <summary>
        /// Obtiene un valor que indica si existe una transición en ejecución.
        /// </summary>
        [Browsable(false)]
        public bool IsRunning => _animator.IsRunning;

        /// <summary>
        /// Obtiene un valor que indica si la transición está pausada.
        /// </summary>
        [Browsable(false)]
        public bool IsPaused => _animator.IsPaused;

        /// <summary>
        /// Anima la posición del control hasta el punto indicado.
        /// </summary>
        /// <param name="destination">Posición final relativa al contenedor padre.</param>
        /// <param name="options">Configuración temporal de la transición.</param>
        /// <exception cref="InvalidOperationException">
        /// Se produce si no existe un control asociado o si el control utiliza <see cref="DockStyle"/>.
        /// </exception>
        public void MoveTo(Point destination, SaraAnimationOptions? options = null) {
            Control target = GetTarget();
            EnsureFreeLayout(target, "la posición");
            Point origin = target.Location;

            StartTransition(
                target,
                progress => target.Location = new Point(
                    Interpolate(origin.X, destination.X, progress),
                    Interpolate(origin.Y, destination.Y, progress)),
                options);
        }

        /// <summary>
        /// Anima el tamaño del control hasta las dimensiones indicadas.
        /// </summary>
        /// <param name="destination">Tamaño final del control.</param>
        /// <param name="options">Configuración temporal de la transición.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce si alguna dimensión del destino es negativa.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Se produce si no existe un control asociado, utiliza <see cref="DockStyle"/> o tiene tamaño automático.
        /// </exception>
        public void ResizeTo(Size destination, SaraAnimationOptions? options = null) {
            ValidateSize(destination, nameof(destination));

            Control target = GetTarget();
            EnsureFreeLayout(target, "el tamaño");
            EnsureManualSize(target);
            Size origin = target.Size;

            StartTransition(
                target,
                progress => target.Size = new Size(
                    Interpolate(origin.Width, destination.Width, progress),
                    Interpolate(origin.Height, destination.Height, progress)),
                options);
        }

        /// <summary>
        /// Anima simultáneamente la posición y el tamaño del control.
        /// </summary>
        /// <param name="destination">Rectángulo final relativo al contenedor padre.</param>
        /// <param name="options">Configuración temporal de la transición.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce si el ancho o el alto del destino son negativos.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Se produce si no existe un control asociado, utiliza <see cref="DockStyle"/> o tiene tamaño automático.
        /// </exception>
        public void ChangeBounds(Rectangle destination, SaraAnimationOptions? options = null) {
            ValidateSize(destination.Size, nameof(destination));

            Control target = GetTarget();
            EnsureFreeLayout(target, "los límites");
            EnsureManualSize(target);
            Rectangle origin = target.Bounds;

            StartTransition(
                target,
                progress => target.Bounds = new Rectangle(
                    Interpolate(origin.X, destination.X, progress),
                    Interpolate(origin.Y, destination.Y, progress),
                    Interpolate(origin.Width, destination.Width, progress),
                    Interpolate(origin.Height, destination.Height, progress)),
                options);
        }

        /// <summary>
        /// Anima el color de fondo del control.
        /// </summary>
        /// <param name="destination">Color final.</param>
        /// <param name="options">Configuración temporal de la transición.</param>
        /// <remarks>
        /// Algunos controles no admiten colores con transparencia. En esos casos deben utilizarse colores opacos.
        /// </remarks>
        public void ChangeBackColor(Color destination, SaraAnimationOptions? options = null) {
            Control target = GetTarget();
            Color origin = target.BackColor;

            StartTransition(
                target,
                progress => target.BackColor = Interpolate(origin, destination, progress),
                options);
        }

        /// <summary>
        /// Anima el color del contenido frontal del control.
        /// </summary>
        /// <param name="destination">Color final.</param>
        /// <param name="options">Configuración temporal de la transición.</param>
        public void ChangeForeColor(Color destination, SaraAnimationOptions? options = null) {
            Control target = GetTarget();
            Color origin = target.ForeColor;

            StartTransition(
                target,
                progress => target.ForeColor = Interpolate(origin, destination, progress),
                options);
        }

        /// <summary>
        /// Anima la opacidad de un formulario.
        /// </summary>
        /// <param name="destination">Opacidad final entre cero y uno.</param>
        /// <param name="options">Configuración temporal de la transición.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce cuando la opacidad no está comprendida entre cero y uno.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Se produce si el control asociado no es un <see cref="Form"/>.
        /// </exception>
        public void FadeTo(double destination, SaraAnimationOptions? options = null) {
            if(double.IsNaN(destination) ||
                double.IsInfinity(destination) ||
                destination < 0d ||
                destination > 1d) {
                throw new ArgumentOutOfRangeException(
                    nameof(destination),
                    destination,
                    "La opacidad debe estar comprendida entre cero y uno.");
            }

            Control target = GetTarget();

            if(target is not Form form) {
                throw new InvalidOperationException(
                    "La transición de opacidad solo puede aplicarse a un formulario.");
            }

            double origin = form.Opacity;

            StartTransition(
                form,
                progress => form.Opacity = origin + ((destination - origin) * progress),
                options);
        }

        /// <summary>
        /// Pausa la transición activa conservando su progreso.
        /// </summary>
        /// <returns><see langword="true"/> si la transición cambió al estado pausado.</returns>
        public bool Pause() {
            ThrowIfDisposed();
            return _animator.Pause();
        }

        /// <summary>
        /// Reanuda una transición pausada.
        /// </summary>
        /// <returns><see langword="true"/> si la transición volvió a ejecutarse.</returns>
        public bool Resume() {
            ThrowIfDisposed();
            return _animator.Resume();
        }

        /// <summary>
        /// Cancela la transición activa conservando el último valor aplicado.
        /// </summary>
        /// <returns><see langword="true"/> si se canceló una transición activa.</returns>
        public bool Stop() {
            ThrowIfDisposed();
            return _animator.Stop();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            if(disposing && !_disposed) {
                _disposed = true;
                DetachTarget();

                _animator.Completed -= Animator_Completed;
                _animator.Canceled -= Animator_Canceled;
                _animator.StateChanged -= Animator_StateChanged;
                _animator.Dispose();

                TargetChanged = null;
                Completed = null;
                Canceled = null;
                StateChanged = null;
            }

            base.Dispose(disposing);
        }

        private void StartTransition(
            Control target,
            Action<float> update,
            SaraAnimationOptions? options) {
            if(target.InvokeRequired) {
                throw new InvalidOperationException(
                    "Las transiciones deben iniciarse desde el hilo que creó el control.");
            }

            _animator.Start(
                0f,
                1f,
                progress => {
                    if(target.IsDisposed || target.Disposing) {
                        _animator.Stop();
                        return;
                    }

                    update(progress);
                },
                options);
        }

        private Control GetTarget() {
            ThrowIfDisposed();

            if(_target is null) {
                throw new InvalidOperationException(
                    "Debe asignar un control a la propiedad Target antes de iniciar una transición.");
            }

            if(_target.IsDisposed || _target.Disposing) {
                throw new InvalidOperationException(
                    "El control asociado no está disponible para realizar la transición.");
            }

            return _target;
        }

        private static void EnsureFreeLayout(Control target, string propertyName) {
            if(target.Dock != DockStyle.None) {
                throw new InvalidOperationException(
                    $"No se puede animar {propertyName} mientras el control utiliza Dock. " +
                    "Establezca DockStyle.None o anime un contenedor libre.");
            }
        }

        private static void EnsureManualSize(Control target) {
            if(target.AutoSize) {
                throw new InvalidOperationException(
                    "No se puede animar el tamaño de un control con AutoSize activado.");
            }
        }

        private static void ValidateSize(Size size, string parameterName) {
            if(size.Width < 0 || size.Height < 0) {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    size,
                    "Las dimensiones del destino no pueden ser negativas.");
            }
        }

        private static int Interpolate(int origin, int destination, float progress) {
            return (int)Math.Round(origin + ((destination - origin) * progress));
        }

        private static Color Interpolate(Color origin, Color destination, float progress) {
            return Color.FromArgb(
                Interpolate(origin.A, destination.A, progress),
                Interpolate(origin.R, destination.R, progress),
                Interpolate(origin.G, destination.G, progress),
                Interpolate(origin.B, destination.B, progress));
        }

        private void DetachTarget() {
            if(_target is not null) {
                _target.Disposed -= Target_Disposed;
                _target = null;
            }
        }

        private void Target_Disposed(object? sender, EventArgs e) {
            DetachTarget();
            _animator.Stop();
            TargetChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Animator_Completed(object? sender, EventArgs e) {
            Completed?.Invoke(this, EventArgs.Empty);
        }

        private void Animator_Canceled(object? sender, EventArgs e) {
            Canceled?.Invoke(this, EventArgs.Empty);
        }

        private void Animator_StateChanged(object? sender, EventArgs e) {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ThrowIfDisposed() {
            if(_disposed) {
                throw new ObjectDisposedException(nameof(SaraControlTransitions));
            }
        }
    }
}

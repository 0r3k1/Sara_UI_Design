using System;
using System.ComponentModel;

namespace Sara_UI_Design.Animations {
    /// <summary>
    /// Configura la duración, la frecuencia y el comportamiento de una animación.
    /// </summary>
    public sealed class SaraAnimationOptions {
        private int _duration = 300;
        private int _frameInterval = 15;
        private SaraEasing _easing = SaraEasing.EaseInOutQuad;

        /// <summary>
        /// Obtiene o establece la duración de cada recorrido, expresada en milisegundos.
        /// Un valor de cero aplica inmediatamente el valor final.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce al asignar un valor menor que cero.
        /// </exception>
        [DefaultValue(300)]
        public int Duration {
            get => _duration;
            set {
                if(value < 0) {
                    throw new ArgumentOutOfRangeException(
                        nameof(Duration),
                        value,
                        "La duración de la animación no puede ser negativa.");
                }

                _duration = value;
            }
        }

        /// <summary>
        /// Obtiene o establece el intervalo solicitado entre actualizaciones, expresado en milisegundos.
        /// El tiempo transcurrido real se calcula con un cronómetro de alta precisión.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce al asignar un valor menor que uno.
        /// </exception>
        [DefaultValue(15)]
        public int FrameInterval {
            get => _frameInterval;
            set {
                if(value < 1) {
                    throw new ArgumentOutOfRangeException(
                        nameof(FrameInterval),
                        value,
                        "El intervalo entre actualizaciones debe ser mayor que cero.");
                }

                _frameInterval = value;
            }
        }

        /// <summary>
        /// Obtiene o establece la curva que transforma el progreso de la animación.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce al asignar un valor que no representa una curva conocida.
        /// </exception>
        [DefaultValue(SaraEasing.EaseInOutQuad)]
        public SaraEasing Easing {
            get => _easing;
            set {
                if(!Enum.IsDefined(typeof(SaraEasing), value)) {
                    throw new ArgumentOutOfRangeException(
                        nameof(Easing),
                        value,
                        "La curva de animación indicada no es compatible.");
                }

                _easing = value;
            }
        }

        /// <summary>
        /// Obtiene o establece si la animación debe comenzar de nuevo al completar un ciclo.
        /// </summary>
        [DefaultValue(false)]
        public bool Repeat { get; set; }

        /// <summary>
        /// Obtiene o establece si la animación debe volver al punto inicial después del recorrido de ida.
        /// Si también se activa <see cref="Repeat"/>, los recorridos de ida y vuelta se repiten indefinidamente.
        /// Una duración de cero aplica inmediatamente el valor final y omite el recorrido de regreso.
        /// </summary>
        [DefaultValue(false)]
        public bool AutoReverse { get; set; }
    }
}

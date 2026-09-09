using System;

namespace Sara_UI_Design.Animations {
    /// <summary>
    /// Proporciona las funciones matemáticas utilizadas por las curvas de animación.
    /// </summary>
    public static class SaraEasingFunctions {
        /// <summary>
        /// Evalúa una curva con un progreso normalizado entre cero y uno.
        /// </summary>
        /// <param name="easing">Curva que se desea evaluar.</param>
        /// <param name="progress">Progreso de la animación. Los valores fuera del intervalo se limitan automáticamente.</param>
        /// <returns>Progreso transformado por la curva seleccionada.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce cuando <paramref name="easing"/> no representa una curva conocida.
        /// </exception>
        public static float Evaluate(SaraEasing easing, float progress) {
            float value = Clamp01(progress);

            switch(easing) {
                case SaraEasing.Linear:
                    return value;
                case SaraEasing.EaseInQuad:
                    return value * value;
                case SaraEasing.EaseOutQuad:
                    return 1f - ((1f - value) * (1f - value));
                case SaraEasing.EaseInOutQuad:
                    return value < 0.5f
                        ? 2f * value * value
                        : 1f - (Square(-2f * value + 2f) / 2f);
                case SaraEasing.EaseInCubic:
                    return value * value * value;
                case SaraEasing.EaseOutCubic:
                    return 1f - Cube(1f - value);
                case SaraEasing.EaseInOutCubic:
                    return value < 0.5f
                        ? 4f * value * value * value
                        : 1f - (Cube(-2f * value + 2f) / 2f);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(easing),
                        easing,
                        "La curva de animación indicada no es compatible.");
            }
        }

        private static float Clamp01(float value) {
            if(value < 0f) {
                return 0f;
            }

            return value > 1f ? 1f : value;
        }

        private static float Square(float value) => value * value;

        private static float Cube(float value) => value * value * value;
    }
}

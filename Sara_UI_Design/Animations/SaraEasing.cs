namespace Sara_UI_Design.Animations {
    /// <summary>
    /// Define las curvas de aceleración incluidas en el motor de animaciones.
    /// </summary>
    public enum SaraEasing {
        /// <summary>
        /// Mantiene una velocidad constante durante toda la animación.
        /// </summary>
        Linear,

        /// <summary>
        /// Comienza lentamente y acelera con una curva cuadrática.
        /// </summary>
        EaseInQuad,

        /// <summary>
        /// Comienza rápidamente y desacelera con una curva cuadrática.
        /// </summary>
        EaseOutQuad,

        /// <summary>
        /// Acelera y desacelera con una curva cuadrática.
        /// </summary>
        EaseInOutQuad,

        /// <summary>
        /// Comienza lentamente y acelera con una curva cúbica.
        /// </summary>
        EaseInCubic,

        /// <summary>
        /// Comienza rápidamente y desacelera con una curva cúbica.
        /// </summary>
        EaseOutCubic,

        /// <summary>
        /// Acelera y desacelera con una curva cúbica.
        /// </summary>
        EaseInOutCubic
    }
}

namespace Sara_UI_Design.Animations {
    /// <summary>
    /// Indica el estado actual de una animación administrada por <see cref="SaraAnimator"/>.
    /// </summary>
    public enum SaraAnimationState {
        /// <summary>
        /// La animación no se ha iniciado o fue cancelada.
        /// </summary>
        Stopped,

        /// <summary>
        /// La animación está avanzando y actualizando su valor.
        /// </summary>
        Running,

        /// <summary>
        /// La animación conserva su progreso, pero no está avanzando.
        /// </summary>
        Paused,

        /// <summary>
        /// La animación llegó correctamente a su destino.
        /// </summary>
        Completed
    }
}

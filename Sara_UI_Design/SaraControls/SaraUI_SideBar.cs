using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Panel de navegación lateral con soporte para animaciones de expansión y colapso. 
    /// Gestiona automáticamente el ancho del control y el texto de los botones hijos para un diseño responsivo.
    /// </summary>
    public class SaraUI_SideBar:Panel {
        // Campos de configuración
        private int expandedWidth = 250;
        private int collapsedWidth = 60;
        private bool isExpanded = true;
        private int animationSpeed = 15;
        private System.Windows.Forms.Timer animationTimer;

        // Propiedades para el Diseñador

        /// <summary>
        /// Obtiene o establece el ancho del panel cuando se encuentra en estado expandido (abierto).
        /// </summary>
        [Category("Sara UI Design")]
        public int ExpandedWidth { get => expandedWidth; set => expandedWidth = value; }

        /// <summary>
        /// Obtiene o establece el ancho del panel cuando se encuentra en estado colapsado (cerrado).
        /// </summary>
        [Category("Sara UI Design")]
        public int CollapsedWidth { get => collapsedWidth; set => collapsedWidth = value; }

        /// <summary>
        /// Obtiene o establece el estado visual del panel. 
        /// Al cambiar este valor en tiempo de ejecución, se dispara automáticamente la animación de transición.
        /// </summary>
        [Category("Sara UI Design")]
        public bool IsExpanded {
            get => isExpanded;
            set {
                isExpanded = value;
                if(!this.DesignMode)
                    animationTimer.Start();
                else
                    this.Width = value ? expandedWidth : collapsedWidth;
            }
        }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_SideBar"/>, configurando el acoplamiento lateral, 
        /// el doble búfer para evitar parpadeos y el temporizador de animación.
        /// </summary>
        public SaraUI_SideBar() {
            this.Width = expandedWidth;
            this.Dock = DockStyle.Left;
            this.BackColor = Color.FromArgb(45, 45, 65); // Color oscuro por defecto
            this.DoubleBuffered = true;

            // Configurar el Timer para la animación
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 1; // Máxima fluidez
            animationTimer.Tick += AnimationTimer_Tick;
        }

        /// <summary>
        /// Controlador del temporizador que gestiona el cambio progresivo del ancho del panel (Interpolación lineal) 
        /// y dispara el cambio de visibilidad de los textos internos.
        /// </summary>
        private void AnimationTimer_Tick(object sender, EventArgs e) {
            if(isExpanded) {
                // Expandiendo
                if(this.Width < expandedWidth) {
                    this.Width += animationSpeed;
                } else {
                    this.Width = expandedWidth;
                    animationTimer.Stop();
                    ToggleChildText(true);
                }
            } else {
                // Colapsando
                if(this.Width > collapsedWidth) {
                    this.Width -= animationSpeed;
                    ToggleChildText(false); // Ocultar texto de inmediato al colapsar
                } else {
                    this.Width = collapsedWidth;
                    animationTimer.Stop();
                }
            }
        }

        /// <summary>
        /// Método auxiliar que recorre los controles hijos (específicamente <see cref="SaraUI_Button"/>) 
        /// para ocultar o restaurar sus textos, utilizando la propiedad Tag como almacenamiento temporal.
        /// </summary>
        /// <param name="show">Indica si los textos deben mostrarse (true) u ocultarse (false).</param>
        private void ToggleChildText(bool show) {
            foreach(Control ctrl in this.Controls) {
                if(ctrl is SaraUI_Button btn) {
                    // Si el botón está colapsado, guardamos el texto en el Tag para no perderlo
                    if(!show && !string.IsNullOrEmpty(btn.Text)) {
                        btn.Tag = btn.Text;
                        btn.Text = "";
                    } else if(show && btn.Tag != null) {
                        btn.Text = btn.Tag.ToString();
                    }
                }
            }
        }
    }
}
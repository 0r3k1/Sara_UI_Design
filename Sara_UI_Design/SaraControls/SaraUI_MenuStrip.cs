using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Barra de menús personalizada de la suite Sara UI. 
    /// Utiliza un renderizador profesional para aplicar estilos modernos de selección y tipografía.
    /// </summary>
    [ToolboxItem(true)]
    public class SaraUI_MenuStrip:MenuStrip {
        private Color primaryColor = Color.MediumSlateBlue;
        private Color menuItemTextColor = Color.DimGray;


        /// <summary>
        /// Obtiene o establece el color de resalte utilizado para los elementos seleccionados y activos.
        /// </summary>
        [Category("Sara UI Design")]
        public Color PrimaryColor {
            get => primaryColor;
            set { primaryColor = value; UpdateRenderer(); }
        }

        /// <summary>
        /// Obtiene o establece el color de la fuente para los elementos del menú en su estado normal.
        /// </summary>
        [Category("Sara UI Design")]
        public Color MenuItemTextColor {
            get => menuItemTextColor;
            set { menuItemTextColor = value; UpdateRenderer(); }
        }

        /// <summary>
        /// Actualiza el renderizador cuando el color de fondo del control cambia para asegurar 
        /// que la tabla de colores se mantenga sincronizada.
        /// </summary>
        protected override void OnBackColorChanged(EventArgs e) {
            base.OnBackColorChanged(e);
            UpdateRenderer();
        }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_MenuStrip"/> definiendo la fuente 
        /// predeterminada y activando el renderizador personalizado.
        /// </summary>
        public SaraUI_MenuStrip() {
            this.Font = new Font("Segoe UI", 9.5F);
            UpdateRenderer();
        }

        /// <summary>
        /// Crea y asigna una nueva instancia del renderizador <see cref="SaraUI_MenuRenderer"/> 
        /// basándose en las propiedades de color actuales.
        /// </summary>
        private void UpdateRenderer() {
            this.Renderer = new SaraUI_MenuRenderer(true, primaryColor, menuItemTextColor, this.BackColor);
        }
    }
}
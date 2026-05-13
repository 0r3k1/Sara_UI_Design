using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Representa un menú contextual personalizado de la suite Sara UI que aplica un diseño moderno 
    /// mediante un renderizador personalizado y control dinámico del tamaño de los elementos.
    /// </summary>
    [ToolboxItem(true)]
    public class SaraUI_DropdownMenu:ContextMenuStrip {
        // Campos

        /// <summary>
        /// Obtiene o establece un valor que indica si el menú se comporta como un menú principal (estilo barra superior) 
        /// o como un submenú desplegable.
        /// </summary>
        private bool isMainMenu;
        private int menuItemHeight = 35;
        private Color menuItemTextColor = Color.Empty;

        /// <summary>
        /// Obtiene o establece el color primario utilizado para resaltar elementos seleccionados y bordes en el menú.
        /// </summary>
        private Color primaryColor = Color.MediumSlateBlue;

        // Propiedades
        [Category("Sara UI Design")]
        public bool IsMainMenu {
            get => isMainMenu;
            set => isMainMenu = value;
        }

        [Category("Sara UI Design")]
        public Color PrimaryColor {
            get => primaryColor;
            set { primaryColor = value; this.Invalidate(); }
        }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_DropdownMenu"/> asociándola a un contenedor 
        /// y aplicando el renderizador visual de Sara UI.
        /// </summary>
        /// <param name="container">El contenedor que albergará este componente.</param>
        public SaraUI_DropdownMenu(IContainer container) : base(container) {
            this.Renderer = new SaraUI_MenuRenderer(isMainMenu, primaryColor, menuItemTextColor, this.BackColor);
        }

        /// <summary>
        /// Se ejecuta al abrirse el menú para actualizar el renderizador con los colores actuales 
        /// y ajustar uniformemente la altura de todos los elementos (<see cref="ToolStripMenuItem"/>).
        /// </summary>
        /// <param name="e">Argumentos del evento de cancelación.</param>
        protected override void OnOpening(CancelEventArgs e) {
            base.OnOpening(e);
            this.Renderer = new SaraUI_MenuRenderer(isMainMenu, primaryColor, menuItemTextColor, this.BackColor);

            foreach(ToolStripItem item in this.Items) {
                if(item is ToolStripMenuItem menuItem) {
                    menuItem.AutoSize = false;
                    menuItem.Height = menuItemHeight;
                }
            }
        }
    }
}
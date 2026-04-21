using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {
    [ToolboxItem(true)]
    public class SaraUI_DropdownMenu:ContextMenuStrip {
        // Campos
        private bool isMainMenu;
        private int menuItemHeight = 35;
        private Color menuItemTextColor = Color.Empty;
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

        public SaraUI_DropdownMenu(IContainer container) : base(container) {
            this.Renderer = new SaraUI_MenuRenderer(isMainMenu, primaryColor, menuItemTextColor, this.BackColor);
        }

        // Sobrescribimos para que el menú siempre use nuestro diseño al abrirse
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
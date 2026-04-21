using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {
    [ToolboxItem(true)]
    public class SaraUI_MenuStrip:MenuStrip {
        private Color primaryColor = Color.MediumSlateBlue;
        private Color menuItemTextColor = Color.DimGray;

        [Category("Sara UI Design")]
        public Color PrimaryColor {
            get => primaryColor;
            set { primaryColor = value; UpdateRenderer(); }
        }

        [Category("Sara UI Design")]
        public Color MenuItemTextColor {
            get => menuItemTextColor;
            set { menuItemTextColor = value; UpdateRenderer(); }
        }

        protected override void OnBackColorChanged(EventArgs e) {
            base.OnBackColorChanged(e);
            UpdateRenderer();
        }

        public SaraUI_MenuStrip() {
            this.Font = new Font("Segoe UI", 9.5F);
            UpdateRenderer();
        }

        private void UpdateRenderer() {
            this.Renderer = new SaraUI_MenuRenderer(true, primaryColor, menuItemTextColor, this.BackColor);
        }
    }
}
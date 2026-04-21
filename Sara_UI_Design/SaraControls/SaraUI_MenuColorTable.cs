using System.Drawing;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {
    public class SaraUI_MenuColorTable:ProfessionalColorTable {
        private Color backColor;
        private Color leftColumnColor;
        private Color borderColor;
        private Color menuItemBorderColor;
        private Color menuItemSelectedColor;
        public SaraUI_MenuColorTable(bool isMainMenu, Color primaryColor, Color customBackColor) {

            if(customBackColor == Color.Empty) {
                backColor = isMainMenu ? Color.FromArgb(37, 39, 60) : Color.White;
            } else {
                backColor = customBackColor;
            }

            leftColumnColor = isMainMenu ? Color.FromArgb(32, 33, 51) : Color.FromArgb(245, 245, 250);
            borderColor = isMainMenu ? Color.FromArgb(32, 33, 51) : Color.FromArgb(230, 230, 240);

            if(isMainMenu) {
                backColor = Color.FromArgb(37, 39, 60);
                leftColumnColor = Color.FromArgb(32, 33, 51);
                borderColor = Color.FromArgb(32, 33, 51);
            } else {
                backColor = Color.White;
                leftColumnColor = Color.FromArgb(245, 245, 250); // Un gris muy suave tipo SaraUI
                borderColor = Color.FromArgb(230, 230, 240);
            }
            menuItemBorderColor = primaryColor;
            menuItemSelectedColor = Color.FromArgb(40, primaryColor); // Selección sutil transparente
        }

        public override Color ToolStripDropDownBackground => backColor;
        public override Color MenuBorder => borderColor;
        public override Color MenuItemBorder => Color.Transparent; // Sin borde duro en selección
        public override Color MenuItemSelected => menuItemSelectedColor;
        public override Color ImageMarginGradientBegin => leftColumnColor;
        public override Color ImageMarginGradientMiddle => leftColumnColor;
        public override Color ImageMarginGradientEnd => leftColumnColor;
    }
}
using System.Drawing;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Define la tabla de colores personalizada para los menús de Sara UI. 
    /// Ajusta automáticamente las tonalidades dependiendo de si el menú es principal o un submenú.
    /// </summary>
    public class SaraUI_MenuColorTable:ProfessionalColorTable {
        private Color backColor;
        private Color leftColumnColor;
        private Color borderColor;
        private Color menuItemBorderColor;
        private Color menuItemSelectedColor;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_MenuColorTable"/> calculando 
        /// los colores de fondo, bordes y márgenes de imagen según el estilo visual de la suite.
        /// </summary>
        public SaraUI_MenuColorTable(bool isMainMenu, Color primaryColor, Color customBackColor) {
            // 1. Establecer el color de fondo base
            if(customBackColor == Color.Empty) {
                backColor = isMainMenu ? Color.FromArgb(37, 39, 60) : Color.White;
            } else {
                backColor = customBackColor;
            }

            // 2. Calcular los colores de contraste de forma inteligente (Evita que se rompa con fondos personalizados)
            // Si el fondo es muy oscuro, usamos tonos oscuros complementarios; si es claro, tonos suaves.
            bool isDark = backColor.GetBrightness() < 0.5f;

            if(isDark) {
                leftColumnColor = Color.FromArgb(Math.Max(0, backColor.R - 5), Math.Max(0, backColor.G - 6), Math.Max(0, backColor.B - 9));
                borderColor = leftColumnColor;
            } else {
                leftColumnColor = Color.FromArgb(245, 245, 250);
                borderColor = Color.FromArgb(230, 230, 240);
            }

            menuItemBorderColor = primaryColor;
            menuItemSelectedColor = Color.FromArgb(40, primaryColor); // Selección sutil transparente
        }

        public override Color ToolStripDropDownBackground => backColor;
        public override Color MenuBorder => borderColor;
        public override Color MenuItemBorder => Color.Transparent;
        public override Color MenuItemSelected => menuItemSelectedColor;
        public override Color ImageMarginGradientBegin => leftColumnColor;
        public override Color ImageMarginGradientMiddle => leftColumnColor;
        public override Color ImageMarginGradientEnd => leftColumnColor;
    }
}
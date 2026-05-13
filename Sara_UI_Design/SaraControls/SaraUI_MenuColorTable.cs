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
        /// <param name="isMainMenu">Indica si se deben aplicar colores de modo oscuro (Main Menu) o modo claro (Submenús).</param>
        /// <param name="primaryColor">Color base para las selecciones y acentos visuales.</param>
        /// <param name="customBackColor">Color de fondo personalizado opcional.</param>
        public SaraUI_MenuColorTable(bool isMainMenu, Color primaryColor, Color customBackColor) {

            if(customBackColor == Color.Empty) {
                backColor = isMainMenu ? Color.FromArgb(37, 39, 60) : Color.White;
            } else {
                backColor = customBackColor;
            }

            leftColumnColor = isMainMenu ? Color.FromArgb(32, 33, 51) : Color.FromArgb(245, 245, 250);
            borderColor = isMainMenu ? Color.FromArgb(32, 33, 51) : Color.FromArgb(230, 230, 240);

            // 1. Definimos los colores base según el tipo de menú
            if(isMainMenu) {
                backColor = Color.FromArgb(37, 39, 60);
                leftColumnColor = Color.FromArgb(32, 33, 51);
                borderColor = Color.FromArgb(32, 33, 51);
            } else {
                backColor = Color.White;
                leftColumnColor = Color.FromArgb(245, 245, 250);
                borderColor = Color.FromArgb(230, 230, 240);
            }

            // 2. Si el usuario mandó un color personalizado
            if(customBackColor != Color.Empty) {
                backColor = customBackColor;
            }

            menuItemBorderColor = primaryColor;
            menuItemSelectedColor = Color.FromArgb(40, primaryColor); // Selección sutil transparente
        }

        /// <summary>
        /// Obtiene el color de fondo principal para el área desplegable del menú.
        /// </summary>
        public override Color ToolStripDropDownBackground => backColor;
        /// <summary>
        /// Obtiene el color del borde exterior que rodea a todo el menú desplegable.
        /// </summary>
        public override Color MenuBorder => borderColor;
        public override Color MenuItemBorder => Color.Transparent; // Sin borde duro en selección
        /// <summary>
        /// Obtiene el color de fondo utilizado cuando un elemento del menú está seleccionado.
        /// </summary>
        public override Color MenuItemSelected => menuItemSelectedColor;
        /// <summary>
        /// Define el color de la columna izquierda donde se muestran normalmente los iconos, 
        /// creando un diseño de banda lateral limpia.
        /// </summary>
        public override Color ImageMarginGradientBegin => leftColumnColor;
        public override Color ImageMarginGradientMiddle => leftColumnColor;
        public override Color ImageMarginGradientEnd => leftColumnColor;
    }
}
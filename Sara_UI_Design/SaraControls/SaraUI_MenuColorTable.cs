using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Proporciona una paleta coherente para barras, menús desplegables,
    /// selecciones, márgenes de imagen, separadores y marcas.
    /// </summary>
    public class SaraUI_MenuColorTable:ProfessionalColorTable {
        private readonly Color _backColor;
        private readonly Color _imageMarginColor;
        private readonly Color _borderColor;
        private readonly Color _selectedColor;
        private readonly Color _pressedColor;
        private readonly Color _separatorColor;

        /// <summary>Inicializa una tabla con la opacidad de selección predeterminada.</summary>
        /// <param name="isMainMenu">Indica si la paleta pertenece a una barra principal.</param>
        /// <param name="primaryColor">Color de énfasis.</param>
        /// <param name="customBackColor">Color de fondo o <see cref="Color.Empty"/> para calcularlo.</param>
        public SaraUI_MenuColorTable(
            bool isMainMenu,
            Color primaryColor,
            Color customBackColor)
            :this(isMainMenu, primaryColor, customBackColor, 36) {
        }

        /// <summary>Inicializa una tabla con una opacidad de selección específica.</summary>
        /// <param name="isMainMenu">Indica si la paleta pertenece a una barra principal.</param>
        /// <param name="primaryColor">Color de énfasis.</param>
        /// <param name="customBackColor">Color de fondo o <see cref="Color.Empty"/> para calcularlo.</param>
        /// <param name="selectionOpacity">Opacidad entre cero y 255.</param>
        /// <exception cref="ArgumentOutOfRangeException">Se produce con una opacidad inválida.</exception>
        public SaraUI_MenuColorTable(
            bool isMainMenu,
            Color primaryColor,
            Color customBackColor,
            int selectionOpacity) {
            if(selectionOpacity < 0 || selectionOpacity > 255) {
                throw new ArgumentOutOfRangeException(
                    nameof(selectionOpacity),
                    selectionOpacity,
                    "La opacidad debe estar entre cero y 255.");
            }

            Color accent = primaryColor.IsEmpty
                ? Color.MediumSlateBlue
                : primaryColor;
            _backColor = customBackColor.IsEmpty
                ? (isMainMenu ? Color.FromArgb(37, 39, 60) : Color.White)
                : customBackColor;

            bool dark = _backColor.GetBrightness() < 0.5f;
            _imageMarginColor = dark
                ? Shift(_backColor, -7)
                : Color.FromArgb(245, 245, 250);
            _borderColor = dark
                ? Shift(_backColor, 18)
                : Color.FromArgb(220, 220, 232);
            _separatorColor = dark
                ? Shift(_backColor, 28)
                : Color.FromArgb(218, 218, 228);
            _selectedColor = Color.FromArgb(selectionOpacity, accent);
            _pressedColor = Color.FromArgb(
                Math.Min(255, selectionOpacity + 28),
                accent);
        }

        /// <inheritdoc/>
        public override Color ToolStripDropDownBackground => _backColor;

        /// <inheritdoc/>
        public override Color MenuBorder => _borderColor;

        /// <inheritdoc/>
        public override Color MenuItemBorder => Color.Transparent;

        /// <inheritdoc/>
        public override Color MenuItemSelected => _selectedColor;

        /// <inheritdoc/>
        public override Color MenuItemSelectedGradientBegin => _selectedColor;

        /// <inheritdoc/>
        public override Color MenuItemSelectedGradientEnd => _selectedColor;

        /// <inheritdoc/>
        public override Color MenuItemPressedGradientBegin => _pressedColor;

        /// <inheritdoc/>
        public override Color MenuItemPressedGradientMiddle => _pressedColor;

        /// <inheritdoc/>
        public override Color MenuItemPressedGradientEnd => _pressedColor;

        /// <inheritdoc/>
        public override Color ImageMarginGradientBegin => _imageMarginColor;

        /// <inheritdoc/>
        public override Color ImageMarginGradientMiddle => _imageMarginColor;

        /// <inheritdoc/>
        public override Color ImageMarginGradientEnd => _imageMarginColor;

        /// <inheritdoc/>
        public override Color SeparatorDark => _separatorColor;

        /// <inheritdoc/>
        public override Color SeparatorLight => Color.Transparent;

        /// <inheritdoc/>
        public override Color CheckBackground => _selectedColor;

        /// <inheritdoc/>
        public override Color CheckSelectedBackground => _selectedColor;

        /// <inheritdoc/>
        public override Color CheckPressedBackground => _pressedColor;

        private static Color Shift(Color color, int amount) {
            return Color.FromArgb(
                color.A,
                ClampByte(color.R + amount),
                ClampByte(color.G + amount),
                ClampByte(color.B + amount));
        }

        private static int ClampByte(int value) {
            return Math.Max(0, Math.Min(255, value));
        }
    }
}

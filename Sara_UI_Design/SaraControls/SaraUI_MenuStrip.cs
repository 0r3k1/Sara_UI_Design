using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Barra de menús que aplica una paleta coherente a sus elementos y a toda
    /// la jerarquía de submenús sin alterar sus datos durante el pintado.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultProperty(nameof(PrimaryColor))]
    public class SaraUI_MenuStrip:MenuStrip {
        private Color _primaryColor = Color.MediumSlateBlue;
        private Color _menuItemTextColor = Color.DimGray;
        private Color _disabledTextColor = Color.Empty;
        private int _selectionOpacity = 36;
        private int _selectionCornerRadius = 6;
        private int _dropDownItemHeight = 35;
        private bool _applyUniformDropDownItemHeight = true;
        private bool _scaleItemHeightWithDpi = true;
        private bool _showDropDownImageMargin = true;
        private bool _initialized;

        /// <summary>Inicializa una barra que respeta la fuente heredada del formulario.</summary>
        public SaraUI_MenuStrip() {
            _initialized = true;
            UpdateRenderer();
        }

        /// <summary>Se produce después de reconstruir y propagar el tema.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? ThemeChanged;

        /// <summary>Obtiene o establece el color de selección y elementos activos.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "MediumSlateBlue")]
        public Color PrimaryColor {
            get => _primaryColor;
            set {
                if(_primaryColor == value) {
                    return;
                }

                _primaryColor = value;
                UpdateRenderer();
            }
        }

        /// <summary>
        /// Obtiene o establece el color normal del texto. <see cref="Color.Empty"/>
        /// utiliza un color calculado según el tipo de menú.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "DimGray")]
        public Color MenuItemTextColor {
            get => _menuItemTextColor;
            set {
                if(_menuItemTextColor == value) {
                    return;
                }

                _menuItemTextColor = value;
                UpdateRenderer();
            }
        }

        /// <summary>Obtiene o establece el color del texto deshabilitado.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color DisabledTextColor {
            get => _disabledTextColor;
            set {
                if(_disabledTextColor == value) {
                    return;
                }

                _disabledTextColor = value;
                UpdateRenderer();
            }
        }

        /// <summary>Obtiene o establece la opacidad del fondo seleccionado.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce fuera del intervalo de 0 a 255.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(36)]
        public int SelectionOpacity {
            get => _selectionOpacity;
            set {
                ValidateByte(value, nameof(SelectionOpacity));

                if(_selectionOpacity == value) {
                    return;
                }

                _selectionOpacity = value;
                UpdateRenderer();
            }
        }

        /// <summary>Obtiene o establece el radio de las selecciones.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce con valores negativos.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(6)]
        public int SelectionCornerRadius {
            get => _selectionCornerRadius;
            set {
                ValidateNonNegative(value, nameof(SelectionCornerRadius));

                if(_selectionCornerRadius == value) {
                    return;
                }

                _selectionCornerRadius = value;
                UpdateRenderer();
            }
        }

        /// <summary>Obtiene o establece la altura lógica de los elementos desplegables.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce con valores menores que uno.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(35)]
        public int DropDownItemHeight {
            get => _dropDownItemHeight;
            set {
                ValidatePositive(value, nameof(DropDownItemHeight));

                if(_dropDownItemHeight == value) {
                    return;
                }

                _dropDownItemHeight = value;
                ApplyThemeToItems(Items);
            }
        }

        /// <summary>Obtiene o establece si se fuerza una altura uniforme en los submenús.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool ApplyUniformDropDownItemHeight {
            get => _applyUniformDropDownItemHeight;
            set {
                if(_applyUniformDropDownItemHeight == value) {
                    return;
                }

                _applyUniformDropDownItemHeight = value;
                ApplyThemeToItems(Items);
            }
        }

        /// <summary>Obtiene o establece si la altura lógica se adapta al DPI actual.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool ScaleItemHeightWithDpi {
            get => _scaleItemHeightWithDpi;
            set {
                if(_scaleItemHeightWithDpi == value) {
                    return;
                }

                _scaleItemHeightWithDpi = value;
                ApplyThemeToItems(Items);
            }
        }

        /// <summary>Obtiene o establece si los submenús muestran el margen reservado para imágenes.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool ShowDropDownImageMargin {
            get => _showDropDownImageMargin;
            set {
                if(_showDropDownImageMargin == value) {
                    return;
                }

                _showDropDownImageMargin = value;
                UpdateRenderer();
            }
        }

        /// <summary>Reconstruye y vuelve a propagar explícitamente el tema actual.</summary>
        public void RefreshTheme() {
            UpdateRenderer();
        }

        /// <inheritdoc/>
        protected override void OnBackColorChanged(EventArgs e) {
            base.OnBackColorChanged(e);

            if(_initialized) {
                UpdateRenderer();
            }
        }

        /// <inheritdoc/>
        protected override void OnItemAdded(ToolStripItemEventArgs e) {
            base.OnItemAdded(e);

            ToolStripItem? item = e.Item;

            if(_initialized && item != null) {
                ApplyThemeToItem(item);
            }
        }

        /// <inheritdoc/>
        protected override void OnMenuActivate(EventArgs e) {
            ApplyThemeToItems(Items);
            base.OnMenuActivate(e);
        }

        private void UpdateRenderer() {
            if(!_initialized || IsDisposed) {
                return;
            }

            SaraUI_MenuRenderer renderer = new SaraUI_MenuRenderer(
                true,
                _primaryColor,
                _menuItemTextColor,
                _disabledTextColor,
                BackColor,
                _selectionCornerRadius,
                _selectionOpacity,
                _showDropDownImageMargin);
            Renderer = renderer;
            ApplyThemeToItems(Items);
            Invalidate(true);
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ApplyThemeToItems(ToolStripItemCollection items) {
            foreach(ToolStripItem item in items) {
                ApplyThemeToItem(item);
            }
        }

        private void ApplyThemeToItem(ToolStripItem item) {
            if(item is not ToolStripMenuItem menuItem) {
                return;
            }

            menuItem.DropDown.Renderer = Renderer;

            if(menuItem.DropDown is ToolStripDropDownMenu dropDownMenu) {
                dropDownMenu.ShowImageMargin = _showDropDownImageMargin;
            }

            ApplyHeight(menuItem.DropDownItems);

            ApplyThemeToItems(menuItem.DropDownItems);
        }

        private void ApplyHeight(ToolStripItemCollection items) {
            int height = GetScaledItemHeight();

            foreach(ToolStripItem item in items) {
                if(item is ToolStripMenuItem) {
                    if(!_applyUniformDropDownItemHeight) {
                        item.AutoSize = true;
                        continue;
                    }

                    Size preferred = item.GetPreferredSize(Size.Empty);
                    item.AutoSize = false;
                    item.Size = new Size(Math.Max(item.Width, preferred.Width), height);
                }
            }
        }

        private int GetScaledItemHeight() {
            if(!_scaleItemHeightWithDpi || !IsHandleCreated) {
                return _dropDownItemHeight;
            }

            return Math.Max(
                1,
                (int)Math.Round(_dropDownItemHeight * (DeviceDpi / 96f)));
        }

        private static void ValidateByte(int value, string propertyName) {
            if(value < 0 || value > 255) {
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    value,
                    "El valor debe encontrarse entre 0 y 255.");
            }
        }

        private static void ValidateNonNegative(int value, string propertyName) {
            if(value < 0) {
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    value,
                    "El valor no puede ser negativo.");
            }
        }

        private static void ValidatePositive(int value, string propertyName) {
            if(value < 1) {
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    value,
                    "El valor debe ser mayor que cero.");
            }
        }
    }
}

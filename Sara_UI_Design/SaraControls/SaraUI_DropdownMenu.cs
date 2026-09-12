using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Menú contextual que aplica una paleta coherente a sus elementos y a
    /// toda la jerarquía de submenús sin alterar los datos del consumidor.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultProperty(nameof(PrimaryColor))]
    public class SaraUI_DropdownMenu:ContextMenuStrip {
        private bool _isMainMenu;
        private int _menuItemHeight = 35;
        private Color _menuItemTextColor = Color.Empty;
        private Color _disabledTextColor = Color.Empty;
        private Color _primaryColor = Color.MediumSlateBlue;
        private int _selectionOpacity = 36;
        private int _selectionCornerRadius = 6;
        private bool _applyUniformItemHeight = true;
        private bool _scaleItemHeightWithDpi = true;
        private bool _initialized;

        /// <summary>Inicializa un menú contextual independiente.</summary>
        public SaraUI_DropdownMenu() {
            InitializeMenu();
        }

        /// <summary>Inicializa un menú contextual asociado a un contenedor.</summary>
        /// <param name="container">Contenedor que administrará el ciclo de vida del menú.</param>
        public SaraUI_DropdownMenu(IContainer container)
            :base(container) {
            InitializeMenu();
        }

        /// <summary>Se produce después de reconstruir y propagar el tema.</summary>
        [Category("Sara UI Design")]
        public event EventHandler? ThemeChanged;

        /// <summary>
        /// Obtiene o establece si el menú utiliza la apariencia de una barra
        /// principal en lugar de la apariencia desplegable.
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(false)]
        public bool IsMainMenu {
            get => _isMainMenu;
            set {
                if(_isMainMenu == value) {
                    return;
                }

                _isMainMenu = value;
                UpdateRenderer();
            }
        }

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
        [DefaultValue(typeof(Color), "Empty")]
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

        /// <summary>Obtiene o establece la altura lógica de cada elemento.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce con valores menores que uno.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(35)]
        public int MenuItemHeight {
            get => _menuItemHeight;
            set {
                ValidatePositive(value, nameof(MenuItemHeight));

                if(_menuItemHeight == value) {
                    return;
                }

                _menuItemHeight = value;
                ApplyThemeToItems(Items);
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

        /// <summary>Obtiene o establece si se fuerza una altura uniforme.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool ApplyUniformItemHeight {
            get => _applyUniformItemHeight;
            set {
                if(_applyUniformItemHeight == value) {
                    return;
                }

                _applyUniformItemHeight = value;
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
        protected override void OnOpening(CancelEventArgs e) {
            base.OnOpening(e);

            if(e.Cancel) {
                return;
            }

            UpdateRenderer(false);
            ApplyThemeToItems(Items);
        }

        private void InitializeMenu() {
            _initialized = true;
            UpdateRenderer();
        }

        private void UpdateRenderer(bool raiseEvent = true) {
            if(!_initialized || IsDisposed) {
                return;
            }

            SaraUI_MenuRenderer renderer = new SaraUI_MenuRenderer(
                _isMainMenu,
                _primaryColor,
                _menuItemTextColor,
                _disabledTextColor,
                BackColor,
                _selectionCornerRadius,
                _selectionOpacity,
                ShowImageMargin);
            Renderer = renderer;
            ApplyThemeToItems(Items);
            Invalidate(true);

            if(raiseEvent) {
                ThemeChanged?.Invoke(this, EventArgs.Empty);
            }
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

            ApplyHeight(menuItem);

            menuItem.DropDown.Renderer = Renderer;

            if(menuItem.DropDown is ToolStripDropDownMenu dropDownMenu) {
                dropDownMenu.ShowImageMargin = ShowImageMargin;
            }

            ApplyThemeToItems(menuItem.DropDownItems);
        }

        private void ApplyHeight(ToolStripMenuItem item) {
            if(!_applyUniformItemHeight) {
                item.AutoSize = true;
                return;
            }

            int height = GetScaledItemHeight();
            Size preferred = item.GetPreferredSize(Size.Empty);
            item.AutoSize = false;
            item.Size = new Size(Math.Max(item.Width, preferred.Width), height);
        }

        private int GetScaledItemHeight() {
            if(!_scaleItemHeightWithDpi || !IsHandleCreated) {
                return _menuItemHeight;
            }

            return Math.Max(
                1,
                (int)Math.Round(_menuItemHeight * (DeviceDpi / 96f)));
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

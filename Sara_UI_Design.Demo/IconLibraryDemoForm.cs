using Sara_UI_Design.SaraControls;

namespace Sara_UI_Design.Demo {
    internal sealed class IconLibraryDemoForm:Form {
        private static readonly HashSet<string> LibraryUseCaseIcons = new HashSet<string>(
            new[] { "Add", "Update", "Delete", "Refresh", "InventoryLoad", "Code", "Title", "Author", "Genre" },
            StringComparer.OrdinalIgnoreCase);

        public IconLibraryDemoForm() {
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1060, 700);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            MinimumSize = new Size(860, 580);
            StartPosition = FormStartPosition.CenterParent;
            Text = "SaraUI_IconLibrary - Catálogo vectorial";

            Label statusLabel = new Label {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Font = new Font(Font, FontStyle.Bold),
                Location = new Point(20, 16),
                Size = new Size(1020, 24),
                Text = "Catálogo de iconos"
            };

            Label searchLabel = CreateFieldLabel("Buscar", 20);
            TextBox searchBox = new TextBox {
                Location = new Point(20, 62),
                Size = new Size(190, 25),
                TabIndex = 0
            };

            Label categoryLabel = CreateFieldLabel("Categoría", 224);
            ComboBox categoryCombo = new ComboBox {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(224, 62),
                Size = new Size(146, 25),
                TabIndex = 1
            };
            categoryCombo.Items.Add("Todas");
            foreach(SaraUI_IconLibrary.SaraIconCategory category in
                Enum.GetValues(typeof(SaraUI_IconLibrary.SaraIconCategory))) {
                categoryCombo.Items.Add(category.ToString());
            }
            categoryCombo.SelectedIndex = 0;

            Label styleLabel = CreateFieldLabel("Estilo", 384);
            ComboBox styleCombo = new ComboBox {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(384, 62),
                Size = new Size(120, 25),
                TabIndex = 2
            };
            foreach(SaraUI_IconLibrary.SaraIconStyle style in
                Enum.GetValues(typeof(SaraUI_IconLibrary.SaraIconStyle))) {
                styleCombo.Items.Add(style);
            }
            styleCombo.SelectedItem = SaraUI_IconLibrary.SaraIconStyle.Outline;

            Label sizeLabel = CreateFieldLabel("Tamaño", 518);
            NumericUpDown sizeSelector = new NumericUpDown {
                Location = new Point(518, 62),
                Maximum = 48,
                Minimum = 16,
                Size = new Size(64, 25),
                TabIndex = 3,
                Value = 32
            };

            Button colorButton = new Button {
                Location = new Point(596, 60),
                Size = new Size(100, 29),
                TabIndex = 4,
                Text = "Color: Violeta",
                UseVisualStyleBackColor = true
            };

            CheckBox aliasesCheckBox = new CheckBox {
                AutoSize = true,
                Location = new Point(712, 64),
                TabIndex = 5,
                Text = "Mostrar alias"
            };

            CheckBox libraryCaseCheckBox = new CheckBox {
                AutoSize = true,
                Location = new Point(824, 64),
                TabIndex = 6,
                Text = "Caso biblioteca"
            };

            FlowLayoutPanel gallery = new FlowLayoutPanel {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 245, 250),
                Location = new Point(20, 104),
                Padding = new Padding(10),
                Size = new Size(1020, 520),
                TabIndex = 7,
                WrapContents = true
            };

            Label instructionsLabel = new Label {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(20, 638),
                Size = new Size(1020, 42),
                Text = "Verifica: símbolos distinguibles a 16, 24, 32 y 48 px; estilos sin recortes; búsqueda y categorías; " +
                    "los nueve iconos del caso biblioteca; alias ocultos por defecto; redimensionamiento y cierre sin excepciones."
            };

            Color[] palette = {
                Color.FromArgb(119, 94, 229),
                Color.FromArgb(28, 126, 214),
                Color.FromArgb(18, 138, 95),
                Color.FromArgb(203, 55, 74),
                Color.FromArgb(45, 45, 58)
            };
            string[] colorNames = { "Violeta", "Azul", "Verde", "Carmesí", "Grafito" };
            int colorIndex = 0;

            void RefreshGallery() {
                string query = searchBox.Text.Trim();
                bool includeAliases = aliasesCheckBox.Checked;
                bool libraryCaseOnly = libraryCaseCheckBox.Checked;
                SaraUI_IconLibrary.SaraIconStyle style =
                    styleCombo.SelectedItem is SaraUI_IconLibrary.SaraIconStyle selectedStyle
                        ? selectedStyle
                        : SaraUI_IconLibrary.SaraIconStyle.Outline;
                SaraUI_IconLibrary.SaraIconCategory? category = null;
                string categoryText = categoryCombo.SelectedItem?.ToString() ?? string.Empty;
                if(categoryCombo.SelectedIndex > 0 &&
                    Enum.TryParse(categoryText, out SaraUI_IconLibrary.SaraIconCategory selectedCategory)) {
                    category = selectedCategory;
                }

                IReadOnlyList<SaraUI_IconLibrary.SaraIconInfo> catalog =
                    SaraUI_IconLibrary.GetIconCatalog(includeAliases);
                var visibleIcons = catalog.Where(icon =>
                    (!category.HasValue || icon.Category == category.Value) &&
                    (!libraryCaseOnly || LibraryUseCaseIcons.Contains(icon.CanonicalName)) &&
                    (query.Length == 0 ||
                        icon.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        icon.Description.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0));

                gallery.SuspendLayout();
                while(gallery.Controls.Count > 0) {
                    Control control = gallery.Controls[0];
                    gallery.Controls.RemoveAt(0);
                    control.Dispose();
                }

                int visibleCount = 0;
                foreach(SaraUI_IconLibrary.SaraIconInfo icon in visibleIcons) {
                    var tile = new IconTile(
                        icon,
                        style,
                        Decimal.ToInt32(sizeSelector.Value),
                        palette[colorIndex]);
                    tile.Selected += (_, _) => {
                        statusLabel.Text = icon.IsAlias
                            ? $"{icon.Name} → {icon.CanonicalName} | {icon.Category} | {icon.Description}"
                            : $"{icon.Name} | {icon.Category} | {icon.Description}";
                    };
                    gallery.Controls.Add(tile);
                    visibleCount++;
                }
                gallery.ResumeLayout(true);

                int canonicalCount = SaraUI_IconLibrary.GetIconCatalog().Count;
                int aliasCount = SaraUI_IconLibrary.GetIconCatalog(true).Count - canonicalCount;
                statusLabel.Text =
                    $"Visibles: {visibleCount} | Canónicos: {canonicalCount} | Alias compatibles: {aliasCount} | " +
                    $"{style} | {sizeSelector.Value:0}px";
            }

            searchBox.TextChanged += (_, _) => RefreshGallery();
            categoryCombo.SelectedIndexChanged += (_, _) => RefreshGallery();
            styleCombo.SelectedIndexChanged += (_, _) => RefreshGallery();
            sizeSelector.ValueChanged += (_, _) => RefreshGallery();
            aliasesCheckBox.CheckedChanged += (_, _) => RefreshGallery();
            libraryCaseCheckBox.CheckedChanged += (_, _) => {
                if(libraryCaseCheckBox.Checked) {
                    categoryCombo.SelectedIndex = 0;
                    searchBox.Clear();
                }
                RefreshGallery();
            };
            colorButton.Click += (_, _) => {
                colorIndex = (colorIndex + 1) % palette.Length;
                colorButton.Text = $"Color: {colorNames[colorIndex]}";
                RefreshGallery();
            };

            Controls.Add(statusLabel);
            Controls.Add(searchLabel);
            Controls.Add(searchBox);
            Controls.Add(categoryLabel);
            Controls.Add(categoryCombo);
            Controls.Add(styleLabel);
            Controls.Add(styleCombo);
            Controls.Add(sizeLabel);
            Controls.Add(sizeSelector);
            Controls.Add(colorButton);
            Controls.Add(aliasesCheckBox);
            Controls.Add(libraryCaseCheckBox);
            Controls.Add(gallery);
            Controls.Add(instructionsLabel);

            RefreshGallery();
        }

        private static Label CreateFieldLabel(string text, int x) {
            return new Label {
                AutoSize = true,
                Location = new Point(x, 43),
                Text = text
            };
        }

        private sealed class IconTile:Control {
            private readonly SaraUI_IconLibrary.SaraIconInfo icon;
            private readonly SaraUI_IconLibrary.SaraIconStyle style;
            private readonly int iconSize;
            private readonly Color iconColor;
            private bool hovered;

            public IconTile(
                SaraUI_IconLibrary.SaraIconInfo icon,
                SaraUI_IconLibrary.SaraIconStyle style,
                int iconSize,
                Color iconColor) {
                this.icon = icon ?? throw new ArgumentNullException(nameof(icon));
                this.style = style;
                this.iconSize = iconSize;
                this.iconColor = iconColor;

                AccessibleDescription = icon.Description;
                AccessibleName = $"Icono {icon.Name}";
                BackColor = icon.IsAlias ? Color.FromArgb(250, 245, 235) : Color.White;
                Cursor = Cursors.Hand;
                DoubleBuffered = true;
                Margin = new Padding(5);
                Size = new Size(148, 108);
                TabStop = true;
            }

            public event EventHandler? Selected;

            protected override void OnClick(EventArgs e) {
                base.OnClick(e);
                Focus();
                Selected?.Invoke(this, EventArgs.Empty);
            }

            protected override void OnEnter(EventArgs e) {
                base.OnEnter(e);
                Invalidate();
            }

            protected override void OnKeyDown(KeyEventArgs e) {
                if(e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) {
                    OnClick(EventArgs.Empty);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
                base.OnKeyDown(e);
            }

            protected override void OnMouseEnter(EventArgs e) {
                base.OnMouseEnter(e);
                hovered = true;
                Invalidate();
            }

            protected override void OnMouseLeave(EventArgs e) {
                base.OnMouseLeave(e);
                hovered = false;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e) {
                base.OnPaint(e);
                Color borderColor = Focused || hovered
                    ? iconColor
                    : Color.FromArgb(215, 215, 225);
                using Pen borderPen = new Pen(borderColor, Focused ? 2f : 1f);
                e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);

                int safeSize = Math.Min(iconSize, 48);
                Rectangle iconBounds = new Rectangle((Width - safeSize) / 2, 10, safeSize, safeSize);
                SaraUI_IconLibrary.TryDrawIcon(icon.Name, e.Graphics, iconBounds, iconColor, style);

                string caption = icon.IsAlias
                    ? $"{icon.Name} → {icon.CanonicalName}"
                    : icon.Name;
                TextRenderer.DrawText(
                    e.Graphics,
                    caption,
                    Font,
                    new Rectangle(5, 64, Width - 10, 36),
                    ForeColor,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.Top |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.WordBreak);
            }
        }
    }
}

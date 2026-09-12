using Sara_UI_Design.SaraControls;

namespace Sara_UI_Design.Demo {
    internal sealed class MenuDemoForm:Form {
        public MenuDemoForm() {
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(940, 650);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            MinimumSize = new Size(820, 600);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Paquete de menús - Pruebas de navegación y tema";

            SaraUI_MenuStrip mainMenu = new SaraUI_MenuStrip {
                BackColor = Color.FromArgb(37, 39, 60),
                DisabledTextColor = Color.FromArgb(140, 142, 155),
                Dock = DockStyle.Top,
                DropDownItemHeight = 35,
                MenuItemTextColor = Color.Gainsboro,
                PrimaryColor = Color.MediumSlateBlue,
                ShowDropDownImageMargin = true
            };
            MainMenuStrip = mainMenu;

            ToolStripMenuItem fileItem = new ToolStripMenuItem("&Archivo");
            ToolStripMenuItem newItem = new ToolStripMenuItem("&Nuevo") {
                Image = SystemIcons.Application.ToBitmap(),
                ShortcutKeys = Keys.Control | Keys.N
            };
            ToolStripMenuItem openItem = new ToolStripMenuItem("&Abrir...") {
                ShortcutKeys = Keys.Control | Keys.O
            };
            ToolStripMenuItem exportItem = new ToolStripMenuItem("Exportar") {
                Enabled = false
            };
            ToolStripMenuItem exitItem = new ToolStripMenuItem("&Cerrar demostración");
            fileItem.DropDownItems.AddRange(new ToolStripItem[] {
                newItem,
                openItem,
                new ToolStripSeparator(),
                exportItem,
                new ToolStripSeparator(),
                exitItem
            });

            ToolStripMenuItem editItem = new ToolStripMenuItem("&Editar");
            ToolStripMenuItem autoSaveItem = new ToolStripMenuItem("Guardado automático") {
                Checked = true,
                CheckOnClick = true
            };
            ToolStripMenuItem preferencesItem = new ToolStripMenuItem("Preferencias");
            preferencesItem.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Tema claro"),
                new ToolStripMenuItem("Tema oscuro"),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Idioma") {
                    DropDownItems = {
                        new ToolStripMenuItem("Español") { Checked = true },
                        new ToolStripMenuItem("English")
                    }
                }
            });
            editItem.DropDownItems.AddRange(new ToolStripItem[] {
                autoSaveItem,
                preferencesItem
            });

            ToolStripMenuItem viewItem = new ToolStripMenuItem("&Ver");
            ToolStripMenuItem panelItem = new ToolStripMenuItem("Panel lateral") {
                Checked = true,
                CheckOnClick = true
            };
            ToolStripMenuItem zoomItem = new ToolStripMenuItem("Zoom");
            zoomItem.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("80 %"),
                new ToolStripMenuItem("100 %") { Checked = true },
                new ToolStripMenuItem("125 %")
            });
            viewItem.DropDownItems.AddRange(new ToolStripItem[] { panelItem, zoomItem });

            ToolStripMenuItem helpItem = new ToolStripMenuItem("A&yuda");
            helpItem.DropDownItems.Add(new ToolStripMenuItem("Acerca de Sara UI"));
            mainMenu.Items.AddRange(new ToolStripItem[] {
                fileItem,
                editItem,
                viewItem,
                helpItem
            });
            mainMenu.RefreshTheme();

            SaraUI_DropdownMenu contextMenu = new SaraUI_DropdownMenu {
                BackColor = Color.White,
                DisabledTextColor = Color.DarkGray,
                MenuItemHeight = 35,
                MenuItemTextColor = Color.DimGray,
                PrimaryColor = Color.MediumSlateBlue,
                ShowImageMargin = true
            };
            ToolStripMenuItem copyItem = new ToolStripMenuItem("Copiar") {
                Image = SystemIcons.Information.ToBitmap(),
                ShortcutKeys = Keys.Control | Keys.C
            };
            ToolStripMenuItem pasteItem = new ToolStripMenuItem("Pegar") {
                ShortcutKeys = Keys.Control | Keys.V
            };
            ToolStripMenuItem shareItem = new ToolStripMenuItem("Compartir");
            shareItem.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Correo"),
                new ToolStripMenuItem("Enlace"),
                new ToolStripMenuItem("Código QR")
            });
            ToolStripMenuItem unavailableItem = new ToolStripMenuItem("Acción no disponible") {
                Enabled = false
            };
            contextMenu.Items.AddRange(new ToolStripItem[] {
                copyItem,
                pasteItem,
                new ToolStripSeparator(),
                shareItem,
                unavailableItem
            });
            contextMenu.RefreshTheme();

            Label titleLabel = new Label {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font(Font, FontStyle.Bold),
                Location = new Point(24, 48),
                Size = new Size(892, 24),
                Text = "SaraUI_MenuStrip, SaraUI_DropdownMenu, renderer y tabla de colores"
            };
            Label descriptionLabel = new Label {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(24, 78),
                Size = new Size(892, 38),
                Text = "Prueba submenús, accesos de teclado, elementos marcados y deshabilitados, propagación dinámica del tema y el menú contextual."
            };
            Label statusLabel = new Label {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(24, 119),
                Size = new Size(892, 38)
            };

            Panel previewPanel = new Panel {
                AccessibleDescription = "Abre el menú contextual mediante clic derecho o la tecla de menú.",
                AccessibleName = "Superficie de prueba del menú contextual",
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.FromArgb(238, 238, 248),
                BorderStyle = BorderStyle.FixedSingle,
                ContextMenuStrip = contextMenu,
                Location = new Point(24, 166),
                Size = new Size(438, 390),
                TabIndex = 0,
                TabStop = true
            };
            Label previewTitle = new Label {
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold),
                Location = new Point(26, 25),
                Text = "Superficie con menú contextual"
            };
            Label previewText = new Label {
                Location = new Point(26, 62),
                Size = new Size(382, 86),
                Text = "Haz clic derecho en esta superficie. Recorre el menú con las flechas, abre Compartir y usa Esc para cerrarlo. También puedes usar Mayús+F10."
            };
            Button showMainButton = CreateActionButton("Abrir Archivo");
            showMainButton.Location = new Point(26, 172);
            Button showContextButton = CreateActionButton("Abrir contextual");
            showContextButton.Location = new Point(200, 172);
            Label dataLabel = new Label {
                Location = new Point(26, 230),
                Size = new Size(382, 82),
                Text = "El renderer elige colores únicamente para pintar. No reemplaza Text, Tag, ForeColor, Checked, Enabled ni ShortcutKeys de los elementos."
            };
            Label shortcutLabel = new Label {
                Location = new Point(26, 330),
                Size = new Size(382, 45),
                Text = "Atajos: Alt+A abre Archivo; Ctrl+N y Ctrl+O activan sus acciones."
            };
            previewPanel.Controls.Add(previewTitle);
            previewPanel.Controls.Add(previewText);
            previewPanel.Controls.Add(showMainButton);
            previewPanel.Controls.Add(showContextButton);
            previewPanel.Controls.Add(dataLabel);
            previewPanel.Controls.Add(shortcutLabel);

            FlowLayoutPanel actionsPanel = new FlowLayoutPanel {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(246, 246, 250),
                Location = new Point(480, 166),
                Padding = new Padding(12),
                Size = new Size(436, 390),
                WrapContents = true
            };
            Label actionsHelpLabel = new Label {
                Margin = new Padding(3, 0, 3, 8),
                Size = new Size(400, 42),
                Text = "Los cambios se observan en el menú que se abrirá automáticamente; no modifican el fondo general de esta ventana."
            };
            Button colorButton = CreateActionButton("Color: Violeta");
            Button backgroundButton = CreateActionButton("Fondo: Mixto");
            Button heightButton = CreateActionButton("Altura: 35");
            Button radiusButton = CreateActionButton("Radio: 6");
            Button opacityButton = CreateActionButton("Opacidad: 36");
            Button imageMarginButton = CreateActionButton("Margen imagen: Sí");
            Button uniformButton = CreateActionButton("Altura uniforme: Sí");
            Button rightToLeftButton = CreateActionButton("RTL: No");
            Button mainStyleButton = CreateActionButton("Contextual: desplegable");
            Button disableButton = CreateActionButton("Deshabilitar Copiar");
            Button addButton = CreateActionButton("Agregar dinámico");
            Button validationButton = CreateActionButton("Validaciones");

            actionsPanel.Controls.Add(actionsHelpLabel);
            actionsPanel.Controls.Add(colorButton);
            actionsPanel.Controls.Add(backgroundButton);
            actionsPanel.Controls.Add(heightButton);
            actionsPanel.Controls.Add(radiusButton);
            actionsPanel.Controls.Add(opacityButton);
            actionsPanel.Controls.Add(imageMarginButton);
            actionsPanel.Controls.Add(uniformButton);
            actionsPanel.Controls.Add(rightToLeftButton);
            actionsPanel.Controls.Add(mainStyleButton);
            actionsPanel.Controls.Add(disableButton);
            actionsPanel.Controls.Add(addButton);
            actionsPanel.Controls.Add(validationButton);

            Label instructionsLabel = new Label {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(24, 570),
                Size = new Size(892, 58),
                Text = "Verifica: apertura con ratón y teclado; flechas y submenús; marcas, iconos, separadores, atajos y deshabilitados; cambios inmediatos de color, fondo, radio, opacidad, altura, margen y RTL; elemento dinámico tematizado; validaciones y cierre sin excepciones."
            };

            int completedActions = 0;
            int dynamicItems = 0;
            int colorIndex = 0;
            int heightIndex = 0;
            int radiusIndex = 0;
            int opacityIndex = 0;
            bool darkContext = false;
            string lastAction = "Sin acciones";
            Color[] colors = { Color.MediumSlateBlue, Color.SeaGreen, Color.Crimson };
            string[] colorNames = { "Violeta", "Verde", "Carmesí" };
            int[] heights = { 35, 26, 48 };
            int[] radii = { 6, 0, 14, 100 };
            int[] opacities = { 36, 0, 96, 220 };

            void UpdateStatus() {
                statusLabel.Text =
                    $"Principal: {mainMenu.PrimaryColor.Name}, {mainMenu.DropDownItemHeight}px | " +
                    $"Contextual: {(contextMenu.IsMainMenu ? "principal" : "desplegable")}, {contextMenu.MenuItemHeight}px | " +
                    $"Dinámicos: {dynamicItems} | Acciones: {completedActions} | {lastAction}";
            }

            void RegisterAction(string action) {
                completedActions++;
                lastAction = action;
                UpdateStatus();
            }

            void ShowContextPreview(
                ToolStripMenuItem? selectedItem = null,
                bool openSubmenu = false) {
                contextMenu.Show(
                    previewPanel,
                    new Point(showContextButton.Left, showContextButton.Bottom + 4));

                ToolStripMenuItem item = selectedItem ??
                    (copyItem.Enabled ? copyItem : pasteItem);
                item.Select();

                if(openSubmenu) {
                    item.ShowDropDown();
                }
            }

            void AttachLeafFeedback(ToolStripItemCollection items) {
                foreach(ToolStripItem item in items) {
                    if(item is not ToolStripMenuItem menuItem) {
                        continue;
                    }

                    if(menuItem.DropDownItems.Count > 0) {
                        AttachLeafFeedback(menuItem.DropDownItems);
                        continue;
                    }

                    if(menuItem == newItem ||
                        menuItem == openItem ||
                        menuItem == exitItem ||
                        menuItem == autoSaveItem ||
                        menuItem == panelItem ||
                        menuItem == copyItem ||
                        menuItem == pasteItem) {
                        continue;
                    }

                    menuItem.Click += (_, _) => RegisterAction(
                        $"opción {(menuItem.Text ?? "(sin texto)").Replace("&", string.Empty)} seleccionada");
                }
            }

            newItem.Click += (_, _) => RegisterAction("Nuevo mediante menú o Ctrl+N");
            openItem.Click += (_, _) => RegisterAction("Abrir mediante menú o Ctrl+O");
            exitItem.Click += (_, _) => Close();
            autoSaveItem.CheckedChanged += (_, _) => RegisterAction(
                autoSaveItem.Checked ? "guardado automático activo" : "guardado automático inactivo");
            panelItem.CheckedChanged += (_, _) => RegisterAction(
                panelItem.Checked ? "panel visible" : "panel oculto");
            copyItem.Click += (_, _) => RegisterAction("Copiar activado");
            pasteItem.Click += (_, _) => RegisterAction("Pegar activado");

            showMainButton.Click += (_, _) => fileItem.ShowDropDown();
            showContextButton.Click += (_, _) => ShowContextPreview();

            colorButton.Click += (_, _) => {
                colorIndex = (colorIndex + 1) % colors.Length;
                mainMenu.PrimaryColor = colors[colorIndex];
                contextMenu.PrimaryColor = colors[colorIndex];
                colorButton.Text = $"Color: {colorNames[colorIndex]}";
                RegisterAction("color principal actualizado");
                ShowContextPreview();
            };
            backgroundButton.Click += (_, _) => {
                darkContext = !darkContext;
                mainMenu.BackColor = darkContext ? Color.White : Color.FromArgb(37, 39, 60);
                mainMenu.MenuItemTextColor = darkContext ? Color.DimGray : Color.Gainsboro;
                contextMenu.BackColor = darkContext ? Color.FromArgb(37, 39, 60) : Color.White;
                contextMenu.MenuItemTextColor = darkContext ? Color.Gainsboro : Color.DimGray;
                backgroundButton.Text = darkContext ? "Fondo: Invertido" : "Fondo: Mixto";
                RegisterAction("fondos y textos actualizados");
                ShowContextPreview();
            };
            heightButton.Click += (_, _) => {
                heightIndex = (heightIndex + 1) % heights.Length;
                mainMenu.DropDownItemHeight = heights[heightIndex];
                contextMenu.MenuItemHeight = heights[heightIndex];
                heightButton.Text = $"Altura: {heights[heightIndex]}";
                RegisterAction("altura actualizada");
                ShowContextPreview();
            };
            radiusButton.Click += (_, _) => {
                radiusIndex = (radiusIndex + 1) % radii.Length;
                mainMenu.SelectionCornerRadius = radii[radiusIndex];
                contextMenu.SelectionCornerRadius = radii[radiusIndex];
                radiusButton.Text = $"Radio: {radii[radiusIndex]}";
                RegisterAction("radio actualizado de forma segura");
                ShowContextPreview();
            };
            opacityButton.Click += (_, _) => {
                opacityIndex = (opacityIndex + 1) % opacities.Length;
                mainMenu.SelectionOpacity = opacities[opacityIndex];
                contextMenu.SelectionOpacity = opacities[opacityIndex];
                opacityButton.Text = $"Opacidad: {opacities[opacityIndex]}";
                RegisterAction("opacidad actualizada");
                ShowContextPreview();
            };
            imageMarginButton.Click += (_, _) => {
                bool show = !mainMenu.ShowDropDownImageMargin;
                mainMenu.ShowDropDownImageMargin = show;
                contextMenu.ShowImageMargin = show;
                contextMenu.RefreshTheme();
                imageMarginButton.Text = show ? "Margen imagen: Sí" : "Margen imagen: No";
                RegisterAction("margen de imágenes actualizado");
                ShowContextPreview();
            };
            uniformButton.Click += (_, _) => {
                bool uniform = !mainMenu.ApplyUniformDropDownItemHeight;
                mainMenu.ApplyUniformDropDownItemHeight = uniform;
                contextMenu.ApplyUniformItemHeight = uniform;
                uniformButton.Text = uniform ? "Altura uniforme: Sí" : "Altura uniforme: No";
                RegisterAction("administración de altura actualizada");
                ShowContextPreview();
            };
            rightToLeftButton.Click += (_, _) => {
                bool enable = mainMenu.RightToLeft != RightToLeft.Yes;
                mainMenu.RightToLeft = enable ? RightToLeft.Yes : RightToLeft.No;
                contextMenu.RightToLeft = enable ? RightToLeft.Yes : RightToLeft.No;
                rightToLeftButton.Text = enable ? "RTL: Sí" : "RTL: No";
                RegisterAction("dirección de lectura actualizada");
                ShowContextPreview();
            };
            mainStyleButton.Click += (_, _) => {
                contextMenu.IsMainMenu = !contextMenu.IsMainMenu;
                mainStyleButton.Text = contextMenu.IsMainMenu
                    ? "Contextual: principal"
                    : "Contextual: desplegable";
                RegisterAction("tipo visual del menú contextual actualizado");
                ShowContextPreview();
            };
            disableButton.Click += (_, _) => {
                copyItem.Enabled = !copyItem.Enabled;
                disableButton.Text = copyItem.Enabled ? "Deshabilitar Copiar" : "Habilitar Copiar";
                RegisterAction(copyItem.Enabled ? "Copiar habilitado" : "Copiar deshabilitado");
                ShowContextPreview();
            };
            addButton.Click += (_, _) => {
                dynamicItems++;
                int itemNumber = dynamicItems;
                ToolStripMenuItem dynamicItem = new ToolStripMenuItem($"Dinámico {itemNumber}");
                dynamicItem.Click += (_, _) => RegisterAction($"Dinámico {itemNumber} activado");
                shareItem.DropDownItems.Add(dynamicItem);
                mainMenu.RefreshTheme();
                contextMenu.RefreshTheme();
                RegisterAction($"elemento dinámico {dynamicItems} agregado y tematizado");
                ShowContextPreview(shareItem, true);
                dynamicItem.Select();
            };
            validationButton.Click += (_, _) => {
                int rejected = 0;

                try {
                    mainMenu.DropDownItemHeight = 0;
                } catch(ArgumentOutOfRangeException) {
                    rejected++;
                }

                try {
                    contextMenu.SelectionCornerRadius = -1;
                } catch(ArgumentOutOfRangeException) {
                    rejected++;
                }

                try {
                    contextMenu.SelectionOpacity = 256;
                } catch(ArgumentOutOfRangeException) {
                    rejected++;
                }

                RegisterAction($"validaciones rechazadas: {rejected}/3");
            };

            AttachLeafFeedback(mainMenu.Items);
            AttachLeafFeedback(contextMenu.Items);

            mainMenu.ThemeChanged += (_, _) => UpdateStatus();
            contextMenu.ThemeChanged += (_, _) => UpdateStatus();

            Controls.Add(titleLabel);
            Controls.Add(descriptionLabel);
            Controls.Add(statusLabel);
            Controls.Add(previewPanel);
            Controls.Add(actionsPanel);
            Controls.Add(instructionsLabel);
            Controls.Add(mainMenu);

            FormClosed += (_, _) => contextMenu.Dispose();
            UpdateStatus();
        }

        private static Button CreateActionButton(string text) {
            return new Button {
                Margin = new Padding(3),
                Size = new Size(190, 38),
                Text = text,
                UseVisualStyleBackColor = true
            };
        }
    }
}

using Sara_UI_Design.Animations;
using Sara_UI_Design.SaraControls;

namespace Sara_UI_Design.Demo {
    internal sealed class TabControlDemoForm:Form {
        public TabControlDemoForm() {
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(760, 500);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            MinimumSize = new Size(780, 420);
            StartPosition = FormStartPosition.CenterParent;
            Text = "SaraUI_TabControl - Pruebas de interacción";

            Label statusLabel = new Label {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(20, 18),
                Size = new Size(720, 28)
            };

            SaraUI_TabControl tabs = new SaraUI_TabControl {
                AccessibleDescription = "Permite cambiar, cerrar y reorganizar visualmente secciones de la demostración.",
                AccessibleName = "Pestañas de demostración",
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AnimationDuration = 320,
                AnimationEasing = SaraEasing.EaseInOutCubic,
                ContentBackColor = Color.White,
                FocusBorderColor = Color.HotPink,
                HoverTabColor = Color.Lavender,
                IndicatorColor = Color.HotPink,
                Location = new Point(20, 52),
                PressedTabColor = Color.Thistle,
                SelectedTabColor = Color.MediumSlateBlue,
                ShowCloseButtons = true,
                Size = new Size(720, 330),
                StretchTabs = true,
                TabIndex = 0,
                UnselectedTabColor = Color.FromArgb(230, 230, 240),
                UnselectedTextColor = Color.DimGray
            };

            TabPage homePage = CreatePage(
                "&Inicio",
                "Esta pestaña demuestra la cancelación de cierre mediante TabClosing.");
            tabs.TabPages.Add(homePage);
            tabs.TabPages.Add(CreatePage(
                "&Datos",
                "Usa Ctrl+Tab y Ctrl+Mayús+Tab para navegar entre páginas habilitadas."));
            tabs.TabPages.Add(CreatePage(
                "&Configuración",
                "Prueba el indicador animado, RightToLeft y el cierre con Ctrl+W."));

            FlowLayoutPanel actionsPanel = new FlowLayoutPanel {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(20, 398),
                Size = new Size(720, 44),
                WrapContents = false
            };

            Button addButton = CreateActionButton("Agregar");
            Button stretchButton = CreateActionButton("Estirar: Sí");
            Button rightToLeftButton = CreateActionButton("RTL: No");
            Button disableButton = CreateActionButton("Deshabilitar");
            Button pauseButton = CreateActionButton("Pausar");
            Button resumeButton = CreateActionButton("Reanudar");
            Button stopButton = CreateActionButton("Detener");
            TabPage? disabledPage = null;
            int addedPageCount = 0;
            string lastAction = "Sin acciones";

            void UpdateStatus() {
                statusLabel.Text =
                    $"Seleccionada: {tabs.SelectedIndex} | Visual: {tabs.VisualState} | " +
                    $"Animación: {tabs.AnimationState} | Indicador: {tabs.DisplayedSelectedTabIndex:0.00} | " +
                    $"Último: {lastAction}";
            }

            addButton.Click += (_, _) => {
                addedPageCount++;
                TabPage page = CreatePage(
                    $"Nueva {addedPageCount}",
                    "Esta página fue agregada durante la ejecución.");
                tabs.TabPages.Add(page);
                tabs.SelectedTab = page;
                lastAction = $"agregada {page.Text}";
                UpdateStatus();
            };

            stretchButton.Click += (_, _) => {
                tabs.StretchTabs = !tabs.StretchTabs;
                stretchButton.Text = tabs.StretchTabs ? "Estirar: Sí" : "Estirar: No";
                lastAction = "distribución cambiada";
                UpdateStatus();
            };

            rightToLeftButton.Click += (_, _) => {
                bool enableRightToLeft = tabs.RightToLeft != RightToLeft.Yes;
                tabs.RightToLeft = enableRightToLeft ? RightToLeft.Yes : RightToLeft.No;
                tabs.RightToLeftLayout = enableRightToLeft;
                rightToLeftButton.Text = enableRightToLeft ? "RTL: Sí" : "RTL: No";
                lastAction = "dirección cambiada";
                UpdateStatus();
            };

            disableButton.Click += (_, _) => {
                if(disabledPage != null && !disabledPage.IsDisposed) {
                    disabledPage.Enabled = true;
                    lastAction = $"habilitada {disabledPage.Text}";
                    disabledPage = null;
                    disableButton.Text = "Deshabilitar";
                } else if(tabs.SelectedTab != null) {
                    disabledPage = tabs.SelectedTab;
                    disabledPage.Enabled = false;
                    lastAction = $"deshabilitada {disabledPage.Text}";
                    tabs.SelectNextTab(forward: true);
                    disableButton.Text = "Habilitar última";
                }

                tabs.Invalidate();
                UpdateStatus();
            };

            pauseButton.Click += (_, _) => {
                tabs.PauseAnimation();
                lastAction = "pausa";
                UpdateStatus();
            };
            resumeButton.Click += (_, _) => {
                tabs.ResumeAnimation();
                lastAction = "reanudación";
                UpdateStatus();
            };
            stopButton.Click += (_, _) => {
                tabs.StopAnimation();
                lastAction = "detención";
                UpdateStatus();
            };

            tabs.TabClosing += (_, e) => {
                if(e.TabPage == homePage) {
                    e.Cancel = true;
                    lastAction = "Inicio está protegida";
                } else {
                    lastAction = $"cerrando {e.TabPage.Text}";
                }

                UpdateStatus();
            };
            tabs.TabClosed += (_, e) => {
                string closedText = e.TabPage.Text;

                if(e.TabPage == disabledPage) {
                    disabledPage = null;
                    disableButton.Text = "Deshabilitar";
                }

                e.TabPage.Dispose();
                lastAction = $"cerrada {closedText}";
                UpdateStatus();
            };
            tabs.SelectedIndexChanged += (_, _) => UpdateStatus();
            tabs.VisualStateChanged += (_, _) => UpdateStatus();
            tabs.AnimationStateChanged += (_, _) => UpdateStatus();

            actionsPanel.Controls.Add(addButton);
            actionsPanel.Controls.Add(stretchButton);
            actionsPanel.Controls.Add(rightToLeftButton);
            actionsPanel.Controls.Add(disableButton);
            actionsPanel.Controls.Add(pauseButton);
            actionsPanel.Controls.Add(resumeButton);
            actionsPanel.Controls.Add(stopButton);

            Label instructionsLabel = new Label {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(20, 452),
                Size = new Size(720, 34),
                Text = "Verifica: selección e indicador suaves; hover y presión; Ctrl+Tab; Ctrl+Mayús+Tab; Ctrl+W; cierre cancelable; pestañas deshabilitadas; RTL; pausa y detención."
            };

            Controls.Add(statusLabel);
            Controls.Add(tabs);
            Controls.Add(actionsPanel);
            Controls.Add(instructionsLabel);

            UpdateStatus();
        }

        private static TabPage CreatePage(string text, string description) {
            TabPage page = new TabPage {
                BackColor = Color.White,
                Padding = new Padding(24),
                Text = text
            };
            Label descriptionLabel = new Label {
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                Text = description,
                TextAlign = ContentAlignment.MiddleCenter
            };
            page.Controls.Add(descriptionLabel);
            return page;
        }

        private static Button CreateActionButton(string text) {
            return new Button {
                Margin = new Padding(2),
                Size = new Size(98, 34),
                Text = text,
                UseVisualStyleBackColor = true
            };
        }
    }
}

using Sara_UI_Design.Animations;
using Sara_UI_Design.SaraControls;

namespace Sara_UI_Design.Demo {
    public partial class MainForm:Form {
        public MainForm() {
            InitializeComponent();
            ConfigureAnimationExamples();
            ConfigureButtonExamples();
            ConfigureRadioButtonExamples();
            ConfigureTextBoxExamples();
            ConfigureComboBoxExamples();
            ConfigureSideBarExamples();
        }

        private void ConfigureAnimationExamples() {
            panel1.BackColor = Color.White;
            panel1.Padding = new Padding(24);

            Label titleLabel = CreateLabel(
                "Pruebas del motor de animaciones",
                new Point(24, 24),
                new Size(520, 28));

            Label instructionsLabel = CreateLabel(
                "Compara ambos progresos, el movimiento, la opacidad y barras de desplazamiento en los dos ejes.",
                new Point(24, 56),
                new Size(520, 44));

            SaraUI_CircularProgressBar circularProgress = new SaraUI_CircularProgressBar {
                AnimationFunction = SaraEasing.EaseInOutCubic,
                AnimationSpeed = 700,
                Location = new Point(209, 100),
                MarqueeAnimationDuration = 1600,
                Size = new Size(150, 150),
                SubscriptText = string.Empty,
                Text = "20",
                Value = 20
            };

            SaraUI_ProgressBar linearProgress = new SaraUI_ProgressBar {
                AccessibleDescription = "Representa el mismo valor que el progreso circular.",
                AccessibleName = "Progreso lineal de la demostración",
                AnimationDuration = 700,
                AnimationEasing = SaraEasing.EaseInOutCubic,
                ChannelColor = Color.FromArgb(230, 230, 240),
                ChannelHeight = 10,
                ForeColor = Color.FromArgb(64, 64, 64),
                Location = new Point(24, 296),
                MarqueeAnimationDuration = 1600,
                MarqueeSegmentPercentage = 28,
                ShowValue = TextPosition.Sliding,
                Size = new Size(520, 38),
                SliderColor = Color.MediumSlateBlue,
                SliderColorEnd = Color.HotPink,
                SliderHeight = 14,
                SymbolAfter = "%",
                Value = 20
            };

            Label stateLabel = CreateLabel(
                string.Empty,
                new Point(24, 252),
                new Size(520, 38));
            stateLabel.TextAlign = ContentAlignment.MiddleCenter;

            Panel motionTrack = new Panel {
                BackColor = Color.FromArgb(238, 238, 248),
                Location = new Point(24, 344),
                Size = new Size(520, 68)
            };

            Panel movingPanel = new Panel {
                BackColor = Color.MediumSlateBlue,
                Location = new Point(16, 8),
                Size = new Size(36, 36)
            };

            SaraUI_ScrollBar horizontalScroll = new SaraUI_ScrollBar {
                AccessibleDescription = "Desplaza horizontalmente un valor entre cero y cien.",
                AccessibleName = "Desplazamiento horizontal de demostración",
                AnimationDuration = 300,
                AnimationEasing = SaraEasing.EaseInOutCubic,
                AutoSizeForOrientation = false,
                ChannelColor = Color.FromArgb(218, 218, 234),
                ChannelThickness = 8,
                FocusBorderColor = Color.HotPink,
                HoverThumbColor = Color.SlateBlue,
                LargeChange = 20,
                Location = new Point(16, 52),
                MinimumThumbSize = 32,
                Orientation = SaraUI_ScrollBar.ScrollOrientation.Horizontal,
                PressedThumbColor = Color.DarkSlateBlue,
                Size = new Size(458, 12),
                SmallChange = 5,
                TabIndex = 0,
                ThumbColor = Color.MediumSlateBlue,
                Value = 20
            };

            SaraUI_ScrollBar verticalScroll = new SaraUI_ScrollBar {
                AccessibleDescription = "Desplaza verticalmente un valor entre cero y cien.",
                AccessibleName = "Desplazamiento vertical de demostración",
                AnimationDuration = 300,
                AnimationEasing = SaraEasing.EaseInOutCubic,
                AutoSizeForOrientation = false,
                ChannelColor = Color.FromArgb(218, 218, 234),
                ChannelThickness = 8,
                FocusBorderColor = Color.HotPink,
                HoverThumbColor = Color.SlateBlue,
                LargeChange = 20,
                Location = new Point(494, 8),
                MinimumThumbSize = 12,
                PressedThumbColor = Color.DarkSlateBlue,
                Size = new Size(12, 40),
                SmallChange = 5,
                TabIndex = 1,
                ThumbColor = Color.MediumSlateBlue,
                Value = 20
            };

            SaraUI_ScrollBar activeScroll = horizontalScroll;

            motionTrack.Controls.Add(movingPanel);
            motionTrack.Controls.Add(horizontalScroll);
            motionTrack.Controls.Add(verticalScroll);
            controlTransitions.Target = movingPanel;
            windowTransitions.Target = this;

            FlowLayoutPanel actionsPanel = new FlowLayoutPanel {
                Location = new Point(24, 422),
                Size = new Size(520, 86),
                WrapContents = true
            };

            Button value25Button = CreateActionButton("Valor 25");
            Button value80Button = CreateActionButton("Valor 80");
            Button marqueeButton = CreateActionButton("Marquee");
            Button moveButton = CreateActionButton("Mover panel");
            Button pauseButton = CreateActionButton("Pausar");
            Button resumeButton = CreateActionButton("Reanudar");
            Button stopButton = CreateActionButton("Detener");
            Button fadeButton = CreateActionButton("Atenuar ventana");

            void UpdateStateLabel() {
                stateLabel.Text =
                    $"Circular: {circularProgress.AnimationState} | Lineal: {linearProgress.AnimationState} " +
                    $"({linearProgress.DisplayedValue:0})\n" +
                    $"P: {controlTransitions.State} | V: {windowTransitions.State} | " +
                    $"H:{horizontalScroll.Value} V:{verticalScroll.Value} | " +
                    $"S:{activeScroll.VisualState}/{activeScroll.AnimationState}";
            }

            void SetProgressValue(int value) {
                if(circularProgress.Style == ProgressBarStyle.Marquee) {
                    circularProgress.Style = ProgressBarStyle.Continuous;
                }

                if(linearProgress.Style == ProgressBarStyle.Marquee) {
                    linearProgress.Style = ProgressBarStyle.Continuous;
                }

                circularProgress.Text = value.ToString();
                circularProgress.Value = value;
                linearProgress.Value = value;
                horizontalScroll.Value = value;
                verticalScroll.Value = 100 - value;
            }

            value25Button.Click += (_, _) => SetProgressValue(25);
            value80Button.Click += (_, _) => SetProgressValue(80);
            marqueeButton.Click += (_, _) => {
                ProgressBarStyle destinationStyle =
                    circularProgress.Style == ProgressBarStyle.Marquee &&
                    linearProgress.Style == ProgressBarStyle.Marquee
                        ? ProgressBarStyle.Continuous
                        : ProgressBarStyle.Marquee;

                circularProgress.Style = destinationStyle;
                linearProgress.Style = destinationStyle;
            };

            moveButton.Click += (_, _) => {
                int leftDestination = 16;
                int rightDestination = verticalScroll.Left - movingPanel.Width - 16;
                int destination = movingPanel.Left < motionTrack.ClientSize.Width / 2
                    ? rightDestination
                    : leftDestination;

                controlTransitions.MoveTo(
                    new Point(destination, movingPanel.Top),
                    new SaraAnimationOptions {
                        AutoReverse = true,
                        Duration = 900,
                        Easing = SaraEasing.EaseInOutCubic
                    });
            };

            fadeButton.Click += (_, _) => {
                double destination = Opacity > 0.85d ? 0.70d : 1d;

                windowTransitions.FadeTo(
                    destination,
                    new SaraAnimationOptions {
                        Duration = 450,
                        Easing = SaraEasing.EaseInOutQuad
                    });
            };

            pauseButton.Click += (_, _) => {
                controlTransitions.Pause();
                windowTransitions.Pause();
                circularProgress.PauseAnimation();
                linearProgress.PauseAnimation();
                horizontalScroll.PauseAnimation();
                verticalScroll.PauseAnimation();
            };

            resumeButton.Click += (_, _) => {
                controlTransitions.Resume();
                windowTransitions.Resume();
                circularProgress.ResumeAnimation();
                linearProgress.ResumeAnimation();
                horizontalScroll.ResumeAnimation();
                verticalScroll.ResumeAnimation();
            };

            stopButton.Click += (_, _) => {
                controlTransitions.Stop();
                windowTransitions.Stop();
                circularProgress.StopAnimation();
                linearProgress.StopAnimation();
                horizontalScroll.StopAnimation();
                verticalScroll.StopAnimation();
            };

            controlTransitions.StateChanged += (_, _) => UpdateStateLabel();
            windowTransitions.StateChanged += (_, _) => UpdateStateLabel();
            circularProgress.AnimationStateChanged += (_, _) => UpdateStateLabel();
            linearProgress.AnimationStateChanged += (_, _) => UpdateStateLabel();

            void ObserveScroll(SaraUI_ScrollBar scrollBar) {
                scrollBar.ValueChanged += (_, _) => {
                    activeScroll = scrollBar;
                    UpdateStateLabel();
                };
                scrollBar.VisualStateChanged += (_, _) => {
                    activeScroll = scrollBar;
                    UpdateStateLabel();
                };
                scrollBar.AnimationStateChanged += (_, _) => {
                    activeScroll = scrollBar;
                    UpdateStateLabel();
                };
            }

            ObserveScroll(horizontalScroll);
            ObserveScroll(verticalScroll);

            actionsPanel.Controls.Add(value25Button);
            actionsPanel.Controls.Add(value80Button);
            actionsPanel.Controls.Add(marqueeButton);
            actionsPanel.Controls.Add(moveButton);
            actionsPanel.Controls.Add(pauseButton);
            actionsPanel.Controls.Add(resumeButton);
            actionsPanel.Controls.Add(stopButton);
            actionsPanel.Controls.Add(fadeButton);

            Label checklistLabel = CreateLabel(
                "Verifica: progresos y scroll animados; arrastre directo; canal, rueda y teclado funcionales; pausa sin saltos y detención inmediata.",
                new Point(24, 516),
                new Size(520, 78));

            panel1.Controls.Add(titleLabel);
            panel1.Controls.Add(instructionsLabel);
            panel1.Controls.Add(circularProgress);
            panel1.Controls.Add(stateLabel);
            panel1.Controls.Add(linearProgress);
            panel1.Controls.Add(motionTrack);
            panel1.Controls.Add(actionsPanel);
            panel1.Controls.Add(checklistLabel);

            UpdateStateLabel();
        }

        private void ConfigureButtonExamples() {
            Label titleLabel = CreateLabel(
                "SaraUI_Button: ratón, foco, teclado y estado deshabilitado",
                new Point(24, 606),
                new Size(288, 26));

            SaraUI_Button saveButton = new SaraUI_Button {
                AccessibleDescription = "Guarda los datos de la demostración.",
                AccessibleName = "Guardar datos",
                AnimationDuration = 180,
                BackColor = Color.MediumSlateBlue,
                BorderColor = Color.MediumSlateBlue,
                BorderRadius = 12,
                BorderSize = 1,
                FocusBorderColor = Color.HotPink,
                ForeColor = Color.White,
                HoverBackColor = Color.SlateBlue,
                IconColor = Color.MistyRose,
                IconName = "Check",
                Location = new Point(24, 638),
                PressedBackColor = Color.DarkSlateBlue,
                Size = new Size(138, 42),
                TabIndex = 0,
                Text = "&Guardar"
            };

            SaraUI_Button deleteButton = new SaraUI_Button {
                AccessibleDescription = "Elimina el elemento seleccionado de la demostración.",
                AccessibleName = "Eliminar elemento",
                AnimationDuration = 180,
                BackColor = Color.White,
                BorderColor = Color.Firebrick,
                BorderRadius = 12,
                BorderSize = 1,
                FocusBorderColor = Color.HotPink,
                ForeColor = Color.Firebrick,
                HoverBackColor = Color.MistyRose,
                IconLocation = SaraUI_Button.SaraIconLocation.Right,
                IconName = "Trash",
                Location = new Point(174, 638),
                Padding = new Padding(10, 0, 10, 0),
                PressedBackColor = Color.FromArgb(244, 198, 204),
                Size = new Size(138, 42),
                TabIndex = 1,
                Text = "&Eliminar",
                TextAlign = ContentAlignment.MiddleLeft
            };

            CheckBox disableCheckBox = new CheckBox {
                AutoSize = true,
                Location = new Point(24, 680),
                TabIndex = 2,
                Text = "Deshabilitar Guardar"
            };

            Label stateLabel = CreateLabel(
                string.Empty,
                new Point(174, 680),
                new Size(138, 24));

            SaraUI_Button activeButton = saveButton;

            void UpdateStateLabel() {
                stateLabel.Text = $"{activeButton.Text.Replace("&", string.Empty)}: " +
                    $"{activeButton.VisualState} | {activeButton.AnimationState}";
            }

            void ObserveButton(SaraUI_Button button) {
                button.VisualStateChanged += (_, _) => {
                    activeButton = button;
                    UpdateStateLabel();
                };
                button.AnimationStateChanged += (_, _) => {
                    activeButton = button;
                    UpdateStateLabel();
                };
            }

            ObserveButton(saveButton);
            ObserveButton(deleteButton);
            disableCheckBox.CheckedChanged += (_, _) => {
                saveButton.Enabled = !disableCheckBox.Checked;
                activeButton = saveButton;
                UpdateStateLabel();
            };

            panel1.Controls.Add(titleLabel);
            panel1.Controls.Add(saveButton);
            panel1.Controls.Add(deleteButton);
            panel1.Controls.Add(disableCheckBox);
            panel1.Controls.Add(stateLabel);

            UpdateStateLabel();
        }

        private void ConfigureRadioButtonExamples() {
            GroupBox optionsGroup = new GroupBox {
                Location = new Point(326, 594),
                Size = new Size(218, 110),
                TabIndex = 3,
                TabStop = false,
                Text = "SaraUI_RadioButton"
            };

            SaraUI_RadioButton basicOption = new SaraUI_RadioButton {
                AccessibleDescription = "Selecciona el plan básico de la demostración.",
                AccessibleName = "Plan básico",
                AnimationDuration = 220,
                AnimationEasing = SaraEasing.EaseInOutCubic,
                AutoSize = false,
                Checked = true,
                CheckedColor = Color.MediumSlateBlue,
                FocusBorderColor = Color.HotPink,
                HoverColor = Color.SlateBlue,
                Location = new Point(10, 24),
                PressedColor = Color.DarkSlateBlue,
                Size = new Size(94, 26),
                TabIndex = 0,
                Text = "&Básico",
                UncheckedColor = Color.Gray
            };

            SaraUI_RadioButton proOption = new SaraUI_RadioButton {
                AccessibleDescription = "Selecciona el plan profesional de la demostración.",
                AccessibleName = "Plan profesional",
                AnimationDuration = 220,
                AnimationEasing = SaraEasing.EaseInOutCubic,
                AutoSize = false,
                CheckedColor = Color.MediumSlateBlue,
                FocusBorderColor = Color.HotPink,
                HoverColor = Color.SlateBlue,
                Location = new Point(10, 54),
                PressedColor = Color.DarkSlateBlue,
                Size = new Size(94, 26),
                TabIndex = 1,
                Text = "&Pro",
                UncheckedColor = Color.Gray
            };

            Label stateLabel = CreateLabel(
                string.Empty,
                new Point(110, 22),
                new Size(98, 42));

            CheckBox disableCheckBox = new CheckBox {
                Location = new Point(110, 70),
                Size = new Size(100, 24),
                TabIndex = 2,
                Text = "Deshabilitar"
            };

            SaraUI_RadioButton activeOption = basicOption;

            void UpdateStateLabel() {
                stateLabel.Text =
                    $"{activeOption.Text.Replace("&", string.Empty)}: {activeOption.VisualState}\n" +
                    $"{activeOption.AnimationState} ({activeOption.DisplayedCheckedProgress:0.00})";
            }

            void ObserveOption(SaraUI_RadioButton option) {
                option.CheckedChanged += (_, _) => {
                    if(option.Checked) {
                        activeOption = option;
                    }

                    UpdateStateLabel();
                };
                option.VisualStateChanged += (_, _) => {
                    activeOption = option;
                    UpdateStateLabel();
                };
                option.AnimationStateChanged += (_, _) => {
                    activeOption = option;
                    UpdateStateLabel();
                };
            }

            ObserveOption(basicOption);
            ObserveOption(proOption);
            disableCheckBox.CheckedChanged += (_, _) => {
                basicOption.Enabled = !disableCheckBox.Checked;
                proOption.Enabled = !disableCheckBox.Checked;
                activeOption = basicOption.Checked ? basicOption : proOption;
                UpdateStateLabel();
            };

            optionsGroup.Controls.Add(basicOption);
            optionsGroup.Controls.Add(proOption);
            optionsGroup.Controls.Add(stateLabel);
            optionsGroup.Controls.Add(disableCheckBox);
            panel1.Controls.Add(optionsGroup);

            UpdateStateLabel();
        }

        private void ConfigureTextBoxExamples() {
            panel2.BackColor = Color.White;
            panel2.Padding = new Padding(24);

            Label titleLabel = CreateLabel(
                "Pruebas de SaraUI_TextBox, SaraUI_ComboBox y SaraUI_ToggleButton",
                new Point(24, 24),
                new Size(520, 28));

            Label instructionsLabel = CreateLabel(
                "Comprueba entradas, selección desplegable y los tres estados del interruptor.",
                new Point(24, 56),
                new Size(520, 42));

            SaraUI_TextBox textInput = CreateTextBox("Buscar por nombre", "Search", new Point(24, 112));
            SaraUI_TextBox passwordInput = CreateTextBox("Contraseña", "Lock", new Point(24, 164));
            passwordInput.Type = SaraUI_TextBox.InputType.Password;

            SaraUI_TextBox numericInput = CreateTextBox("Solo números", "Stats", new Point(24, 216));
            numericInput.Type = SaraUI_TextBox.InputType.Numeric;

            SaraUI_TextBox multilineInput = CreateTextBox("Escribe una descripción", "Edit", new Point(24, 316));
            multilineInput.Type = SaraUI_TextBox.InputType.Multiline;
            multilineInput.Height = 76;

            Label eventStatusLabel = CreateLabel(
                "TextChanged: 0 | _TextChanged: 0",
                new Point(24, 400),
                new Size(520, 28));

            int standardEventCount = 0;
            int legacyEventCount = 0;

            textInput.TextChanged += (_, _) => {
                standardEventCount++;
                UpdateEventStatus(eventStatusLabel, standardEventCount, legacyEventCount);
            };

            textInput._TextChanged += (_, _) => {
                legacyEventCount++;
                UpdateEventStatus(eventStatusLabel, standardEventCount, legacyEventCount);
            };

            Button clearButton = new Button {
                Location = new Point(24, 436),
                Size = new Size(180, 36),
                Text = "Vaciar texto normal",
                UseVisualStyleBackColor = true
            };
            clearButton.Click += (_, _) => textInput.Text = string.Empty;

            SaraUI_ToggleButton validationToggle = new SaraUI_ToggleButton {
                AccessibleDescription = "Activa, desactiva o deja indeterminada la entrada numérica.",
                AccessibleName = "Validación de entrada numérica",
                AnimationDuration = 220,
                AnimationEasing = SaraEasing.EaseInOutCubic,
                CheckState = CheckState.Checked,
                FocusBorderColor = Color.HotPink,
                IndeterminateBackColor = Color.DarkGoldenrod,
                Location = new Point(224, 443),
                OffBackColor = Color.Gray,
                OnBackColor = Color.MediumSlateBlue,
                Size = new Size(50, 26),
                TabIndex = 5,
                Text = "Validación de entrada numérica",
                ThreeState = true
            };

            Label toggleStateLabel = CreateLabel(
                string.Empty,
                new Point(282, 434),
                new Size(126, 40));
            toggleStateLabel.TextAlign = ContentAlignment.MiddleLeft;

            CheckBox disableToggleCheckBox = new CheckBox {
                Location = new Point(414, 441),
                Size = new Size(130, 30),
                TabIndex = 6,
                Text = "Deshabilitar"
            };

            void UpdateToggleState() {
                toggleStateLabel.Text =
                    $"{validationToggle.CheckState}\n" +
                    $"{validationToggle.VisualState} | {validationToggle.AnimationState}";
            }

            validationToggle.CheckStateChanged += (_, _) => {
                numericInput.Enabled = validationToggle.CheckState != CheckState.Unchecked;
                UpdateToggleState();
            };
            validationToggle.VisualStateChanged += (_, _) => UpdateToggleState();
            validationToggle.AnimationStateChanged += (_, _) => UpdateToggleState();
            disableToggleCheckBox.CheckedChanged += (_, _) => {
                validationToggle.Enabled = !disableToggleCheckBox.Checked;
                UpdateToggleState();
            };

            panel2.Controls.Add(titleLabel);
            panel2.Controls.Add(instructionsLabel);
            panel2.Controls.Add(textInput);
            panel2.Controls.Add(passwordInput);
            panel2.Controls.Add(numericInput);
            panel2.Controls.Add(multilineInput);
            panel2.Controls.Add(eventStatusLabel);
            panel2.Controls.Add(clearButton);
            panel2.Controls.Add(validationToggle);
            panel2.Controls.Add(toggleStateLabel);
            panel2.Controls.Add(disableToggleCheckBox);

            UpdateToggleState();
        }

        private void ConfigureComboBoxExamples() {
            SaraUI_ComboBox environmentComboBox = new SaraUI_ComboBox {
                AccessibleDescription = "Selecciona el entorno utilizado por la demostración.",
                AccessibleName = "Entorno de ejecución",
                AnimationDuration = 220,
                AnimationEasing = SaraEasing.EaseInOutCubic,
                BackColor = Color.White,
                BorderColor = Color.MediumSlateBlue,
                BorderFocusColor = Color.HotPink,
                DropDownStyle = ComboBoxStyle.DropDownList,
                HoverBackColor = Color.Lavender,
                IconColor = Color.MediumSlateBlue,
                ListBackColor = Color.White,
                ListTextColor = Color.DimGray,
                Location = new Point(24, 268),
                PlaceholderText = "Selecciona un entorno",
                Size = new Size(330, 40),
                TabIndex = 3
            };
            environmentComboBox.Items.AddRange(new object[] {
                "Desarrollo",
                "Pruebas",
                "Producción"
            });

            Label stateLabel = CreateLabel(
                string.Empty,
                new Point(366, 266),
                new Size(178, 24));
            stateLabel.TextAlign = ContentAlignment.MiddleLeft;

            CheckBox disableCheckBox = new CheckBox {
                Location = new Point(366, 288),
                Size = new Size(178, 24),
                TabIndex = 4,
                Text = "Deshabilitar lista"
            };

            int standardEventCount = 0;
            int legacyEventCount = 0;

            void UpdateStateLabel() {
                stateLabel.Text =
                    $"{environmentComboBox.VisualState} | {environmentComboBox.AnimationState} " +
                    $"S:{standardEventCount} A:{legacyEventCount}";
            }

            environmentComboBox.SelectedIndexChanged += (_, _) => {
                standardEventCount++;
                UpdateStateLabel();
            };
            environmentComboBox.OnSelectedIndexChanged += (_, _) => {
                legacyEventCount++;
                UpdateStateLabel();
            };
            environmentComboBox.VisualStateChanged += (_, _) => UpdateStateLabel();
            environmentComboBox.AnimationStateChanged += (_, _) => UpdateStateLabel();
            disableCheckBox.CheckedChanged += (_, _) => {
                environmentComboBox.Enabled = !disableCheckBox.Checked;
                UpdateStateLabel();
            };

            panel2.Controls.Add(environmentComboBox);
            panel2.Controls.Add(stateLabel);
            panel2.Controls.Add(disableCheckBox);

            UpdateStateLabel();
        }

        private void ConfigureSideBarExamples() {
            Label titleLabel = CreateLabel(
                "Pruebas de SaraUI_SideBar",
                new Point(24, 488),
                new Size(520, 28));

            Panel sideBarHost = new Panel {
                BackColor = Color.FromArgb(238, 238, 248),
                Location = new Point(24, 520),
                Size = new Size(520, 128)
            };

            SaraUI_SideBar sideBar = new SaraUI_SideBar {
                AnimationDuration = 500,
                AnimationEasing = SaraEasing.EaseInOutCubic,
                BackColor = Color.FromArgb(45, 45, 65),
                CollapsedWidth = 64,
                ExpandedWidth = 230
            };

            SaraUI_Button homeButton = CreateSideBarButton("Inicio", "Dashboard", new Point(8, 14));
            SaraUI_Button settingsButton = CreateSideBarButton("Ajustes", "Settings", new Point(8, 64));
            sideBar.Controls.Add(homeButton);
            sideBar.Controls.Add(settingsButton);

            Label stateLabel = CreateLabel(
                string.Empty,
                new Point(250, 12),
                new Size(260, 26));

            Button toggleButton = CreateActionButton("Alternar");
            toggleButton.Location = new Point(250, 43);

            Button pauseButton = CreateActionButton("Pausar");
            pauseButton.Location = new Point(376, 43);

            Button resumeButton = CreateActionButton("Reanudar");
            resumeButton.Location = new Point(250, 83);

            Button stopButton = CreateActionButton("Detener");
            stopButton.Location = new Point(376, 83);

            void ResizeMenuButtons() {
                int buttonWidth = Math.Max(1, sideBar.ClientSize.Width - 16);
                homeButton.Width = buttonWidth;
                settingsButton.Width = buttonWidth;
            }

            void UpdateStateLabel() {
                string destination = sideBar.IsExpanded ? "Expandida" : "Contraída";
                stateLabel.Text = $"{destination} | {sideBar.AnimationState} | {sideBar.Width}px";
            }

            sideBar.SizeChanged += (_, _) => {
                ResizeMenuButtons();
                UpdateStateLabel();
            };
            sideBar.IsExpandedChanged += (_, _) => UpdateStateLabel();
            sideBar.AnimationStateChanged += (_, _) => UpdateStateLabel();

            toggleButton.Click += (_, _) => sideBar.Toggle();
            pauseButton.Click += (_, _) => sideBar.PauseAnimation();
            resumeButton.Click += (_, _) => sideBar.ResumeAnimation();
            stopButton.Click += (_, _) => sideBar.StopAnimation();

            sideBarHost.Controls.Add(sideBar);
            sideBarHost.Controls.Add(stateLabel);
            sideBarHost.Controls.Add(toggleButton);
            sideBarHost.Controls.Add(pauseButton);
            sideBarHost.Controls.Add(resumeButton);
            sideBarHost.Controls.Add(stopButton);
            panel2.Controls.Add(titleLabel);
            panel2.Controls.Add(sideBarHost);

            ResizeMenuButtons();
            UpdateStateLabel();
        }

        private static SaraUI_TextBox CreateTextBox(string placeholder, string iconName, Point location) {
            return new SaraUI_TextBox {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BorderColor = Color.MediumSlateBlue,
                BorderFocusColor = Color.HotPink,
                BorderRadius = 12,
                IconColor = Color.MediumSlateBlue,
                IconName = iconName,
                Location = location,
                PlaceholderText = placeholder,
                Size = new Size(520, 44)
            };
        }

        private static Label CreateLabel(string text, Point location, Size size) {
            return new Label {
                AutoEllipsis = true,
                Location = location,
                Size = size,
                Text = text
            };
        }

        private static Button CreateActionButton(string text) {
            return new Button {
                Margin = new Padding(3),
                Size = new Size(120, 34),
                Text = text,
                UseVisualStyleBackColor = true
            };
        }

        private static SaraUI_Button CreateSideBarButton(string text, string iconName, Point location) {
            return new SaraUI_Button {
                BackColor = Color.FromArgb(45, 45, 65),
                BorderRadius = 8,
                ForeColor = Color.White,
                IconName = iconName,
                Location = location,
                Size = new Size(214, 38),
                Text = text
            };
        }

        private static void UpdateEventStatus(Label label, int standardCount, int legacyCount) {
            label.Text = $"TextChanged: {standardCount} | _TextChanged: {legacyCount}";
        }
    }
}

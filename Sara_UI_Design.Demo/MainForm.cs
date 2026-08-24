using Sara_UI_Design.Animations;
using Sara_UI_Design.SaraControls;

namespace Sara_UI_Design.Demo {
    public partial class MainForm:Form {
        public MainForm() {
            InitializeComponent();
            ConfigureAnimationExamples();
            ConfigureTextBoxExamples();
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
                "Prueba el progreso, el movimiento de un control y la opacidad de la ventana.",
                new Point(24, 56),
                new Size(520, 44));

            SaraUI_CircularProgressBar circularProgress = new SaraUI_CircularProgressBar {
                AnimationFunction = SaraEasing.EaseInOutCubic,
                AnimationSpeed = 700,
                Location = new Point(194, 108),
                MarqueeAnimationDuration = 1600,
                Size = new Size(196, 196),
                SubscriptText = string.Empty,
                Text = "20",
                Value = 20
            };

            Label stateLabel = CreateLabel(
                string.Empty,
                new Point(24, 314),
                new Size(520, 28));
            stateLabel.TextAlign = ContentAlignment.MiddleCenter;

            Panel motionTrack = new Panel {
                BackColor = Color.FromArgb(238, 238, 248),
                Location = new Point(24, 352),
                Size = new Size(520, 70)
            };

            Panel movingPanel = new Panel {
                BackColor = Color.MediumSlateBlue,
                Location = new Point(16, 17),
                Size = new Size(36, 36)
            };
            motionTrack.Controls.Add(movingPanel);
            controlTransitions.Target = movingPanel;
            windowTransitions.Target = this;

            FlowLayoutPanel actionsPanel = new FlowLayoutPanel {
                Location = new Point(24, 438),
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
                    $"Panel: {controlTransitions.State} | Circular: {circularProgress.AnimationState} | " +
                    $"Ventana: {windowTransitions.State}";
            }

            void SetCircularValue(int value) {
                if(circularProgress.Style == ProgressBarStyle.Marquee) {
                    circularProgress.Style = ProgressBarStyle.Continuous;
                }

                circularProgress.Text = value.ToString();
                circularProgress.Value = value;
            }

            value25Button.Click += (_, _) => SetCircularValue(25);
            value80Button.Click += (_, _) => SetCircularValue(80);
            marqueeButton.Click += (_, _) => {
                circularProgress.Style = circularProgress.Style == ProgressBarStyle.Marquee
                    ? ProgressBarStyle.Continuous
                    : ProgressBarStyle.Marquee;
            };

            moveButton.Click += (_, _) => {
                int leftDestination = 16;
                int rightDestination = motionTrack.ClientSize.Width - movingPanel.Width - 16;
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
            };

            resumeButton.Click += (_, _) => {
                controlTransitions.Resume();
                windowTransitions.Resume();
                circularProgress.ResumeAnimation();
            };

            stopButton.Click += (_, _) => {
                controlTransitions.Stop();
                windowTransitions.Stop();
                circularProgress.StopAnimation();
            };

            controlTransitions.StateChanged += (_, _) => UpdateStateLabel();
            windowTransitions.StateChanged += (_, _) => UpdateStateLabel();
            circularProgress.AnimationStateChanged += (_, _) => UpdateStateLabel();

            actionsPanel.Controls.Add(value25Button);
            actionsPanel.Controls.Add(value80Button);
            actionsPanel.Controls.Add(marqueeButton);
            actionsPanel.Controls.Add(moveButton);
            actionsPanel.Controls.Add(pauseButton);
            actionsPanel.Controls.Add(resumeButton);
            actionsPanel.Controls.Add(stopButton);
            actionsPanel.Controls.Add(fadeButton);

            Label checklistLabel = CreateLabel(
                "Verifica: movimiento y opacidad suaves, pausa sin saltos, reanudación desde el mismo punto y detención inmediata.",
                new Point(24, 540),
                new Size(520, 58));

            panel1.Controls.Add(titleLabel);
            panel1.Controls.Add(instructionsLabel);
            panel1.Controls.Add(circularProgress);
            panel1.Controls.Add(stateLabel);
            panel1.Controls.Add(motionTrack);
            panel1.Controls.Add(actionsPanel);
            panel1.Controls.Add(checklistLabel);

            UpdateStateLabel();
        }

        private void ConfigureTextBoxExamples() {
            panel2.BackColor = Color.White;
            panel2.Padding = new Padding(24);

            Label titleLabel = CreateLabel(
                "Pruebas de SaraUI_TextBox",
                new Point(24, 24),
                new Size(520, 28));

            Label instructionsLabel = CreateLabel(
                "Los contadores deben avanzar juntos y no cambiar al enfocar o abandonar el control.",
                new Point(24, 56),
                new Size(520, 42));

            SaraUI_TextBox textInput = CreateTextBox("Buscar por nombre", "Search", new Point(24, 112));
            SaraUI_TextBox passwordInput = CreateTextBox("Contraseña", "Lock", new Point(24, 176));
            passwordInput.Type = SaraUI_TextBox.InputType.Password;

            SaraUI_TextBox numericInput = CreateTextBox("Solo números", "Stats", new Point(24, 240));
            numericInput.Type = SaraUI_TextBox.InputType.Numeric;

            SaraUI_TextBox multilineInput = CreateTextBox("Escribe una descripción", "Edit", new Point(24, 304));
            multilineInput.Type = SaraUI_TextBox.InputType.Multiline;
            multilineInput.Height = 96;

            Label eventStatusLabel = CreateLabel(
                "TextChanged: 0 | _TextChanged: 0",
                new Point(24, 424),
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
                Location = new Point(24, 468),
                Size = new Size(180, 36),
                Text = "Vaciar texto normal",
                UseVisualStyleBackColor = true
            };
            clearButton.Click += (_, _) => textInput.Text = string.Empty;

            panel2.Controls.Add(titleLabel);
            panel2.Controls.Add(instructionsLabel);
            panel2.Controls.Add(textInput);
            panel2.Controls.Add(passwordInput);
            panel2.Controls.Add(numericInput);
            panel2.Controls.Add(multilineInput);
            panel2.Controls.Add(eventStatusLabel);
            panel2.Controls.Add(clearButton);
        }

        private void ConfigureSideBarExamples() {
            Label titleLabel = CreateLabel(
                "Pruebas de SaraUI_SideBar",
                new Point(24, 520),
                new Size(520, 28));

            Panel sideBarHost = new Panel {
                BackColor = Color.FromArgb(238, 238, 248),
                Location = new Point(24, 552),
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

using Sara_UI_Design.SaraControls;

namespace Sara_UI_Design.Demo {
    public partial class MainForm:Form {
        public MainForm() {
            InitializeComponent();
            ConfigureTextBoxExamples();
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

        private static void UpdateEventStatus(Label label, int standardCount, int legacyCount) {
            label.Text = $"TextChanged: {standardCount} | _TextChanged: {legacyCount}";
        }
    }
}

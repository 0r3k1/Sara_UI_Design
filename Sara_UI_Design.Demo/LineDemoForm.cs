using System.Drawing.Drawing2D;
using Sara_UI_Design.Animations;
using Sara_UI_Design.SaraControls;

namespace Sara_UI_Design.Demo {
    internal sealed class LineDemoForm:Form {
        public LineDemoForm() {
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1040, 650);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            MinimumSize = new Size(1060, 690);
            StartPosition = FormStartPosition.CenterParent;
            Text = "SaraUI_Line - Pruebas de separadores";

            Label titleLabel = new Label {
                AutoSize = true,
                Location = new Point(20, 18),
                Text = "Líneas horizontales y verticales con geometría, estados y DPI seguros."
            };

            Label statusLabel = new Label {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(20, 47),
                Size = new Size(1000, 28)
            };

            Panel previewPanel = new Panel {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.White,
                Location = new Point(20, 82),
                Size = new Size(650, 500)
            };

            SaraUI_Line primaryLine = new SaraUI_Line {
                AccessibleDescription = "Separador principal interactivo.",
                AccessibleName = "Línea principal",
                Alignment = SaraUI_Line.LineAlignment.Center,
                AnimationDuration = 700,
                AnimationEasing = SaraEasing.EaseInOutCubic,
                EndCap = LineCap.Round,
                FocusCueColor = Color.HotPink,
                FocusLineColor = Color.HotPink,
                HoverLineColor = Color.DeepSkyBlue,
                LineColor = Color.MediumSlateBlue,
                LineColor2 = Color.HotPink,
                LineStyle = DashStyle.Solid,
                LineWidth = 8,
                Location = new Point(38, 48),
                Padding = new Padding(24, 12, 24, 12),
                Size = new Size(570, 58),
                StartCap = LineCap.Round,
                TabIndex = 0,
                TabStop = true
            };

            Label primaryLabel = new Label {
                Location = new Point(38, 112),
                Size = new Size(570, 38),
                Text = "Principal: pasa el ratón, haz clic para darle foco y usa Tab para salir.",
                TextAlign = ContentAlignment.TopCenter
            };

            SaraUI_Line verticalLine = new SaraUI_Line {
                Alignment = SaraUI_Line.LineAlignment.Center,
                DashCap = DashCap.Round,
                EndCap = LineCap.Triangle,
                LineColor = Color.Teal,
                LineStyle = DashStyle.Dash,
                LineWidth = 6,
                Location = new Point(72, 165),
                Orientation = SaraUI_Line.LineOrientation.Vertical,
                Padding = new Padding(12, 18, 12, 18),
                Size = new Size(72, 245),
                StartCap = LineCap.Round
            };

            Label verticalLabel = new Label {
                Location = new Point(38, 418),
                Size = new Size(140, 42),
                Text = "Vertical, discontinua y con remates distintos.",
                TextAlign = ContentAlignment.TopCenter
            };

            Label alignmentLabel = new Label {
                AutoSize = true,
                Location = new Point(205, 166),
                Text = "Alineación transversal"
            };

            SaraUI_Line nearLine = CreateAlignmentLine(
                SaraUI_Line.LineAlignment.Near,
                Color.Crimson,
                new Point(205, 196));
            SaraUI_Line centerLine = CreateAlignmentLine(
                SaraUI_Line.LineAlignment.Center,
                Color.DarkOrange,
                new Point(205, 270));
            SaraUI_Line farLine = CreateAlignmentLine(
                SaraUI_Line.LineAlignment.Far,
                Color.SeaGreen,
                new Point(205, 344));

            Label nearLabel = CreateCaption("Near", new Point(502, 208));
            Label centerLabel = CreateCaption("Center", new Point(502, 282));
            Label farLabel = CreateCaption("Far", new Point(502, 356));

            SaraUI_Line tinyLine = new SaraUI_Line {
                LineColor = Color.MediumSlateBlue,
                LineColor2 = Color.HotPink,
                LineWidth = 80,
                Location = new Point(230, 435),
                Padding = new Padding(2),
                Size = new Size(12, 10)
            };

            Label tinyLabel = new Label {
                Location = new Point(258, 423),
                Size = new Size(330, 38),
                Text = "Tamaño mínimo con LineWidth = 80: el grosor se limita sin excepción.",
                TextAlign = ContentAlignment.MiddleLeft
            };

            previewPanel.Controls.Add(primaryLine);
            previewPanel.Controls.Add(primaryLabel);
            previewPanel.Controls.Add(verticalLine);
            previewPanel.Controls.Add(verticalLabel);
            previewPanel.Controls.Add(alignmentLabel);
            previewPanel.Controls.Add(nearLine);
            previewPanel.Controls.Add(centerLine);
            previewPanel.Controls.Add(farLine);
            previewPanel.Controls.Add(nearLabel);
            previewPanel.Controls.Add(centerLabel);
            previewPanel.Controls.Add(farLabel);
            previewPanel.Controls.Add(tinyLine);
            previewPanel.Controls.Add(tinyLabel);

            FlowLayoutPanel actionsPanel = new FlowLayoutPanel {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                AutoScroll = true,
                BackColor = Color.FromArgb(244, 244, 249),
                Location = new Point(688, 82),
                Padding = new Padding(9),
                Size = new Size(332, 500),
                WrapContents = true
            };

            Button orientationButton = CreateActionButton("Horizontal");
            Button alignmentButton = CreateActionButton("Alinear: Center");
            Button styleButton = CreateActionButton("Línea: Solid");
            Button capButton = CreateActionButton("Remates: Round");
            Button gradientButton = CreateActionButton("Degradado: Sí");
            Button rightToLeftButton = CreateActionButton("RTL: No");
            Button widthButton = CreateActionButton("Grosor: 8");
            Button paddingButton = CreateActionButton("Padding: 24");
            Button dpiButton = CreateActionButton("Escala DPI: Sí");
            Button enabledButton = CreateActionButton("Deshabilitar");
            Button tinyButton = CreateActionButton("Mínimo: 12x10");
            Button validationButton = CreateActionButton("Validaciones");
            Button pauseButton = CreateActionButton("Pausar");
            Button resumeButton = CreateActionButton("Reanudar");
            Button stopButton = CreateActionButton("Detener");

            int completedCount = 0;
            int canceledCount = 0;
            int widthIndex = 1;
            int paddingIndex = 1;
            int styleIndex = 0;
            int capIndex = 0;
            bool tinyCompact = true;
            string lastAction = "Sin acciones";
            int[] widths = { 1, 8, 80 };
            int[] paddings = { 0, 24, 70 };
            DashStyle[] styles = {
                DashStyle.Solid,
                DashStyle.Dash,
                DashStyle.Dot,
                DashStyle.DashDot,
                DashStyle.DashDotDot
            };
            LineCap[] caps = {
                LineCap.Round,
                LineCap.Flat,
                LineCap.Square,
                LineCap.Triangle
            };

            void UpdateStatus() {
                statusLabel.Text =
                    $"{primaryLine.Orientation} | {primaryLine.Alignment} | " +
                    $"{primaryLine.LineStyle} | {primaryLine.LineWidth}px | " +
                    $"{primaryLine.VisualState} | {primaryLine.AnimationState} | " +
                    $"C:{completedCount} X:{canceledCount} | {lastAction}";
            }

            orientationButton.Click += (_, _) => {
                bool vertical = primaryLine.Orientation == SaraUI_Line.LineOrientation.Horizontal;
                primaryLine.Orientation = vertical
                    ? SaraUI_Line.LineOrientation.Vertical
                    : SaraUI_Line.LineOrientation.Horizontal;
                primaryLine.Location = vertical
                    ? new Point(580, 28)
                    : new Point(38, 48);
                primaryLine.Size = vertical
                    ? new Size(58, 390)
                    : new Size(570, 58);
                orientationButton.Text = vertical ? "Vertical" : "Horizontal";
                lastAction = "orientación cambiada";
                UpdateStatus();
            };

            alignmentButton.Click += (_, _) => {
                primaryLine.Alignment = NextValue(primaryLine.Alignment);
                alignmentButton.Text = $"Alinear: {primaryLine.Alignment}";
                lastAction = "alineación cambiada";
                UpdateStatus();
            };

            styleButton.Click += (_, _) => {
                styleIndex = (styleIndex + 1) % styles.Length;
                primaryLine.LineStyle = styles[styleIndex];
                styleButton.Text = $"Línea: {primaryLine.LineStyle}";
                lastAction = "patrón cambiado";
                UpdateStatus();
            };

            capButton.Click += (_, _) => {
                capIndex = (capIndex + 1) % caps.Length;
                primaryLine.StartCap = caps[capIndex];
                primaryLine.EndCap = caps[capIndex];
                capButton.Text = $"Remates: {caps[capIndex]}";
                lastAction = "remates cambiados";
                UpdateStatus();
            };

            gradientButton.Click += (_, _) => {
                primaryLine.LineColor2 = primaryLine.LineColor2.IsEmpty
                    ? Color.HotPink
                    : Color.Empty;
                gradientButton.Text = primaryLine.LineColor2.IsEmpty
                    ? "Degradado: No"
                    : "Degradado: Sí";
                lastAction = "degradado cambiado";
                UpdateStatus();
            };

            rightToLeftButton.Click += (_, _) => {
                bool enabled = primaryLine.RightToLeft != RightToLeft.Yes;
                primaryLine.RightToLeft = enabled ? RightToLeft.Yes : RightToLeft.No;
                rightToLeftButton.Text = enabled ? "RTL: Sí" : "RTL: No";
                lastAction = "sentido lógico cambiado";
                UpdateStatus();
            };

            widthButton.Click += (_, _) => {
                widthIndex = (widthIndex + 1) % widths.Length;
                primaryLine.LineWidth = widths[widthIndex];
                widthButton.Text = $"Grosor: {primaryLine.LineWidth}";
                lastAction = "grosor cambiado";
                UpdateStatus();
            };

            paddingButton.Click += (_, _) => {
                paddingIndex = (paddingIndex + 1) % paddings.Length;
                int value = paddings[paddingIndex];
                primaryLine.Padding = new Padding(value);
                paddingButton.Text = $"Padding: {value}";
                lastAction = "margen interior cambiado";
                UpdateStatus();
            };

            dpiButton.Click += (_, _) => {
                primaryLine.ScaleLineWidthWithDpi = !primaryLine.ScaleLineWidthWithDpi;
                dpiButton.Text = primaryLine.ScaleLineWidthWithDpi
                    ? "Escala DPI: Sí"
                    : "Escala DPI: No";
                lastAction = "escalado DPI cambiado";
                UpdateStatus();
            };

            enabledButton.Click += (_, _) => {
                primaryLine.Enabled = !primaryLine.Enabled;
                enabledButton.Text = primaryLine.Enabled ? "Deshabilitar" : "Habilitar";
                lastAction = "estado Enabled cambiado";
                UpdateStatus();
            };

            tinyButton.Click += (_, _) => {
                tinyCompact = !tinyCompact;
                tinyLine.Size = tinyCompact ? new Size(12, 10) : new Size(150, 26);
                tinyButton.Text = tinyCompact ? "Mínimo: 12x10" : "Restaurar mínimo";
                lastAction = "tamaño extremo cambiado";
                UpdateStatus();
            };

            validationButton.Click += (_, _) => {
                int rejected = 0;

                try {
                    primaryLine.LineWidth = 0;
                } catch(ArgumentOutOfRangeException) {
                    rejected++;
                }

                try {
                    primaryLine.LineStyle = DashStyle.Custom;
                } catch(ArgumentException) {
                    rejected++;
                }

                try {
                    primaryLine.StartCap = LineCap.Custom;
                } catch(ArgumentException) {
                    rejected++;
                }

                try {
                    primaryLine.AnimationFrameInterval = 0;
                } catch(ArgumentOutOfRangeException) {
                    rejected++;
                }

                lastAction = rejected == 4
                    ? "4 valores inválidos rechazados sin alterar el estado"
                    : $"validación incompleta: {rejected}/4";
                UpdateStatus();
            };

            pauseButton.Click += (_, _) => {
                lastAction = primaryLine.PauseAnimation()
                    ? "animación pausada"
                    : "sin animación activa";
                UpdateStatus();
            };

            resumeButton.Click += (_, _) => {
                lastAction = primaryLine.ResumeAnimation()
                    ? "animación reanudada"
                    : "sin pausa activa";
                UpdateStatus();
            };

            stopButton.Click += (_, _) => {
                lastAction = primaryLine.StopAnimation()
                    ? "animación detenida"
                    : "sin animación activa";
                UpdateStatus();
            };

            primaryLine.VisualStateChanged += (_, _) => UpdateStatus();
            primaryLine.AnimationCompleted += (_, _) => {
                completedCount++;
                UpdateStatus();
            };
            primaryLine.AnimationCanceled += (_, _) => {
                canceledCount++;
                UpdateStatus();
            };
            primaryLine.AnimationStateChanged += (_, _) => UpdateStatus();

            actionsPanel.Controls.Add(orientationButton);
            actionsPanel.Controls.Add(alignmentButton);
            actionsPanel.Controls.Add(styleButton);
            actionsPanel.Controls.Add(capButton);
            actionsPanel.Controls.Add(gradientButton);
            actionsPanel.Controls.Add(rightToLeftButton);
            actionsPanel.Controls.Add(widthButton);
            actionsPanel.Controls.Add(paddingButton);
            actionsPanel.Controls.Add(dpiButton);
            actionsPanel.Controls.Add(enabledButton);
            actionsPanel.Controls.Add(tinyButton);
            actionsPanel.Controls.Add(validationButton);
            actionsPanel.Controls.Add(pauseButton);
            actionsPanel.Controls.Add(resumeButton);
            actionsPanel.Controls.Add(stopButton);

            Label instructionsLabel = new Label {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(20, 595),
                Size = new Size(1000, 42),
                Text = "Verifica: orientación y alineación; Padding; degradado invertido por RTL; trazos y remates completos; grosor limitado en espacios mínimos; hover, foco y deshabilitación; validaciones; pausa, reanudación, detención, redimensionamiento y cierre sin excepciones."
            };

            Controls.Add(titleLabel);
            Controls.Add(statusLabel);
            Controls.Add(previewPanel);
            Controls.Add(actionsPanel);
            Controls.Add(instructionsLabel);

            UpdateStatus();
        }

        private static SaraUI_Line CreateAlignmentLine(
            SaraUI_Line.LineAlignment alignment,
            Color color,
            Point location) {
            return new SaraUI_Line {
                Alignment = alignment,
                BackColor = Color.FromArgb(242, 242, 248),
                LineColor = color,
                LineWidth = 6,
                Location = location,
                Padding = new Padding(12, 8, 12, 8),
                Size = new Size(280, 54)
            };
        }

        private static Label CreateCaption(string text, Point location) {
            return new Label {
                Location = location,
                Size = new Size(90, 30),
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static Button CreateActionButton(string text) {
            return new Button {
                AutoEllipsis = true,
                Margin = new Padding(5),
                Size = new Size(140, 34),
                Text = text,
                UseVisualStyleBackColor = true
            };
        }

        private static TEnum NextValue<TEnum>(TEnum current)
            where TEnum:struct, Enum {
            Array values = Enum.GetValues(typeof(TEnum));
            int currentIndex = 0;

            for(int index = 0; index < values.Length; index++) {
                if(Equals(values.GetValue(index), current)) {
                    currentIndex = index;
                    break;
                }
            }

            int nextIndex = (currentIndex + 1) % values.Length;
            return (TEnum)values.GetValue(nextIndex)!;
        }
    }
}

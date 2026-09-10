using Sara_UI_Design.Animations;
using Sara_UI_Design.SaraControls;

namespace Sara_UI_Design.Demo {
    internal sealed class ShadowPanelDemoForm:Form {
        public ShadowPanelDemoForm() {
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(226, 228, 238);
            ClientSize = new Size(1000, 650);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            MinimumSize = new Size(860, 610);
            StartPosition = FormStartPosition.CenterParent;
            Text = "SaraUI_ShadowPanel - Pruebas de sombra y contenido";

            Label titleLabel = new Label {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font(Font, FontStyle.Bold),
                Location = new Point(24, 18),
                Size = new Size(952, 24),
                Text = "La sombra no debe recortarse ni ocupar el Padding solicitado por el contenido."
            };

            Label statusLabel = new Label {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(24, 44),
                Size = new Size(952, 38)
            };

            SaraUI_ShadowPanel shadowPanel = new SaraUI_ShadowPanel {
                AccessibleDescription = "Panel flexible con sombra configurable de la demostración.",
                AccessibleName = "Panel con sombra de demostración",
                AlignItems = SaraUI_FlexPanel.FlexAlignment.Center,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AnimationDuration = 900,
                AnimationEasing = SaraEasing.EaseInOutCubic,
                AnimationEnabled = true,
                BackColor = Color.White,
                BorderColor = Color.MediumSlateBlue,
                BorderRadius = 28,
                BorderThickness = 1,
                ChildSpacing = 12,
                Direction = SaraUI_FlexPanel.FlexDirection.Row,
                Justify = SaraUI_FlexPanel.JustifyContent.SpaceEvenly,
                Location = new Point(24, 90),
                Padding = new Padding(22),
                ShadowColor = Color.FromArgb(62, 52, 110),
                ShadowFocusScale = 0.72f,
                ShadowOffsetX = 8,
                ShadowOffsetY = 12,
                ShadowOpacity = 130,
                ShadowSize = 20,
                Size = new Size(620, 450),
                TabIndex = 0,
                WrapContents = SaraUI_FlexPanel.FlexWrap.Wrap
            };

            for(int index = 1; index <= 7; index++) {
                shadowPanel.Controls.Add(CreateContentItem(index));
            }

            FlowLayoutPanel actionsPanel = new FlowLayoutPanel {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                AutoScroll = true,
                BackColor = Color.FromArgb(240, 241, 248),
                FlowDirection = FlowDirection.LeftToRight,
                Location = new Point(666, 90),
                Padding = new Padding(10),
                Size = new Size(310, 450),
                WrapContents = true
            };

            Button shadowSizeButton = CreateActionButton("Difusión");
            Button opacityButton = CreateActionButton("Opacidad");
            Button offsetButton = CreateActionButton("Dirección sombra");
            Button focusButton = CreateActionButton("Suavidad");
            Button radiusButton = CreateActionButton("Radio");
            Button paddingButton = CreateActionButton("Padding");
            Button borderButton = CreateActionButton("Borde");
            Button directionButton = CreateActionButton("Fila / columna");
            Button pauseButton = CreateActionButton("Pausar");
            Button resumeButton = CreateActionButton("Reanudar");
            Button stopButton = CreateActionButton("Detener");

            int shadowSizeIndex = 2;
            int opacityIndex = 2;
            int offsetIndex = 0;
            int focusIndex = 1;
            int radiusIndex = 2;
            int paddingIndex = 1;
            bool borderVisible = true;
            int completedCount = 0;
            int canceledCount = 0;
            string lastAction = "Sin acciones";
            int[] shadowSizes = { 0, 10, 20, 32 };
            int[] opacities = { 0, 70, 130, 210 };
            Point[] offsets = {
                new Point(8, 12),
                new Point(18, 0),
                new Point(-14, 12),
                new Point(-12, -10)
            };
            float[] focusScales = { 0.5f, 0.72f, 0.9f };
            int[] radii = { 0, 14, 28, 56 };
            int[] contentPaddings = { 8, 22, 42 };

            void UpdateStatus() {
                Padding insets = shadowPanel.ShadowInsets;
                statusLabel.Text =
                    $"Sombra: {shadowPanel.ShadowSize}px / {shadowPanel.ShadowOpacity} | " +
                    $"Offset: ({shadowPanel.ShadowOffsetX}, {shadowPanel.ShadowOffsetY}) | " +
                    $"Reserva: L{insets.Left} T{insets.Top} R{insets.Right} B{insets.Bottom} | " +
                    $"Padding: {shadowPanel.Padding.Left}px | {shadowPanel.AnimationState} | " +
                    $"C:{completedCount} X:{canceledCount} | {lastAction}";
            }

            shadowSizeButton.Click += (_, _) => {
                shadowSizeIndex = NextIndex(shadowSizeIndex, shadowSizes.Length);
                shadowPanel.ShadowSize = shadowSizes[shadowSizeIndex];
                lastAction = "difusión modificada";
                UpdateStatus();
            };
            opacityButton.Click += (_, _) => {
                opacityIndex = NextIndex(opacityIndex, opacities.Length);
                shadowPanel.ShadowOpacity = opacities[opacityIndex];
                lastAction = "opacidad modificada";
                UpdateStatus();
            };
            offsetButton.Click += (_, _) => {
                offsetIndex = NextIndex(offsetIndex, offsets.Length);
                shadowPanel.SetShadowOffset(offsets[offsetIndex].X, offsets[offsetIndex].Y);
                lastAction = "dirección de la sombra modificada";
                UpdateStatus();
            };
            focusButton.Click += (_, _) => {
                focusIndex = NextIndex(focusIndex, focusScales.Length);
                shadowPanel.ShadowFocusScale = focusScales[focusIndex];
                lastAction = "suavidad modificada";
                UpdateStatus();
            };
            radiusButton.Click += (_, _) => {
                radiusIndex = NextIndex(radiusIndex, radii.Length);
                shadowPanel.BorderRadius = radii[radiusIndex];
                lastAction = "radio modificado";
                UpdateStatus();
            };
            paddingButton.Click += (_, _) => {
                paddingIndex = NextIndex(paddingIndex, contentPaddings.Length);
                shadowPanel.Padding = new Padding(contentPaddings[paddingIndex]);
                lastAction = "Padding de contenido modificado";
                UpdateStatus();
            };
            borderButton.Click += (_, _) => {
                borderVisible = !borderVisible;
                shadowPanel.BorderThickness = borderVisible ? 2 : 0;
                lastAction = borderVisible ? "borde visible" : "borde oculto";
                UpdateStatus();
            };
            directionButton.Click += (_, _) => {
                shadowPanel.Direction = shadowPanel.Direction == SaraUI_FlexPanel.FlexDirection.Row
                    ? SaraUI_FlexPanel.FlexDirection.Column
                    : SaraUI_FlexPanel.FlexDirection.Row;
                lastAction = "dirección de contenido modificada";
                UpdateStatus();
            };
            pauseButton.Click += (_, _) => {
                lastAction = shadowPanel.PauseAnimation()
                    ? "animación pausada"
                    : "sin animación activa";
                UpdateStatus();
            };
            resumeButton.Click += (_, _) => {
                lastAction = shadowPanel.ResumeAnimation()
                    ? "animación reanudada"
                    : "sin pausa activa";
                UpdateStatus();
            };
            stopButton.Click += (_, _) => {
                lastAction = shadowPanel.StopAnimation()
                    ? "animación detenida"
                    : "sin animación activa";
                UpdateStatus();
            };

            shadowPanel.AnimationCompleted += (_, _) => {
                completedCount++;
                UpdateStatus();
            };
            shadowPanel.AnimationCanceled += (_, _) => {
                canceledCount++;
                UpdateStatus();
            };
            shadowPanel.AnimationStateChanged += (_, _) => UpdateStatus();

            actionsPanel.Controls.Add(shadowSizeButton);
            actionsPanel.Controls.Add(opacityButton);
            actionsPanel.Controls.Add(offsetButton);
            actionsPanel.Controls.Add(focusButton);
            actionsPanel.Controls.Add(radiusButton);
            actionsPanel.Controls.Add(paddingButton);
            actionsPanel.Controls.Add(borderButton);
            actionsPanel.Controls.Add(directionButton);
            actionsPanel.Controls.Add(pauseButton);
            actionsPanel.Controls.Add(resumeButton);
            actionsPanel.Controls.Add(stopButton);

            Label instructionsLabel = new Label {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(24, 558),
                Size = new Size(952, 68),
                Text = "Verifica: sombra completa en los cuatro lados y con offsets negativos; contenido siempre dentro de la superficie; Padding independiente; borde y radios correctos; distribución animada sin superposición; pausa, reanudación y detención; redimensionamiento y cierre sin excepciones."
            };

            Controls.Add(titleLabel);
            Controls.Add(statusLabel);
            Controls.Add(shadowPanel);
            Controls.Add(actionsPanel);
            Controls.Add(instructionsLabel);

            UpdateStatus();
        }

        private static Button CreateContentItem(int number) {
            int hueOffset = (number * 18) % 90;
            return new Button {
                AccessibleName = $"Contenido {number}",
                BackColor = Color.FromArgb(92 + hueOffset, 78, 210 - (hueOffset / 2)),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Margin = new Padding(6),
                Size = new Size(112 + ((number % 2) * 26), 52),
                Text = $"Contenido {number}",
                UseVisualStyleBackColor = false
            };
        }

        private static Button CreateActionButton(string text) {
            return new Button {
                Margin = new Padding(3),
                Size = new Size(132, 36),
                Text = text,
                UseVisualStyleBackColor = true
            };
        }

        private static int NextIndex(int currentIndex, int count) {
            return (currentIndex + 1) % count;
        }
    }
}

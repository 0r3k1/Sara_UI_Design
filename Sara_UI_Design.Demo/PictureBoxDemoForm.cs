using System.Drawing.Drawing2D;
using Sara_UI_Design.Animations;
using Sara_UI_Design.SaraControls;

namespace Sara_UI_Design.Demo {
    internal sealed class PictureBoxDemoForm:Form {
        public PictureBoxDemoForm() {
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(232, 233, 242);
            ClientSize = new Size(1050, 700);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            MinimumSize = new Size(920, 650);
            StartPosition = FormStartPosition.CenterParent;
            Text = "SaraUI_PictureBox - Pruebas de imagen y región";

            Bitmap primaryImage = CreateSampleImage(
                Color.MediumSlateBlue,
                Color.DeepPink,
                "SARA");
            Bitmap alternateImage = CreateSampleImage(
                Color.Teal,
                Color.Goldenrod,
                "UI");

            Label titleLabel = new Label {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(24, 18),
                Size = new Size(1002, 24),
                Text = "Imágenes circulares y rectangulares con región, borde y estados seguros."
            };

            Label statusLabel = new Label {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(24, 44),
                Size = new Size(1002, 38)
            };

            Panel previewPanel = new Panel {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White,
                Location = new Point(24, 88),
                Size = new Size(630, 510)
            };

            SaraUI_PictureBox primaryPicture = new SaraUI_PictureBox {
                AccessibleDescription = "Imagen principal utilizada para probar forma, foco y estados.",
                AccessibleName = "Imagen principal de demostración",
                AnimationDuration = 700,
                AnimationEasing = SaraEasing.EaseInOutCubic,
                BackColor = Color.FromArgb(238, 238, 248),
                BorderColor = Color.MediumSlateBlue,
                BorderColor2 = Color.DeepPink,
                BorderSize = 8,
                FocusBorderColor = Color.Gold,
                FocusBorderColor2 = Color.OrangeRed,
                HoverBorderColor = Color.Cyan,
                HoverBorderColor2 = Color.MediumSlateBlue,
                Image = primaryImage,
                Location = new Point(36, 38),
                Size = new Size(250, 250),
                SizeMode = PictureBoxSizeMode.StretchImage,
                TabIndex = 0,
                TabStop = true
            };

            SaraUI_PictureBox roundedPicture = new SaraUI_PictureBox {
                AccessibleDescription = "Imagen rectangular usada para comprobar radios y SizeMode.",
                AccessibleName = "Imagen rectangular de demostración",
                BackColor = Color.FromArgb(238, 238, 248),
                BorderColor = Color.Teal,
                BorderColor2 = Color.Goldenrod,
                BorderRadius = 34,
                BorderSize = 7,
                Image = alternateImage,
                IsCircular = false,
                Location = new Point(330, 38),
                Size = new Size(250, 170),
                SizeMode = PictureBoxSizeMode.Zoom,
                TabIndex = 1,
                TabStop = true
            };

            SaraUI_PictureBox smallPicture = new SaraUI_PictureBox {
                AccessibleDescription = "Imagen pequeña con un borde solicitado mayor que su superficie.",
                AccessibleName = "Imagen pequeña de seguridad",
                BackColor = Color.FromArgb(238, 238, 248),
                BorderColor = Color.DarkSlateBlue,
                BorderColor2 = Color.HotPink,
                BorderRadius = 100,
                BorderSize = 80,
                Image = primaryImage,
                IsCircular = false,
                Location = new Point(330, 260),
                Size = new Size(90, 58),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Label primaryLabel = CreateCaption(
                "Principal: pasa el ratón y haz clic para darle foco.",
                new Point(36, 306),
                new Size(260, 46));
            Label roundedLabel = CreateCaption(
                "Rectangular: prueba radio y ajuste de imagen.",
                new Point(330, 216),
                new Size(250, 38));
            Label smallLabel = CreateCaption(
                "Borde extremo limitado al tamaño disponible.",
                new Point(330, 326),
                new Size(250, 38));

            previewPanel.Controls.Add(primaryPicture);
            previewPanel.Controls.Add(roundedPicture);
            previewPanel.Controls.Add(smallPicture);
            previewPanel.Controls.Add(primaryLabel);
            previewPanel.Controls.Add(roundedLabel);
            previewPanel.Controls.Add(smallLabel);

            FlowLayoutPanel actionsPanel = new FlowLayoutPanel {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                AutoScroll = true,
                BackColor = Color.FromArgb(244, 244, 249),
                Location = new Point(676, 88),
                Padding = new Padding(9),
                Size = new Size(350, 510),
                WrapContents = true
            };

            Button shapeButton = CreateActionButton("Forma: Circular");
            Button ratioButton = CreateActionButton("Cuadrado: Sí");
            Button borderButton = CreateActionButton("Borde: 8px");
            Button radiusButton = CreateActionButton("Radio: 34px");
            Button gradientButton = CreateActionButton("Ángulo: 50°");
            Button lineButton = CreateActionButton("Línea: Solid");
            Button capButton = CreateActionButton("Remate: Flat");
            Button sizeModeButton = CreateActionButton("Ajuste: StretchImage");
            Button clipButton = CreateActionButton("Recorte: Sí");
            Button imageButton = CreateActionButton("Imagen: SARA");
            Button enabledButton = CreateActionButton("Estado: Habilitado");
            Button smallSizeButton = CreateActionButton("Tamaño pequeño");
            Button validationButton = CreateActionButton("Validaciones");
            Button pauseButton = CreateActionButton("Pausar");
            Button resumeButton = CreateActionButton("Reanudar");
            Button stopButton = CreateActionButton("Detener");

            int borderIndex = 2;
            int radiusIndex = 2;
            int angleIndex = 0;
            int lineIndex = 0;
            int capIndex = 0;
            int sizeModeIndex = 0;
            bool alternateSelected = false;
            bool compactSmallPicture = false;
            int completedCount = 0;
            int canceledCount = 0;
            string lastAction = "Sin acciones";
            int[] borderSizes = { 0, 2, 8, 24, 400 };
            int[] radii = { 0, 12, 34, 90, 400 };
            float[] angles = { 50f, 90f, 180f, 315f };
            DashStyle[] lineStyles = {
                DashStyle.Solid,
                DashStyle.Dash,
                DashStyle.Dot,
                DashStyle.DashDot,
                DashStyle.DashDotDot
            };
            DashCap[] capStyles = { DashCap.Flat, DashCap.Round, DashCap.Triangle };
            PictureBoxSizeMode[] sizeModes = {
                PictureBoxSizeMode.StretchImage,
                PictureBoxSizeMode.Zoom,
                PictureBoxSizeMode.CenterImage,
                PictureBoxSizeMode.Normal
            };

            void UpdateStatus() {
                statusLabel.Text =
                    $"Forma: {(primaryPicture.IsCircular ? "Circular" : "Redondeada")} | " +
                    $"Tamaño: {primaryPicture.Width}x{primaryPicture.Height} | " +
                    $"Recorte: {primaryPicture.ClipToShape} | {primaryPicture.SizeMode} | " +
                    $"{primaryPicture.VisualState} / {primaryPicture.AnimationState} | " +
                    $"C:{completedCount} X:{canceledCount} | {lastAction}";
            }

            shapeButton.Click += (_, _) => {
                primaryPicture.IsCircular = !primaryPicture.IsCircular;

                if(primaryPicture.IsCircular) {
                    primaryPicture.Size = new Size(250, 250);
                } else {
                    primaryPicture.Size = new Size(280, 210);
                }

                shapeButton.Text = primaryPicture.IsCircular
                    ? "Forma: Circular"
                    : "Forma: Redondeada";
                lastAction = "forma modificada";
                UpdateStatus();
            };
            ratioButton.Click += (_, _) => {
                primaryPicture.MaintainCircularAspectRatio =
                    !primaryPicture.MaintainCircularAspectRatio;

                if(primaryPicture.IsCircular && !primaryPicture.MaintainCircularAspectRatio) {
                    primaryPicture.Size = new Size(280, 190);
                }

                ratioButton.Text = primaryPicture.MaintainCircularAspectRatio
                    ? "Cuadrado: Sí"
                    : "Cuadrado: No";
                lastAction = "restricción de proporción modificada";
                UpdateStatus();
            };
            borderButton.Click += (_, _) => {
                borderIndex = NextIndex(borderIndex, borderSizes.Length);
                primaryPicture.BorderSize = borderSizes[borderIndex];
                borderButton.Text = $"Borde: {primaryPicture.BorderSize}px";
                lastAction = "grosor modificado";
                UpdateStatus();
            };
            radiusButton.Click += (_, _) => {
                radiusIndex = NextIndex(radiusIndex, radii.Length);
                roundedPicture.BorderRadius = radii[radiusIndex];
                radiusButton.Text = $"Radio: {roundedPicture.BorderRadius}px";
                lastAction = "radio rectangular modificado";
                UpdateStatus();
            };
            gradientButton.Click += (_, _) => {
                angleIndex = NextIndex(angleIndex, angles.Length);
                primaryPicture.GradientAngle = angles[angleIndex];
                gradientButton.Text = $"Ángulo: {primaryPicture.GradientAngle:0}°";
                lastAction = "ángulo del degradado modificado";
                UpdateStatus();
            };
            lineButton.Click += (_, _) => {
                lineIndex = NextIndex(lineIndex, lineStyles.Length);
                primaryPicture.BorderLineStyle = lineStyles[lineIndex];
                lineButton.Text = $"Línea: {primaryPicture.BorderLineStyle}";
                lastAction = "estilo de línea modificado";
                UpdateStatus();
            };
            capButton.Click += (_, _) => {
                capIndex = NextIndex(capIndex, capStyles.Length);
                primaryPicture.BorderCapStyle = capStyles[capIndex];
                capButton.Text = $"Remate: {primaryPicture.BorderCapStyle}";
                lastAction = "remate modificado";
                UpdateStatus();
            };
            sizeModeButton.Click += (_, _) => {
                sizeModeIndex = NextIndex(sizeModeIndex, sizeModes.Length);
                roundedPicture.SizeMode = sizeModes[sizeModeIndex];
                sizeModeButton.Text = $"Ajuste: {roundedPicture.SizeMode}";
                lastAction = "SizeMode modificado";
                UpdateStatus();
            };
            clipButton.Click += (_, _) => {
                primaryPicture.ClipToShape = !primaryPicture.ClipToShape;
                clipButton.Text = primaryPicture.ClipToShape
                    ? "Recorte: Sí"
                    : "Recorte: No";
                lastAction = "recorte modificado";
                UpdateStatus();
            };
            imageButton.Click += (_, _) => {
                alternateSelected = !alternateSelected;
                primaryPicture.Image = alternateSelected ? alternateImage : primaryImage;
                imageButton.Text = alternateSelected ? "Imagen: UI" : "Imagen: SARA";
                lastAction = "imagen reemplazada sin transferir propiedad";
                UpdateStatus();
            };
            enabledButton.Click += (_, _) => {
                primaryPicture.Enabled = !primaryPicture.Enabled;
                enabledButton.Text = primaryPicture.Enabled
                    ? "Estado: Habilitado"
                    : "Estado: Deshabilitado";
                lastAction = "estado habilitado modificado";
                UpdateStatus();
            };
            smallSizeButton.Click += (_, _) => {
                compactSmallPicture = !compactSmallPicture;
                smallPicture.Size = compactSmallPicture
                    ? new Size(10, 8)
                    : new Size(90, 58);
                smallSizeButton.Text = compactSmallPicture
                    ? "Tamaño normal"
                    : "Tamaño pequeño";
                lastAction = "tamaño extremo comprobado";
                UpdateStatus();
            };
            validationButton.Click += (_, _) => {
                int previousBorderSize = primaryPicture.BorderSize;
                float previousAngle = primaryPicture.GradientAngle;
                DashStyle previousLineStyle = primaryPicture.BorderLineStyle;
                bool negativeRejected = false;
                bool angleRejected = false;
                bool customStyleRejected = false;

                try {
                    primaryPicture.BorderSize = -1;
                } catch(ArgumentOutOfRangeException) {
                    negativeRejected = true;
                }

                try {
                    primaryPicture.GradientAngle = float.NaN;
                } catch(ArgumentOutOfRangeException) {
                    angleRejected = true;
                }

                try {
                    primaryPicture.BorderLineStyle = DashStyle.Custom;
                } catch(NotSupportedException) {
                    customStyleRejected = true;
                }

                lastAction = negativeRejected && angleRejected && customStyleRejected &&
                    primaryPicture.BorderSize == previousBorderSize &&
                    Math.Abs(primaryPicture.GradientAngle - previousAngle) < float.Epsilon &&
                    primaryPicture.BorderLineStyle == previousLineStyle
                        ? "valores inválidos rechazados sin alterar el estado"
                        : "ERROR: una validación no conservó el estado";
                UpdateStatus();
            };
            pauseButton.Click += (_, _) => {
                lastAction = primaryPicture.PauseAnimation()
                    ? "animación pausada"
                    : "sin animación activa";
                UpdateStatus();
            };
            resumeButton.Click += (_, _) => {
                lastAction = primaryPicture.ResumeAnimation()
                    ? "animación reanudada"
                    : "sin pausa activa";
                UpdateStatus();
            };
            stopButton.Click += (_, _) => {
                lastAction = primaryPicture.StopAnimation()
                    ? "animación detenida"
                    : "sin animación activa";
                UpdateStatus();
            };

            primaryPicture.VisualStateChanged += (_, _) => UpdateStatus();
            primaryPicture.AnimationCompleted += (_, _) => {
                completedCount++;
                UpdateStatus();
            };
            primaryPicture.AnimationCanceled += (_, _) => {
                canceledCount++;
                UpdateStatus();
            };
            primaryPicture.AnimationStateChanged += (_, _) => UpdateStatus();
            primaryPicture.SizeChanged += (_, _) => UpdateStatus();

            actionsPanel.Controls.Add(shapeButton);
            actionsPanel.Controls.Add(ratioButton);
            actionsPanel.Controls.Add(borderButton);
            actionsPanel.Controls.Add(radiusButton);
            actionsPanel.Controls.Add(gradientButton);
            actionsPanel.Controls.Add(lineButton);
            actionsPanel.Controls.Add(capButton);
            actionsPanel.Controls.Add(sizeModeButton);
            actionsPanel.Controls.Add(clipButton);
            actionsPanel.Controls.Add(imageButton);
            actionsPanel.Controls.Add(enabledButton);
            actionsPanel.Controls.Add(smallSizeButton);
            actionsPanel.Controls.Add(validationButton);
            actionsPanel.Controls.Add(pauseButton);
            actionsPanel.Controls.Add(resumeButton);
            actionsPanel.Controls.Add(stopButton);

            Label instructionsLabel = new Label {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(24, 614),
                Size = new Size(1002, 62),
                Text = "Verifica: recorte circular y redondeado sin esquinas residuales; restricción cuadrada opcional; borde sólido y discontinuo completo; degradado y radios extremos; SizeMode nativo; hover, foco y deshabilitado animados; valores inválidos rechazados; imagen reemplazable; tamaños mínimos, pausa, reanudación, detención y cierre sin excepciones."
            };

            Controls.Add(titleLabel);
            Controls.Add(statusLabel);
            Controls.Add(previewPanel);
            Controls.Add(actionsPanel);
            Controls.Add(instructionsLabel);

            FormClosed += (_, _) => {
                primaryPicture.Image = null;
                roundedPicture.Image = null;
                smallPicture.Image = null;
                primaryImage.Dispose();
                alternateImage.Dispose();
            };

            UpdateStatus();
        }

        private static Label CreateCaption(string text, Point location, Size size) {
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
                Size = new Size(155, 36),
                Text = text,
                UseVisualStyleBackColor = true
            };
        }

        private static Bitmap CreateSampleImage(Color startColor, Color endColor, string text) {
            Bitmap bitmap = new Bitmap(600, 400);

            using(Graphics graphics = Graphics.FromImage(bitmap))
            using(LinearGradientBrush backgroundBrush = new LinearGradientBrush(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                startColor,
                endColor,
                35f))
            using(SolidBrush accentBrush = new SolidBrush(Color.FromArgb(105, Color.White)))
            using(SolidBrush textBrush = new SolidBrush(Color.White))
            using(Font textFont = new Font("Segoe UI", 54f, FontStyle.Bold, GraphicsUnit.Pixel)) {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.FillRectangle(backgroundBrush, 0, 0, bitmap.Width, bitmap.Height);
                graphics.FillEllipse(accentBrush, -70, -80, 300, 300);
                graphics.FillEllipse(accentBrush, 410, 180, 260, 260);
                SizeF textSize = graphics.MeasureString(text, textFont);
                graphics.DrawString(
                    text,
                    textFont,
                    textBrush,
                    (bitmap.Width - textSize.Width) / 2f,
                    (bitmap.Height - textSize.Height) / 2f);
            }

            return bitmap;
        }

        private static int NextIndex(int currentIndex, int count) {
            return (currentIndex + 1) % count;
        }
    }
}

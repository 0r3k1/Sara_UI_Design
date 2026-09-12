using Sara_UI_Design.Animations;
using Sara_UI_Design.SaraControls;

namespace Sara_UI_Design.Demo {
    internal sealed class GridPanelDemoForm:Form {
        public GridPanelDemoForm() {
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(232, 233, 242);
            ClientSize = new Size(1050, 700);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            MinimumSize = new Size(900, 650);
            StartPosition = FormStartPosition.CenterParent;
            Text = "SaraUI_GridPanel - Pruebas de cuadrícula";

            Label titleLabel = new Label {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(24, 18),
                Size = new Size(1002, 24),
                Text = "Cuadrícula con píxeles, fracciones, posiciones, spans y flujo automático."
            };

            Label statusLabel = new Label {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(24, 44),
                Size = new Size(1002, 40)
            };

            SaraUI_GridPanel gridPanel = new SaraUI_GridPanel {
                AccessibleDescription = "Distribuye los elementos de la demostración en una cuadrícula.",
                AccessibleName = "Cuadrícula de demostración",
                AlignItems = SaraUI_GridPanel.GridItemAlignment.Stretch,
                AllowFormDrag = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AnimationDuration = 900,
                AnimationEasing = SaraEasing.EaseInOutCubic,
                AnimationEnabled = true,
                BackColor = Color.White,
                BorderRadius = 20,
                ColumnGap = 12,
                ColumnsConfig = "120px, 1fr, 2fr",
                JustifyItems = SaraUI_GridPanel.GridItemAlignment.Stretch,
                Location = new Point(24, 90),
                Padding = new Padding(18),
                RowGap = 12,
                RowsConfig = "80px, 1fr, 1fr",
                Size = new Size(660, 510),
                TabIndex = 0
            };

            Label dockedLabel = new Label {
                BackColor = Color.FromArgb(214, 216, 234),
                Dock = DockStyle.Bottom,
                Height = 28,
                Text = "Dock.Bottom permanece bajo el layout nativo.",
                TextAlign = ContentAlignment.MiddleCenter
            };

            Button headerItem = CreateGridItem("Encabezado: span de 3 columnas", Color.MediumSlateBlue);
            Button sideItem = CreateGridItem("Panel lateral\nspan de 2 filas", Color.DarkSlateBlue);
            Button positionedItem = CreateGridItem("Posición asociada", Color.Teal);
            Button legacyItem = CreateGridItem("Tag histórico 1,2", Color.SteelBlue);
            Button automaticItem1 = CreateGridItem("Automático A", Color.IndianRed);
            Button automaticItem2 = CreateGridItem("Automático B", Color.DarkGoldenrod);
            string consumerTag = "Datos pertenecientes al consumidor";
            positionedItem.Tag = consumerTag;
            legacyItem.Tag = "1,2";

            SaraUI_GridPanel.SetGridPosition(headerItem, 0, 0, 1, 3);
            SaraUI_GridPanel.SetGridPosition(sideItem, 1, 0, 2, 1);
            SaraUI_GridPanel.SetGridPosition(positionedItem, 1, 1);

            gridPanel.Controls.Add(dockedLabel);
            gridPanel.Controls.Add(headerItem);
            gridPanel.Controls.Add(sideItem);
            gridPanel.Controls.Add(positionedItem);
            gridPanel.Controls.Add(legacyItem);
            gridPanel.Controls.Add(automaticItem1);
            gridPanel.Controls.Add(automaticItem2);

            FlowLayoutPanel actionsPanel = new FlowLayoutPanel {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                AutoScroll = true,
                BackColor = Color.FromArgb(244, 244, 249),
                Location = new Point(706, 90),
                Padding = new Padding(10),
                Size = new Size(320, 510),
                WrapContents = true
            };

            Button templateButton = CreateActionButton("Plantilla");
            Button gapsButton = CreateActionButton("Separaciones");
            Button horizontalAlignButton = CreateActionButton("Horizontal: Stretch");
            Button verticalAlignButton = CreateActionButton("Vertical: Stretch");
            Button rightToLeftButton = CreateActionButton("RTL: No");
            Button paddingButton = CreateActionButton("Padding");
            Button radiusButton = CreateActionButton("Radio");
            Button positionButton = CreateActionButton("Mover asociado");
            Button clearPositionButton = CreateActionButton("Flujo automático");
            Button overflowButton = CreateActionButton("Agregar excedente");
            Button invalidButton = CreateActionButton("Configuración inválida");
            Button dragButton = CreateActionButton("Arrastre: Sí");
            Button pauseButton = CreateActionButton("Pausar");
            Button resumeButton = CreateActionButton("Reanudar");
            Button stopButton = CreateActionButton("Detener");

            bool alternateTemplate = false;
            bool alternateGaps = false;
            bool alternatePosition = false;
            bool alternatePadding = false;
            bool rounded = true;
            Button? overflowItem = null;
            int completedCount = 0;
            int canceledCount = 0;
            string lastAction = "Sin acciones";

            void UpdateStatus() {
                string columnSizes = string.Join("/", gridPanel.GetCalculatedColumnSizes());
                string rowSizes = string.Join("/", gridPanel.GetCalculatedRowSizes());
                bool tagPreserved = Equals(positionedItem.Tag, consumerTag);
                statusLabel.Text =
                    $"Columnas: {columnSizes}px | Filas: {rowSizes}px | " +
                    $"Sin celda: {gridPanel.UnplacedControlCount} | Tag conservado: {tagPreserved} | " +
                    $"{gridPanel.AnimationState} | C:{completedCount} X:{canceledCount} | {lastAction}";
            }

            templateButton.Click += (_, _) => {
                alternateTemplate = !alternateTemplate;
                gridPanel.SetGridTemplate(
                    alternateTemplate ? "1fr, 1fr, 1fr" : "120px, 1fr, 2fr",
                    alternateTemplate ? "1fr, 2fr, 1fr" : "80px, 1fr, 1fr");
                lastAction = "plantilla modificada";
                UpdateStatus();
            };
            gapsButton.Click += (_, _) => {
                alternateGaps = !alternateGaps;
                gridPanel.SetGaps(alternateGaps ? 28 : 12, alternateGaps ? 22 : 12);
                lastAction = "separaciones modificadas";
                UpdateStatus();
            };
            horizontalAlignButton.Click += (_, _) => {
                gridPanel.JustifyItems = NextValue(gridPanel.JustifyItems);
                horizontalAlignButton.Text = $"Horizontal: {gridPanel.JustifyItems}";
                lastAction = $"alineación horizontal: {gridPanel.JustifyItems}";
                UpdateStatus();
            };
            verticalAlignButton.Click += (_, _) => {
                gridPanel.AlignItems = NextValue(gridPanel.AlignItems);
                verticalAlignButton.Text = $"Vertical: {gridPanel.AlignItems}";
                lastAction = $"alineación vertical: {gridPanel.AlignItems}";
                UpdateStatus();
            };
            rightToLeftButton.Click += (_, _) => {
                bool enable = gridPanel.RightToLeft != RightToLeft.Yes;
                gridPanel.RightToLeft = enable ? RightToLeft.Yes : RightToLeft.No;
                rightToLeftButton.Text = enable ? "RTL: Sí" : "RTL: No";
                lastAction = "dirección de lectura modificada";
                UpdateStatus();
            };
            paddingButton.Click += (_, _) => {
                alternatePadding = !alternatePadding;
                gridPanel.Padding = new Padding(alternatePadding ? 36 : 18);
                lastAction = "Padding modificado";
                UpdateStatus();
            };
            radiusButton.Click += (_, _) => {
                rounded = !rounded;
                gridPanel.BorderRadius = rounded ? 20 : 0;
                lastAction = rounded ? "esquinas redondeadas" : "esquinas rectas";
                UpdateStatus();
            };
            positionButton.Click += (_, _) => {
                alternatePosition = !alternatePosition;
                SaraUI_GridPanel.SetGridPosition(
                    positionedItem,
                    alternatePosition ? 2 : 1,
                    1);
                lastAction = "posición asociada modificada";
                UpdateStatus();
            };
            clearPositionButton.Click += (_, _) => {
                bool removed = SaraUI_GridPanel.ClearGridPosition(positionedItem);
                lastAction = removed
                    ? "control devuelto al flujo automático"
                    : "el control ya estaba en flujo automático";
                UpdateStatus();
            };
            overflowButton.Click += (_, _) => {
                if(overflowItem == null) {
                    overflowItem = CreateGridItem("Sin celda disponible", Color.DimGray);
                    gridPanel.Controls.Add(overflowItem);
                    overflowButton.Text = "Quitar excedente";
                    lastAction = "excedente agregado fuera de la cuadrícula";
                } else {
                    gridPanel.Controls.Remove(overflowItem);
                    overflowItem.Dispose();
                    overflowItem = null;
                    overflowButton.Text = "Agregar excedente";
                    lastAction = "control excedente retirado";
                }

                UpdateStatus();
            };
            invalidButton.Click += (_, _) => {
                string previousConfiguration = gridPanel.ColumnsConfig;

                try {
                    gridPanel.ColumnsConfig = "1fr, columna-inválida";
                    lastAction = "ERROR: se aceptó una configuración inválida";
                } catch(FormatException) {
                    lastAction = gridPanel.ColumnsConfig == previousConfiguration
                        ? "error detectado y configuración conservada"
                        : "ERROR: cambió la configuración anterior";
                }

                UpdateStatus();
            };
            dragButton.Click += (_, _) => {
                gridPanel.AllowFormDrag = !gridPanel.AllowFormDrag;
                dragButton.Text = gridPanel.AllowFormDrag ? "Arrastre: Sí" : "Arrastre: No";
                lastAction = "arrastre del formulario modificado";
                UpdateStatus();
            };
            pauseButton.Click += (_, _) => {
                lastAction = gridPanel.PauseAnimation()
                    ? "animación pausada"
                    : "sin animación activa";
                UpdateStatus();
            };
            resumeButton.Click += (_, _) => {
                lastAction = gridPanel.ResumeAnimation()
                    ? "animación reanudada"
                    : "sin pausa activa";
                UpdateStatus();
            };
            stopButton.Click += (_, _) => {
                lastAction = gridPanel.StopAnimation()
                    ? "animación detenida"
                    : "sin animación activa";
                UpdateStatus();
            };

            gridPanel.AnimationCompleted += (_, _) => {
                completedCount++;
                UpdateStatus();
            };
            gridPanel.AnimationCanceled += (_, _) => {
                canceledCount++;
                UpdateStatus();
            };
            gridPanel.AnimationStateChanged += (_, _) => UpdateStatus();
            gridPanel.SizeChanged += (_, _) => UpdateStatus();

            actionsPanel.Controls.Add(templateButton);
            actionsPanel.Controls.Add(gapsButton);
            actionsPanel.Controls.Add(horizontalAlignButton);
            actionsPanel.Controls.Add(verticalAlignButton);
            actionsPanel.Controls.Add(rightToLeftButton);
            actionsPanel.Controls.Add(paddingButton);
            actionsPanel.Controls.Add(radiusButton);
            actionsPanel.Controls.Add(positionButton);
            actionsPanel.Controls.Add(clearPositionButton);
            actionsPanel.Controls.Add(overflowButton);
            actionsPanel.Controls.Add(invalidButton);
            actionsPanel.Controls.Add(dragButton);
            actionsPanel.Controls.Add(pauseButton);
            actionsPanel.Controls.Add(resumeButton);
            actionsPanel.Controls.Add(stopButton);

            Label instructionsLabel = new Label {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(24, 618),
                Size = new Size(1002, 60),
                Text = "Verifica: tamaños fijos y fr exactos; spans y flujo automático sin superposición; Margin, Padding, alineaciones y RTL; Tag conservado; rechazo de sintaxis inválida; control excedente contabilizado; arrastre desde un área vacía; pausa, reanudación, detención, redimensionamiento y cierre seguro."
            };

            Controls.Add(titleLabel);
            Controls.Add(statusLabel);
            Controls.Add(gridPanel);
            Controls.Add(actionsPanel);
            Controls.Add(instructionsLabel);

            UpdateStatus();
        }

        private static Button CreateGridItem(string text, Color color) {
            return new Button {
                AccessibleName = text.Replace("\n", " "),
                BackColor = color,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Margin = new Padding(6),
                Size = new Size(130, 54),
                Text = text,
                UseVisualStyleBackColor = false
            };
        }

        private static Button CreateActionButton(string text) {
            return new Button {
                Margin = new Padding(3),
                Size = new Size(140, 36),
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

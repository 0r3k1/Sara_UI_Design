using Sara_UI_Design.Animations;
using Sara_UI_Design.SaraControls;

namespace Sara_UI_Design.Demo {
    internal sealed class FlexPanelDemoForm:Form {
        public FlexPanelDemoForm() {
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 620);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            MinimumSize = new Size(780, 560);
            StartPosition = FormStartPosition.CenterParent;
            Text = "SaraUI_FlexPanel - Pruebas de distribución";

            Label statusLabel = new Label {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(20, 18),
                Size = new Size(860, 28)
            };

            SaraUI_FlexPanel flexPanel = new SaraUI_FlexPanel {
                AccessibleDescription = "Organiza dinámicamente los elementos de la demostración.",
                AccessibleName = "Panel flexible de demostración",
                AlignItems = SaraUI_FlexPanel.FlexAlignment.Center,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AnimationDuration = 900,
                AnimationEasing = SaraEasing.EaseInOutCubic,
                AnimationEnabled = true,
                BackColor = Color.FromArgb(238, 238, 248),
                BorderRadius = 18,
                ChildSpacing = 12,
                Direction = SaraUI_FlexPanel.FlexDirection.Row,
                Justify = SaraUI_FlexPanel.JustifyContent.SpaceEvenly,
                Location = new Point(20, 52),
                Padding = new Padding(18),
                Size = new Size(860, 380),
                TabIndex = 0,
                WrapContents = SaraUI_FlexPanel.FlexWrap.Wrap
            };

            Label dockedLabel = new Label {
                BackColor = Color.FromArgb(218, 218, 238),
                Dock = DockStyle.Bottom,
                Height = 28,
                Text = "Este elemento usa Dock.Bottom y queda bajo el layout nativo.",
                TextAlign = ContentAlignment.MiddleCenter
            };
            flexPanel.Controls.Add(dockedLabel);

            int nextItemNumber = 1;
            int completedCount = 0;
            int canceledCount = 0;
            string lastAction = "Sin acciones";

            for(int index = 0; index < 6; index++) {
                flexPanel.Controls.Add(CreateFlexItem(nextItemNumber++));
            }

            FlowLayoutPanel actionsPanel = new FlowLayoutPanel {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(20, 452),
                Size = new Size(860, 80),
                WrapContents = true
            };

            Button directionButton = CreateActionButton("Fila");
            Button justifyButton = CreateActionButton("Distribución");
            Button wrapButton = CreateActionButton("Wrap: Sí");
            Button alignButton = CreateActionButton("Alinear");
            Button rightToLeftButton = CreateActionButton("RTL: No");
            Button addButton = CreateActionButton("Agregar");
            Button removeButton = CreateActionButton("Eliminar");
            Button pauseButton = CreateActionButton("Pausar");
            Button resumeButton = CreateActionButton("Reanudar");
            Button stopButton = CreateActionButton("Detener");

            int CountFlexItems() {
                int count = 0;

                foreach(Control control in flexPanel.Controls) {
                    if(control.Dock == DockStyle.None) {
                        count++;
                    }
                }

                return count;
            }

            void UpdateStatus() {
                statusLabel.Text =
                    $"Dirección: {flexPanel.Direction} | Distribución: {flexPanel.Justify} | " +
                    $"Alineación: {flexPanel.AlignItems} | Wrap: {flexPanel.WrapContents} | " +
                    $"Elementos: {CountFlexItems()} | Animación: {flexPanel.AnimationState} | " +
                    $"Completadas: {completedCount} | Canceladas: {canceledCount} | {lastAction}";
            }

            directionButton.Click += (_, _) => {
                flexPanel.Direction = flexPanel.Direction == SaraUI_FlexPanel.FlexDirection.Row
                    ? SaraUI_FlexPanel.FlexDirection.Column
                    : SaraUI_FlexPanel.FlexDirection.Row;
                directionButton.Text = flexPanel.Direction == SaraUI_FlexPanel.FlexDirection.Row
                    ? "Fila"
                    : "Columna";
                lastAction = "dirección cambiada";
                UpdateStatus();
            };

            justifyButton.Click += (_, _) => {
                flexPanel.Justify = NextValue(flexPanel.Justify);
                justifyButton.Text = flexPanel.Justify.ToString();
                lastAction = "distribución cambiada";
                UpdateStatus();
            };

            wrapButton.Click += (_, _) => {
                flexPanel.WrapContents =
                    flexPanel.WrapContents == SaraUI_FlexPanel.FlexWrap.Wrap
                        ? SaraUI_FlexPanel.FlexWrap.NoWrap
                        : SaraUI_FlexPanel.FlexWrap.Wrap;
                wrapButton.Text = flexPanel.WrapContents == SaraUI_FlexPanel.FlexWrap.Wrap
                    ? "Wrap: Sí"
                    : "Wrap: No";
                lastAction = "envoltura cambiada";
                UpdateStatus();
            };

            alignButton.Click += (_, _) => {
                flexPanel.AlignItems = NextValue(flexPanel.AlignItems);
                alignButton.Text = flexPanel.AlignItems.ToString();
                lastAction = "alineación cambiada";
                UpdateStatus();
            };

            rightToLeftButton.Click += (_, _) => {
                bool enableRightToLeft = flexPanel.RightToLeft != RightToLeft.Yes;
                flexPanel.RightToLeft = enableRightToLeft ? RightToLeft.Yes : RightToLeft.No;
                rightToLeftButton.Text = enableRightToLeft ? "RTL: Sí" : "RTL: No";
                lastAction = "dirección de lectura cambiada";
                UpdateStatus();
            };

            addButton.Click += (_, _) => {
                Button item = CreateFlexItem(nextItemNumber++);
                flexPanel.Controls.Add(item);
                lastAction = $"agregado {item.Text}";
                UpdateStatus();
            };

            removeButton.Click += (_, _) => {
                Control? item = FindLastFlexItem(flexPanel);

                if(item == null) {
                    lastAction = "no quedan elementos flexibles";
                } else {
                    string itemText = item.Text;
                    flexPanel.Controls.Remove(item);
                    item.Dispose();
                    lastAction = $"eliminado {itemText}";
                }

                UpdateStatus();
            };

            pauseButton.Click += (_, _) => {
                lastAction = flexPanel.PauseAnimation() ? "animación pausada" : "sin animación activa";
                UpdateStatus();
            };
            resumeButton.Click += (_, _) => {
                lastAction = flexPanel.ResumeAnimation() ? "animación reanudada" : "sin pausa activa";
                UpdateStatus();
            };
            stopButton.Click += (_, _) => {
                lastAction = flexPanel.StopAnimation() ? "animación detenida" : "sin animación activa";
                UpdateStatus();
            };

            flexPanel.AnimationCompleted += (_, _) => {
                completedCount++;
                UpdateStatus();
            };
            flexPanel.AnimationCanceled += (_, _) => {
                canceledCount++;
                UpdateStatus();
            };
            flexPanel.AnimationStateChanged += (_, _) => UpdateStatus();

            actionsPanel.Controls.Add(directionButton);
            actionsPanel.Controls.Add(justifyButton);
            actionsPanel.Controls.Add(wrapButton);
            actionsPanel.Controls.Add(alignButton);
            actionsPanel.Controls.Add(rightToLeftButton);
            actionsPanel.Controls.Add(addButton);
            actionsPanel.Controls.Add(removeButton);
            actionsPanel.Controls.Add(pauseButton);
            actionsPanel.Controls.Add(resumeButton);
            actionsPanel.Controls.Add(stopButton);

            Label instructionsLabel = new Label {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(20, 548),
                Size = new Size(860, 52),
                Text = "Verifica: orden estable; Padding y Margin; fila y columna; todas las distribuciones; wrap; alineación; RTL; agregar y eliminar durante una transición; pausa, reanudación y detención; redimensionamiento sin superposición."
            };

            Controls.Add(statusLabel);
            Controls.Add(flexPanel);
            Controls.Add(actionsPanel);
            Controls.Add(instructionsLabel);

            UpdateStatus();
        }

        private static Button CreateFlexItem(int number) {
            int colorOffset = (number * 23) % 80;
            return new Button {
                AccessibleName = $"Elemento flexible {number}",
                BackColor = Color.FromArgb(105 + colorOffset, 85, 220 - (colorOffset / 2)),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Margin = new Padding(4 + (number % 3), 5, 8, 7),
                Size = new Size(92 + ((number % 3) * 24), 42 + ((number % 2) * 18)),
                Text = $"Elemento {number}",
                UseVisualStyleBackColor = false
            };
        }

        private static Button CreateActionButton(string text) {
            return new Button {
                Margin = new Padding(2),
                Size = new Size(100, 34),
                Text = text,
                UseVisualStyleBackColor = true
            };
        }

        private static Control? FindLastFlexItem(SaraUI_FlexPanel panel) {
            for(int index = panel.Controls.Count - 1; index >= 0; index--) {
                Control control = panel.Controls[index];

                if(control.Dock == DockStyle.None) {
                    return control;
                }
            }

            return null;
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

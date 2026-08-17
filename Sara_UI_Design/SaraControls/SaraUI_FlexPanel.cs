using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Contenedor avanzado que implementa un sistema de diseño flexible inspirado en CSS Flexbox. 
    /// Permite alinear, distribuir y ajustar controles hijos automáticamente en filas o columnas.
    /// </summary>
    [ToolboxItem(true)]
    public class SaraUI_FlexPanel:Panel {

        public enum FlexDirection { Row, Column }
        public enum JustifyContent { Start, Center, End, SpaceBetween, SpaceAround, SpaceEvenly }
        public enum FlexWrap { NoWrap, Wrap }

        private FlexDirection direction = FlexDirection.Row;
        private JustifyContent justify = JustifyContent.Start;
        private FlexWrap wrap = FlexWrap.NoWrap;
        private int childSpacing = 10;
        private int borderRadius = 0;

        /// <summary>
        /// Obtiene o establece la orientación de los elementos (Horizontal o Vertical).
        /// </summary>
        [Category("Sara UI Design")]
        public FlexDirection Direction { get => direction; set { direction = value; PerformLayout(); } }

        /// <summary>
        /// Obtiene o establece el modo de alineación y distribución de los elementos dentro del panel.
        /// </summary>
        [Category("Sara UI Design")]
        public JustifyContent Justify { get => justify; set { justify = value; PerformLayout(); } }

        /// <summary>
        /// Indica si los elementos deben saltar a una nueva línea/columna cuando no hay espacio suficiente.
        /// </summary>
        [Category("Sara UI Design")]
        public FlexWrap WrapContents { get => wrap; set { wrap = value; PerformLayout(); } }

        /// <summary>
        /// Obtiene o establece el espacio de separación (gap) entre cada control hijo.
        /// </summary>
        [Category("Sara UI Design")]
        public int ChildSpacing { get => childSpacing; set { childSpacing = value; PerformLayout(); } }

        /// <summary>
        /// Obtiene o establece el radio de redondeo de las esquinas del panel. 
        /// </summary>
        [Category("Sara UI Design")]
        public int BorderRadius { get => borderRadius; set { borderRadius = value; Invalidate(); } }

        public SaraUI_FlexPanel() {
            this.DoubleBuffered = true;
            this.Size = new Size(300, 200);
        }

        protected override void OnLayout(LayoutEventArgs levent) {
            base.OnLayout(levent);
            var visibleControls = Controls.Cast<Control>()
                                          .Where(c => c.Visible)
                                          .Reverse()
                                          .ToList();

            if(!visibleControls.Any())
                return;

            if(direction == FlexDirection.Row)
                LayoutRow(visibleControls);
            else
                LayoutColumn(visibleControls);
        }

        private void LayoutRow(List<Control> controls) {
            int availableWidth = this.Width - Padding.Left - Padding.Right;
            int totalItemsWidth = controls.Sum(c => c.Width);
            int remainingSpace = availableWidth - totalItemsWidth;
            int currentX = Padding.Left;
            int currentY = Padding.Top;
            int dynamicGap = childSpacing;

            // CORRECCIÓN PROTECCIÓN CRÍTICA: Validamos la cantidad de controles para evitar división por cero
            switch(justify) {
                case JustifyContent.Center:
                currentX = Padding.Left + (remainingSpace / 2);
                break;
                case JustifyContent.End:
                currentX = Width - Padding.Right - totalItemsWidth - (childSpacing * (controls.Count - 1));
                break;
                case JustifyContent.SpaceBetween:
                dynamicGap = (controls.Count > 1) ? remainingSpace / (controls.Count - 1) : 0;
                break;
                case JustifyContent.SpaceAround:
                dynamicGap = remainingSpace / Math.Max(1, controls.Count);
                currentX = Padding.Left + (dynamicGap / 2);
                break;
                case JustifyContent.SpaceEvenly:
                dynamicGap = remainingSpace / (controls.Count + 1);
                currentX = Padding.Left + dynamicGap;
                break;
            }

            int rowHeight = 0;
            foreach(var ctrl in controls) {
                if(wrap == FlexWrap.Wrap && currentX + ctrl.Width > Width - Padding.Right) {
                    currentX = Padding.Left;
                    currentY += rowHeight + childSpacing;
                    rowHeight = 0;
                }

                ctrl.Location = new Point(currentX, currentY);
                currentX += ctrl.Width + dynamicGap;
                rowHeight = Math.Max(rowHeight, ctrl.Height);
            }
        }

        private void LayoutColumn(List<Control> controls) {
            int availableHeight = this.Height - Padding.Top - Padding.Bottom;
            int totalItemsHeight = controls.Sum(c => c.Height);
            int remainingSpace = availableHeight - totalItemsHeight;
            int currentY = Padding.Top;
            int dynamicGap = childSpacing;

            // CORRECCIÓN PROTECCIÓN CRÍTICA: Validamos la cantidad de controles para evitar división por cero
            switch(justify) {
                case JustifyContent.Center:
                currentY = Padding.Top + (remainingSpace / 2);
                break;
                case JustifyContent.End:
                currentY = Height - Padding.Bottom - totalItemsHeight - (childSpacing * (controls.Count - 1));
                break;
                case JustifyContent.SpaceBetween:
                dynamicGap = (controls.Count > 1) ? remainingSpace / (controls.Count - 1) : 0;
                break;
                case JustifyContent.SpaceAround:
                dynamicGap = remainingSpace / Math.Max(1, controls.Count);
                currentY = Padding.Top + (dynamicGap / 2);
                break;
                case JustifyContent.SpaceEvenly:
                dynamicGap = remainingSpace / (controls.Count + 1);
                currentY = Padding.Top + dynamicGap;
                break;
            }

            foreach(var ctrl in controls) {
                int posX = Padding.Left + (Width - Padding.Left - Padding.Right - ctrl.Width) / 2;
                ctrl.Location = new Point(posX, currentY);
                currentY += ctrl.Height + dynamicGap;
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            if(borderRadius <= 0) {
                this.Region = new Region(ClientRectangle);
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Forzar limpieza perimetral externa antes del corte de región
            e.Graphics.Clear(this.Parent?.BackColor ?? SystemColors.Control);

            using(var path = GetFigurePath(ClientRectangle, borderRadius)) {
                this.Region = new Region(path);
            }
        }

        protected GraphicsPath GetFigurePath(Rectangle rect, int radius) {
            GraphicsPath path = new GraphicsPath();
            float s = radius * 2F;
            if(s <= 0)
                s = 1;
            if(s > rect.Width)
                s = rect.Width;
            if(s > rect.Height)
                s = rect.Height;

            path.AddArc(rect.X, rect.Y, s, s, 180, 90);
            path.AddArc(rect.Right - s, rect.Y, s, s, 270, 90);
            path.AddArc(rect.Right - s, rect.Bottom - s, s, s, 0, 90);
            path.AddArc(rect.X, rect.Bottom - s, s, s, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
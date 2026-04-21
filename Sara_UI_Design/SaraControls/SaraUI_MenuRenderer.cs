using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {
    public class SaraUI_MenuRenderer:ToolStripProfessionalRenderer {
        private Color primaryColor;
        private Color textColor;
        private int arrowThickness;

        public SaraUI_MenuRenderer(bool isMainMenu, Color primaryColor, Color textColor, Color backColor)
        : base(new SaraUI_MenuColorTable(isMainMenu, primaryColor, backColor)) {
            this.primaryColor = primaryColor;
            this.textColor = (textColor == Color.Empty) ? (isMainMenu ? Color.Gainsboro : Color.DimGray) : textColor;
            this.arrowThickness = isMainMenu ? 3 : 2;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            base.OnRenderItemText(e);
            // El texto cambia al color primario cuando se selecciona
            e.Item.ForeColor = e.Item.Selected ? primaryColor : textColor;
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
            if(e.Item.Selected) {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // Dibujamos un rectángulo de selección redondeado sutil
                Rectangle rect = new Rectangle(2, 1, e.Item.Width - 4, e.Item.Height - 2);
                using(SolidBrush brush = new SolidBrush(Color.FromArgb(30, primaryColor))) {
                    using(GraphicsPath path = GetRoundedPath(rect, 6)) {
                        g.FillPath(brush, path);
                    }
                }
            }
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) {
            var graph = e.Graphics;
            var arrowSize = new Size(6, 10);
            var arrowColor = e.Item.Selected ? primaryColor : Color.DarkGray;
            graph.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(arrowColor, arrowThickness)) {
                // Dibujamos un chevron moderno en lugar de un triángulo
                int x = e.ArrowRectangle.X + (e.ArrowRectangle.Width - arrowSize.Width) / 2;
                int y = e.ArrowRectangle.Y + (e.ArrowRectangle.Height - arrowSize.Height) / 2;
                graph.DrawLine(pen, x, y, x + 4, y + 5);
                graph.DrawLine(pen, x + 4, y + 5, x, y + 10);
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius) {
            GraphicsPath path = new GraphicsPath();
            float s = radius * 2f;
            path.AddArc(rect.X, rect.Y, s, s, 180, 90);
            path.AddArc(rect.Right - s, rect.Y, s, s, 270, 90);
            path.AddArc(rect.Right - s, rect.Bottom - s, s, s, 0, 90);
            path.AddArc(rect.X, rect.Bottom - s, s, s, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
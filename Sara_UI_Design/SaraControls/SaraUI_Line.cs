using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {
    public class SaraUI_Line:Control {
        // Enums para la orientación
        public enum LineOrientation { Horizontal, Vertical }

        // Fields
        private LineOrientation orientation = LineOrientation.Horizontal;
        private int lineWidth = 2;
        private Color lineColor = Color.DimGray;
        private DashStyle lineStyle = DashStyle.Solid;

        // Properties
        [Category("Sara UI Design")]
        public LineOrientation Orientation {
            get => orientation;
            set { orientation = value; this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        public int LineWidth {
            get => lineWidth;
            set { lineWidth = (value > 0) ? value : 1; this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        public Color LineColor {
            get => lineColor;
            set { lineColor = value; this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        public DashStyle LineStyle {
            get => lineStyle;
            set { lineStyle = value; this.Invalidate(); }
        }

        public SaraUI_Line() {
            // Estilos para evitar parpadeo y permitir fondos transparentes
            this.SetStyle(ControlStyles.SupportsTransparentBackColor |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw |
                          ControlStyles.UserPaint, true);

            this.BackColor = Color.Transparent;
            this.Size = new Size(100, 2); // Tamaño inicial por defecto
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(lineColor, lineWidth)) {
                pen.DashStyle = lineStyle;

                if(orientation == LineOrientation.Horizontal) {
                    // Dibuja la línea centrada verticalmente
                    float y = this.Height / 2.0f;
                    g.DrawLine(pen, 0, y, this.Width, y);
                } else {
                    // Dibuja la línea centrada horizontalmente
                    float x = this.Width / 2.0f;
                    g.DrawLine(pen, x, 0, x, this.Height);
                }
            }
        }
    }
}
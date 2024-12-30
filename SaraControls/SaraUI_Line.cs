using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {
    public class SaraUI_Line : Control {
        // Fields
        private int x1 = 0;
        private int y1 = 0;
        private int x2 = 100;
        private int y2 = 0;
        private int lineWidth = 2;
        private Color lineColor = Color.Black;

        // Properties
        [Category("Sara UI Design")]
        public int X1 {
            get { return x1; }
            set {
                x1 = value;
                this.Invalidate();
            }
        }

        [Category("Sara UI Design")]
        public int Y1 {
            get { return y1; }
            set {
                y1 = value;
                this.Invalidate();
            }
        }

        [Category("Sara UI Design")]
        public int X2 {
            get { return x2; }
            set {
                x2 = value;
                this.Invalidate();
            }
        }

        [Category("Sara UI Design")]
        public int Y2 {
            get { return y2; }
            set {
                y2 = value;
                this.Invalidate();
            }
        }

        [Category("Sara UI Design")]
        public int LineWidth {
            get { return lineWidth; }
            set {
                if (value > 0) {
                    lineWidth = value;
                    this.Invalidate();
                }
            }
        }

        [Category("Sara UI Design")]
        public Color LineColor {
            get { return lineColor; }
            set {
                lineColor = value;
                this.Invalidate();
            }
        }

        // Constructor
        public SaraUI_Line() {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                          ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        // Overridden Paint method
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            using (Pen pen = new Pen(lineColor, lineWidth)) {
                e.Graphics.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        // Overridden Size to ensure the control is flexible
        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            x2 = this.Width;
            //y2 = this.Height;
            this.Invalidate();
        }
    }
}

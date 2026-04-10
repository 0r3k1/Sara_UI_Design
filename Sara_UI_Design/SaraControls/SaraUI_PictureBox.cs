using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {
    public class SaraUI_PictureBox:PictureBox {
        private int borderSize = 2;
        private Color borderColor = Color.RoyalBlue;
        private Color borderColor2 = Color.HotPink;
        private DashStyle borderLineStyle = DashStyle.Solid;
        private DashCap borderCapStyle = DashCap.Flat;
        private float gradientAngle = 50F;
        private bool isCircular = true;

        [Category("Sara UI Design")]
        public int BorderSize { get => borderSize; set { borderSize = value; Invalidate(); } }

        [Category("Sara UI Design")]
        public Color BorderColor { get => borderColor; set { borderColor = value; Invalidate(); } }

        [Category("Sara UI Design")]
        public Color BorderColor2 { get => borderColor2; set { borderColor2 = value; Invalidate(); } }

        [Category("Sara UI Design")]
        public DashStyle BorderLineStyle { get => borderLineStyle; set { borderLineStyle = value; Invalidate(); } }

        [Category("Sara UI Design")]
        public DashCap BorderCapStyle { get => borderCapStyle; set { borderCapStyle = value; Invalidate(); } }

        [Category("Sara UI Design")]
        public float GradientAngle { get => gradientAngle; set { gradientAngle = value; Invalidate(); } }

        [Category("Sara UI Design")]
        public bool IsCircular { get => isCircular; set { isCircular = value; Invalidate(); } }

        public SaraUI_PictureBox() {
            this.Size = new Size(100, 100);
            this.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            if(isCircular)
                this.Size = new Size(this.Width, this.Width);
        }

        protected override void OnPaint(PaintEventArgs pe) {
            base.OnPaint(pe);
            var graph = pe.Graphics;
            graph.SmoothingMode = SmoothingMode.AntiAlias;

            var rectContour = Rectangle.Inflate(this.ClientRectangle, -1, -1);
            var rectBorder = Rectangle.Inflate(rectContour, -borderSize, -borderSize);

            using(var pathRegion = new GraphicsPath())
            using(var borderGColor = new LinearGradientBrush(rectBorder, borderColor, borderColor2, gradientAngle))
            using(var penBorder = new Pen(borderGColor, borderSize)) {

                if(isCircular)
                    pathRegion.AddEllipse(rectContour);
                else
                    pathRegion.AddRectangle(rectContour);

                this.Region = new Region(pathRegion);
                penBorder.DashStyle = borderLineStyle;
                penBorder.DashCap = borderCapStyle;

                if(borderSize > 0) {
                    if(isCircular)
                        graph.DrawEllipse(penBorder, rectBorder);
                    else
                        graph.DrawRectangle(penBorder, rectBorder);
                }
            }
        }
    }
}
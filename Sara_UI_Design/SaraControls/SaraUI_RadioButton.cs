using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {
    public class SaraUI_RadioButton:RadioButton {
        private Color checkedColor = Color.MediumSlateBlue;
        private Color unCheckedColor = Color.Gray;
        private bool isHovering = false;

        [Category("Sara UI Design")]
        public Color CheckedColor { get => checkedColor; set { checkedColor = value; Invalidate(); } }

        [Category("Sara UI Design")]
        public Color UnCheckedColor { get => unCheckedColor; set { unCheckedColor = value; Invalidate(); } }

        public SaraUI_RadioButton() {
            this.MinimumSize = new Size(0, 21);
            this.Padding = new Padding(10, 0, 0, 0);
            this.Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); isHovering = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); isHovering = false; Invalidate(); }

        protected override void OnPaint(PaintEventArgs pevent) {
            Graphics graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(this.BackColor);

            float rbSize = 18F;
            float checkSize = 10F;
            RectangleF rectBorder = new RectangleF(1, (Height - rbSize) / 2, rbSize, rbSize);
            RectangleF rectCheck = new RectangleF(rectBorder.X + (rbSize - checkSize) / 2, (Height - checkSize) / 2, checkSize, checkSize);

            Color renderColor = Checked ? checkedColor : (isHovering ? Color.DarkGray : unCheckedColor);

            using(Pen penBorder = new Pen(renderColor, 2F))
            using(SolidBrush brushCheck = new SolidBrush(checkedColor))
            using(SolidBrush brushText = new SolidBrush(this.ForeColor)) {
                graphics.DrawEllipse(penBorder, rectBorder);
                if(Checked)
                    graphics.FillEllipse(brushCheck, rectCheck);

                Size textSize = TextRenderer.MeasureText(this.Text, this.Font);
                graphics.DrawString(Text, Font, brushText, rbSize + 8, (Height - textSize.Height) / 2);
            }
        }
    }
}
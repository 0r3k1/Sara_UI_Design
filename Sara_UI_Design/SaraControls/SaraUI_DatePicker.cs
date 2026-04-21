using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {
    public class SaraUI_DatePicker:DateTimePicker {
        // Fields
        private Color skinColor = Color.MediumSlateBlue;
        private Color textColor = Color.White;
        private Color borderColor = Color.PaleVioletRed;
        private int borderSize = 0;
        private int borderRadius = 10;
        private bool droppedDown = false;

        // Propiedades en Sara UI Design
        [Category("Sara UI Design")]
        public Color SkinColor {
            get => skinColor;
            set { skinColor = value; this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        public Color TextColor {
            get => textColor;
            set { textColor = value; this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        public Color BorderColor {
            get => borderColor;
            set { borderColor = value; this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        public int BorderSize {
            get => borderSize;
            set { borderSize = value; this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        public int BorderRadius {
            get => borderRadius;
            set { borderRadius = value; this.Invalidate(); }
        }

        public SaraUI_DatePicker() {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.MinimumSize = new Size(0, 35);
            this.Font = new Font("Segoe UI", 9.5F);
        }

        protected override void OnDropDown(EventArgs eventargs) { base.OnDropDown(eventargs); droppedDown = true; this.Invalidate(); }
        protected override void OnCloseUp(EventArgs eventargs) { base.OnCloseUp(eventargs); droppedDown = false; this.Invalidate(); }
        protected override void OnKeyPress(KeyPressEventArgs e) { base.OnKeyPress(e); e.Handled = true; } // Solo lectura en texto


        protected override void OnPaint(PaintEventArgs e) {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // En lugar de transparencia, pintamos el fondo con el color del padre 
            // para que las esquinas redondeadas se vean bien
            graphics.Clear(this.Parent?.BackColor ?? Color.White);

            Rectangle rectClient = this.ClientRectangle;

            // 1. Dibujar Fondo Redondeado
            using(GraphicsPath pathBack = GetFigurePath(rectClient, borderRadius)) {
                using(SolidBrush brushBack = new SolidBrush(skinColor)) {
                    graphics.FillPath(brushBack, pathBack);
                }
            }

            // 2. Dibujar Texto
            using(SolidBrush brushText = new SolidBrush(textColor))
            using(StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center }) {
                graphics.DrawString("   " + this.Text, this.Font, brushText,
                                   new Rectangle(10, 0, this.Width - 40, this.Height), sf);
            }

            // 3. USAR LIBRERÍA DE ICONOS
            Rectangle iconRect = new Rectangle(this.Width - 30, (this.Height - 20) / 2, 20, 20);
            SaraUI_IconLibrary.DrawCalendar(graphics, iconRect, textColor);

            // 4. Borde (si aplica)
            if(borderSize > 0) {
                using(Pen penBorder = new Pen(borderColor, borderSize)) {
                    penBorder.Alignment = PenAlignment.Inset;
                    using(GraphicsPath pathBorder = GetFigurePath(rectClient, borderRadius))
                        graphics.DrawPath(penBorder, pathBorder);
                }
            }
        }

        private GraphicsPath GetFigurePath(Rectangle rect, int radius) {
            GraphicsPath path = new GraphicsPath();
            float s = radius * 2F;
            path.AddArc(rect.X, rect.Y, s, s, 180, 90);
            path.AddArc(rect.Right - s, rect.Y, s, s, 270, 90);
            path.AddArc(rect.Right - s, rect.Bottom - s, s, s, 0, 90);
            path.AddArc(rect.X, rect.Bottom - s, s, s, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
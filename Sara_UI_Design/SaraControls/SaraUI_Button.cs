using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {
    [DefaultEvent("Click")] // define el evento por defecto
    public class SaraUI_Button:Button {
        // Campos privados
        private int borderSize = 0;
        private int borderRadius = 20;
        private Color borderColor = Color.PaleVioletRed;

        // Campos para efectos visuales
        private bool isMouseOver = false;
        private bool isPressed = false;

        // Propiedades con Categoría
        [Category("Sara UI Design")]
        public int BorderSize {
            get => borderSize;
            set { borderSize = value; Invalidate(); }
        }

        [Category("Sara UI Design")]
        public int BorderRadius {
            get => borderRadius;
            set {
                borderRadius = (value <= Height) ? value : Height;
                Invalidate();
            }
        }

        [Category("Sara UI Design")]
        public Color BorderColor {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        public SaraUI_Button() {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Size = new Size(150, 40);
            this.BackColor = Color.MediumSlateBlue;
            this.ForeColor = Color.White;

            // Crucial para evitar parpadeo
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
        }


        protected override void OnMouseEnter(EventArgs e) {
            base.OnMouseEnter(e);
            isMouseOver = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            isMouseOver = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent) {
            base.OnMouseDown(mevent);
            isPressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent) {
            base.OnMouseUp(mevent);
            isPressed = false;
            Invalidate();
        }

        private GraphicsPath GetFigurePath(Rectangle rect, int radius) {
            GraphicsPath path = new GraphicsPath();
            float curveSize = radius * 2F;

            path.StartFigure();
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
            path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
            path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnPaint(PaintEventArgs pevent) {
            base.OnPaint(pevent);
            Graphics graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rectSurface = this.ClientRectangle;
            Rectangle rectBorder = Rectangle.Inflate(rectSurface, -borderSize, -borderSize);

            // Efecto de oscurecimiento simple al pasar el mouse
            if(isMouseOver) {
                using(SolidBrush hoverBrush = new SolidBrush(Color.FromArgb(30, Color.White))) {
                    graphics.FillRectangle(hoverBrush, rectSurface);
                }
            }

            if(borderRadius > 2) {
                using(GraphicsPath pathSurface = GetFigurePath(rectSurface, borderRadius))
                using(GraphicsPath pathBorder = GetFigurePath(rectBorder, borderRadius - borderSize))
                using(Pen penSurface = new Pen(this.Parent?.BackColor ?? Color.White, 2))
                using(Pen penBorder = new Pen(borderColor, borderSize)) {
                    // Aplicar región redondeada
                    this.Region = new Region(pathSurface);

                    // Dibujar borde de superficie (antialiasing fix)
                    graphics.DrawPath(penSurface, pathSurface);

                    // Dibujar borde principal
                    if(borderSize >= 1)
                        graphics.DrawPath(penBorder, pathBorder);
                }
            } else {
                this.Region = new Region(rectSurface);
                if(borderSize >= 1) {
                    using(Pen penBorder = new Pen(borderColor, borderSize)) {
                        penBorder.Alignment = PenAlignment.Inset;
                        graphics.DrawRectangle(penBorder, 0, 0, Width - 1, Height - 1);
                    }
                }
            }
        }
    }
}
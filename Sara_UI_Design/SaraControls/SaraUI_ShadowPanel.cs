using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {
    public class SaraUI_ShadowPanel:SaraUI_FlexPanel {
        // Fields para la sombra
        private int shadowSize = 10;
        private Color shadowColor = Color.FromArgb(64, 64, 64);
        private int shadowOpacity = 100;
        private int shadowOffsetX = 0;
        private int shadowOffsetY = 5; // Por defecto un poco hacia abajo

        // --- Propiedades en Sara UI Design ---

        [Category("Sara UI Design")]
        public int ShadowSize {
            get => shadowSize;
            set { shadowSize = value > 0 ? value : 1; UpdatePadding(); this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        public Color ShadowColor {
            get => shadowColor;
            set { shadowColor = value; this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        public int ShadowOpacity {
            get => shadowOpacity;
            set { shadowOpacity = Math.Max(0, Math.Min(255, value)); this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        public int ShadowOffsetX {
            get => shadowOffsetX;
            set { shadowOffsetX = value; this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        public int ShadowOffsetY {
            get => shadowOffsetY;
            set { shadowOffsetY = value; this.Invalidate(); }
        }

        public SaraUI_ShadowPanel() {
            this.DoubleBuffered = true;
            this.BackColor = Color.White;
            UpdatePadding();
        }

        private void UpdatePadding() {
            // El padding asegura que el contenido Flex no se dibuje sobre la zona de la sombra
            this.Padding = new Padding(shadowSize + Math.Abs(shadowOffsetX),
                                      shadowSize + Math.Abs(shadowOffsetY),
                                      shadowSize + Math.Abs(shadowOffsetX),
                                      shadowSize + Math.Abs(shadowOffsetY));
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Limpiar fondo con el color del contenedor padre
            g.Clear(this.Parent?.BackColor ?? SystemColors.Control);

            // 1. Definir el área del panel blanco (fijo en el centro relativo al padding)
            Rectangle rectPanel = new Rectangle(
                shadowSize + (shadowOffsetX < 0 ? Math.Abs(shadowOffsetX) : 0),
                shadowSize + (shadowOffsetY < 0 ? Math.Abs(shadowOffsetY) : 0),
                this.Width - (shadowSize * 2) - Math.Abs(shadowOffsetX) - 1,
                this.Height - (shadowSize * 2) - Math.Abs(shadowOffsetY) - 1
            );

            if(rectPanel.Width > 5 && rectPanel.Height > 5) {
                // 2. Dibujar la sombra desplazada
                DrawShadow(g, rectPanel);

                // 3. Dibujar el fondo del panel
                using(GraphicsPath pathPanel = GetFigurePath(rectPanel, BorderRadius))
                using(SolidBrush brushBack = new SolidBrush(this.BackColor)) {
                    g.FillPath(brushBack, pathPanel);
                }
            }
        }

        private void DrawShadow(Graphics g, Rectangle rect) {
            // El rectángulo de la sombra es el área del panel + el tamaño de difuminado
            Rectangle shadowRect = rect;
            shadowRect.Inflate(shadowSize, shadowSize);

            // APLICAMOS EL OFFSET: Movemos el rectángulo de la sombra
            shadowRect.Offset(shadowOffsetX, shadowOffsetY);

            using(GraphicsPath pathShadow = GetFigurePath(shadowRect, BorderRadius))
            using(PathGradientBrush pgb = new PathGradientBrush(pathShadow)) {
                // Mantener la intensidad en el centro
                pgb.FocusScales = new PointF(0.85f, 0.85f);
                pgb.CenterColor = Color.FromArgb(shadowOpacity, shadowColor);
                pgb.SurroundColors = new Color[] { Color.Transparent };

                g.FillPath(pgb, pathShadow);
            }
        }

        private GraphicsPath GetFigurePath(Rectangle rect, int radius) {
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
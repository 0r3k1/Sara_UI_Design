using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Panel avanzado que combina la disposición flexible (Flexbox) con efectos de sombra realistas de degradado difuso.
    /// </summary>
    [ToolboxItem(true)]
    public class SaraUI_ShadowPanel:SaraUI_FlexPanel {
        private int shadowSize = 10;
        private Color shadowColor = Color.FromArgb(64, 64, 64);
        private int shadowOpacity = 100;
        private int shadowOffsetX = 0;
        private int shadowOffsetY = 5;

        /// <summary>
        /// Obtiene o establece el tamaño o difusión (blur) de la sombra en píxeles.
        /// </summary>
        [Category("Sara UI Design")]
        public int ShadowSize {
            get => shadowSize;
            set { shadowSize = value > 0 ? value : 1; UpdatePadding(); this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el color base de la sombra. Por defecto es un gris oscuro.
        /// </summary>
        [Category("Sara UI Design")]
        public Color ShadowColor {
            get => shadowColor;
            set { shadowColor = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el nivel de transparencia de la sombra (de 0 a 255).
        /// </summary>
        [Category("Sara UI Design")]
        public int ShadowOpacity {
            get => shadowOpacity;
            set { shadowOpacity = Math.Max(0, Math.Min(255, value)); this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el desplazamiento horizontal de la sombra respecto al centro del panel.
        /// </summary>
        [Category("Sara UI Design")]
        public int ShadowOffsetX {
            get => shadowOffsetX;
            set { shadowOffsetX = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el desplazamiento vertical de la sombra.
        /// </summary>
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
            this.Padding = new Padding(shadowSize + Math.Abs(shadowOffsetX),
                                      shadowSize + Math.Abs(shadowOffsetY),
                                      shadowSize + Math.Abs(shadowOffsetX),
                                      shadowSize + Math.Abs(shadowOffsetY));
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.Clear(this.Parent?.BackColor ?? SystemColors.Control);

            Rectangle rectPanel = new Rectangle(
                shadowSize + (shadowOffsetX < 0 ? Math.Abs(shadowOffsetX) : 0),
                shadowSize + (shadowOffsetY < 0 ? Math.Abs(shadowOffsetY) : 0),
                this.Width - (shadowSize * 2) - Math.Abs(shadowOffsetX) - 1,
                this.Height - (shadowSize * 2) - Math.Abs(shadowOffsetY) - 1
            );

            if(rectPanel.Width > 5 && rectPanel.Height > 5) {
                // 1. Dibujar la sombra degradada atenuada
                DrawShadow(g, rectPanel);

                // 2. Dibujar el fondo del contenedor principal redondeado
                using(GraphicsPath pathPanel = GetFigurePath(rectPanel, BorderRadius))
                using(SolidBrush brushBack = new SolidBrush(this.BackColor)) {
                    g.FillPath(brushBack, pathPanel);
                }
            }
        }

        private void DrawShadow(Graphics g, Rectangle rect) {
            Rectangle shadowRect = rect;
            shadowRect.Inflate(shadowSize, shadowSize);
            shadowRect.Offset(shadowOffsetX, shadowOffsetY);

            using(GraphicsPath pathShadow = GetFigurePath(shadowRect, BorderRadius))
            using(PathGradientBrush pgb = new PathGradientBrush(pathShadow)) {
                // OPTIMIZACIÓN DE BLUR DE GRADIENTE CONTINUO (Acabado neumórfico suave)
                pgb.FocusScales = new PointF(0.75f, 0.75f);
                pgb.CenterColor = Color.FromArgb(shadowOpacity, shadowColor);
                pgb.SurroundColors = new Color[] { Color.Transparent };

                g.FillPath(pgb, pathShadow);
            }
        }
    }
}
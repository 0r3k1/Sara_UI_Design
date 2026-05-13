using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Panel avanzado que combina la disposición flexible (Flexbox) con efectos de sombra realistas.
    /// Permite crear interfaces con profundidad mediante sombras difuminadas, desplazamientos (offsets) y bordes redondeados.
    /// </summary>
    public class SaraUI_ShadowPanel:SaraUI_FlexPanel {
        // Fields para la sombra
        private int shadowSize = 10;
        private Color shadowColor = Color.FromArgb(64, 64, 64);
        private int shadowOpacity = 100;
        private int shadowOffsetX = 0;
        private int shadowOffsetY = 5; // Por defecto un poco hacia abajo

        // --- Propiedades en Sara UI Design ---

        /// <summary>
        /// Obtiene o establece el tamaño o difusión (blur) de la sombra en píxeles. 
        /// Un valor mayor crea una sombra más suave y extendida.
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
        /// Útil para simular la dirección de la fuente de luz.
        /// </summary>
        [Category("Sara UI Design")]
        public int ShadowOffsetY {
            get => shadowOffsetY;
            set { shadowOffsetY = value; this.Invalidate(); }
        }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_ShadowPanel"/>, activando el doble búfer 
        /// y ajustando el espaciado interno para albergar la sombra.
        /// </summary>
        public SaraUI_ShadowPanel() {
            this.DoubleBuffered = true;
            this.BackColor = Color.White;
            UpdatePadding();
        }

        /// <summary>
        /// Calcula y actualiza automáticamente los márgenes internos (Padding) del panel 
        /// para asegurar que el contenido flexible no se superponga con el área de dibujo de la sombra.
        /// </summary>
        private void UpdatePadding() {
            // El padding asegura que el contenido Flex no se dibuje sobre la zona de la sombra
            this.Padding = new Padding(shadowSize + Math.Abs(shadowOffsetX),
                                      shadowSize + Math.Abs(shadowOffsetY),
                                      shadowSize + Math.Abs(shadowOffsetX),
                                      shadowSize + Math.Abs(shadowOffsetY));
        }

        /// <summary>
        /// Gestiona el ciclo de dibujo del control, renderizando primero la sombra difuminada 
        /// y posteriormente el fondo del panel con sus bordes redondeados.
        /// </summary>
        /// <param name="e">Argumentos del evento de dibujo.</param>
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

        /// <summary>
        /// Realiza el dibujo técnico de la sombra utilizando un <see cref="PathGradientBrush"/>. 
        /// Aplica opacidad, color, desplazamiento y difuminado perimetral.
        /// </summary>
        /// <param name="g">Superficie de dibujo.</param>
        /// <param name="rect">Rectángulo que define el área del panel principal.</param>
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

        /// <summary>
        /// Genera el trazado geométrico con esquinas redondeadas, validando que el radio 
        /// sea proporcional a las dimensiones del rectángulo proporcionado.
        /// </summary>
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
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Control gráfico simple para dibujar líneas separadoras horizontales o verticales 
    /// con soporte para diferentes grosores, colores y estilos de trazo.
    /// </summary>
    public class SaraUI_Line:Control {

        /// <summary>
        /// Define la dirección de la línea dentro de los límites del control.
        /// </summary>
        public enum LineOrientation { Horizontal, Vertical }

        // Fields
        private LineOrientation orientation = LineOrientation.Horizontal;
        private int lineWidth = 2;
        private Color lineColor = Color.DimGray;
        private DashStyle lineStyle = DashStyle.Solid;

        // Properties

        /// <summary>
        /// Obtiene o establece si la línea se dibuja de forma horizontal o vertical.
        /// </summary>
        [Category("Sara UI Design")]
        public LineOrientation Orientation {
            get => orientation;
            set { orientation = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el grosor de la línea en píxeles.
        /// </summary>
        [Category("Sara UI Design")]
        public int LineWidth {
            get => lineWidth;
            set { lineWidth = (value > 0) ? value : 1; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el color de la línea.
        /// </summary>
        [Category("Sara UI Design")]
        public Color LineColor {
            get => lineColor;
            set { lineColor = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el estilo del trazo (Sólido, Punteado, Discontinuo, etc.).
        /// </summary>
        [Category("Sara UI Design")]
        public DashStyle LineStyle {
            get => lineStyle;
            set { lineStyle = value; this.Invalidate(); }
        }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_Line"/> con fondo transparente 
        /// y optimizaciones de redibujado.
        /// </summary>
        public SaraUI_Line() {
            // Estilos para evitar parpadeo y permitir fondos transparentes
            this.SetStyle(ControlStyles.SupportsTransparentBackColor |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw |
                          ControlStyles.UserPaint, true);

            this.BackColor = Color.Transparent;
            this.Size = new Size(100, 2); // Tamaño inicial por defecto
        }

        /// <summary>
        /// Renderiza la línea en el centro del control aplicando el estilo y grosor definidos.
        /// </summary>
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
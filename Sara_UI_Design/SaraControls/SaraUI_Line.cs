using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Control gráfico simple para dibujar líneas separadoras horizontales o verticales 
    /// con soporte para diferentes grosores, colores y estilos de trazo redondeado.
    /// </summary>
    [ToolboxItem(true)]
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
        /// y optimizaciones de redibujado fluido.
        /// </summary>
        public SaraUI_Line() {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw |
                          ControlStyles.UserPaint, true);

            this.BackColor = Color.Transparent;
            this.Size = new Size(100, 2);
        }

        /// <summary>
        /// Renderiza la línea en el centro del control aplicando el estilo, grosor y acabados esféricos suaves.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e) {
            // CORRECCIÓN: Llamada limpia al método base sin el parámetro nombrado erróneo
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(lineColor, lineWidth)) {
                pen.DashStyle = lineStyle;

                // Aplicamos el acabado curvo en las puntas para suavizar la suite
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                if(orientation == LineOrientation.Horizontal) {
                    float y = this.Height / 2.0f;
                    g.DrawLine(pen, lineWidth / 2f, y, this.Width - (lineWidth / 2f), y);
                } else {
                    float x = this.Width / 2.0f;
                    g.DrawLine(pen, x, lineWidth / 2f, x, this.Height - (lineWidth / 2f));
                }
            }
        }
    }
}
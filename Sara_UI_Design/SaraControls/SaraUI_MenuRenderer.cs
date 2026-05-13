using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Motor de renderizado personalizado para menús de Sara UI. 
    /// Gestiona el dibujo de textos con antialiasing, fondos de selección redondeados y flechas de estilo chevron.
    /// </summary>
    public class SaraUI_MenuRenderer:ToolStripProfessionalRenderer {
        private Color primaryColor;
        private Color textColor;
        private int arrowThickness;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_MenuRenderer"/> configurando la paleta de colores
        /// y los grosores de trazo según el tipo de menú.
        /// </summary>
        /// <param name="isMainMenu">Indica si el menú es una barra principal o un submenú desplegable.</param>
        /// <param name="primaryColor">Color de resalte para selecciones y elementos activos.</param>
        /// <param name="textColor">Color base para el texto de los elementos.</param>
        /// <param name="backColor">Color de fondo general del menú.</param>
        public SaraUI_MenuRenderer(bool isMainMenu, Color primaryColor, Color textColor, Color backColor)
        : base(new SaraUI_MenuColorTable(isMainMenu, primaryColor, backColor)) {
            this.primaryColor = primaryColor;
            this.textColor = (textColor == Color.Empty) ? (isMainMenu ? Color.Gainsboro : Color.DimGray) : textColor;
            this.arrowThickness = isMainMenu ? 3 : 2;
        }

        /// <summary>
        /// Renderiza el texto de los elementos del menú aplicando suavizado ClearType 
        /// y cambiando dinámicamente el color cuando el elemento está seleccionado.
        /// </summary>
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            base.OnRenderItemText(e);
            // El texto cambia al color primario cuando se selecciona
            e.Item.ForeColor = e.Item.Selected ? primaryColor : textColor;
        }

        /// <summary>
        /// Dibuja el fondo de los elementos cuando el usuario pasa el ratón sobre ellos, 
        /// creando un rectángulo de selección sutil con esquinas redondeadas.
        /// </summary>
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
            if(e.Item.Selected) {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // Dibujamos un rectángulo de selección redondeado sutil
                Rectangle rect = new Rectangle(2, 1, e.Item.Width - 4, e.Item.Height - 2);
                using(SolidBrush brush = new SolidBrush(Color.FromArgb(30, primaryColor))) {
                    using(GraphicsPath path = GetRoundedPath(rect, 6)) {
                        g.FillPath(brush, path);
                    }
                }
            }
        }

        /// <summary>
        /// Sustituye la flecha clásica de los submenús por un chevron (punta de flecha) 
        /// moderno dibujado mediante vectores.
        /// </summary>
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) {
            var graph = e.Graphics;
            var arrowSize = new Size(6, 10);
            var arrowColor = e.Item.Selected ? primaryColor : Color.DarkGray;
            graph.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(arrowColor, arrowThickness)) {
                // Dibujamos un chevron moderno en lugar de un triángulo
                int x = e.ArrowRectangle.X + (e.ArrowRectangle.Width - arrowSize.Width) / 2;
                int y = e.ArrowRectangle.Y + (e.ArrowRectangle.Height - arrowSize.Height) / 2;
                graph.DrawLine(pen, x, y, x + 4, y + 5);
                graph.DrawLine(pen, x + 4, y + 5, x, y + 10);
            }
        }

        /// <summary>
        /// Genera un camino geométrico (<see cref="GraphicsPath"/>) para crear figuras redondeadas dentro del menú.
        /// </summary>
        /// <param name="rect">Área a redondear.</param>
        /// <param name="radius">Radio de curvatura de las esquinas.</param>
        /// <returns>Un objeto con la forma del trazado redondeado.</returns>
        private GraphicsPath GetRoundedPath(Rectangle rect, int radius) {
            GraphicsPath path = new GraphicsPath();
            float s = radius * 2f;
            path.AddArc(rect.X, rect.Y, s, s, 180, 90);
            path.AddArc(rect.Right - s, rect.Y, s, s, 270, 90);
            path.AddArc(rect.Right - s, rect.Bottom - s, s, s, 0, 90);
            path.AddArc(rect.X, rect.Bottom - s, s, s, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Control de imagen avanzado de la suite Sara UI. 
    /// Permite renderizar imágenes circulares o rectangulares con bordes redondeados y degradados personalizables.
    /// </summary>
    [ToolboxItem(true)]
    public class SaraUI_PictureBox:PictureBox {
        private int borderSize = 2;
        private Color borderColor = Color.RoyalBlue;
        private Color borderColor2 = Color.HotPink;
        private DashStyle borderLineStyle = DashStyle.Solid;
        private DashCap borderCapStyle = DashCap.Flat;
        private float gradientAngle = 50F;
        private bool isCircular = true;
        private int borderRadius = 12; // Nuevo field para control de esquinas suaves

        /// <summary>
        /// Obtiene o establece el grosor del borde decorativo en píxeles.
        /// </summary>
        [Category("Sara UI Design")]
        public int BorderSize { get => borderSize; set { borderSize = value; Invalidate(); } }

        /// <summary>
        /// Primer color del degradado para el borde decorativo.
        /// </summary>
        [Category("Sara UI Design")]
        public Color BorderColor { get => borderColor; set { borderColor = value; Invalidate(); } }

        /// <summary>
        /// Segundo color del degradado para el borde. Si es igual al primero, el borde será de un color sólido.
        /// </summary>
        [Category("Sara UI Design")]
        public Color BorderColor2 { get => borderColor2; set { borderColor2 = value; Invalidate(); } }

        /// <summary>
        /// Define el estilo de la línea del borde (Sólido, punteado, discontinuo).
        /// </summary>
        [Category("Sara UI Design")]
        public DashStyle BorderLineStyle { get => borderLineStyle; set { borderLineStyle = value; Invalidate(); } }

        /// <summary>
        /// Define el estilo de terminación de los trazos discontinuos en el borde.
        /// </summary>
        [Category("Sara UI Design")]
        public DashCap BorderCapStyle { get => borderCapStyle; set { borderCapStyle = value; Invalidate(); } }

        /// <summary>
        /// Obtiene o establece el ángulo de inclinación (en grados) para el degradado lineal del borde.
        /// </summary>
        [Category("Sara UI Design")]
        public float GradientAngle { get => gradientAngle; set { gradientAngle = value; Invalidate(); } }

        /// <summary>
        /// Obtiene o establece si el control debe recortar la imagen en forma de círculo manteniendo relación de aspecto 1:1.
        /// </summary>
        [Category("Sara UI Design")]
        public bool IsCircular { get => isCircular; set { isCircular = value; Invalidate(); } }

        /// <summary>
        /// Obtiene o establece el radio de curvatura de las esquinas cuando la propiedad IsCircular es falsa.
        /// </summary>
        [Category("Sara UI Design")]
        public int BorderRadius { get => borderRadius; set { borderRadius = (value >= 0) ? value : 0; Invalidate(); } }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_PictureBox"/> con un tamaño proporcional
        /// y el modo de ajuste de imagen estirado de manera predeterminada.
        /// </summary>
        public SaraUI_PictureBox() {
            this.Size = new Size(100, 100);
            this.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            if(isCircular)
                this.Size = new Size(this.Width, this.Width);
        }

        /// <summary>
        /// Gestiona el dibujo del control, aplicando el recorte por región y renderizando el borde degradado.
        /// </summary>
        protected override void OnPaint(PaintEventArgs pe) {
            base.OnPaint(pe);
            var graph = pe.Graphics;
            graph.SmoothingMode = SmoothingMode.AntiAlias;

            var rectContour = Rectangle.Inflate(this.ClientRectangle, -1, -1);
            var rectBorder = Rectangle.Inflate(rectContour, -borderSize, -borderSize);

            using(var pathRegion = new GraphicsPath())
            using(var borderGColor = new LinearGradientBrush(rectBorder, borderColor, borderColor2, gradientAngle))
            using(var penBorder = new Pen(borderGColor, borderSize)) {

                if(isCircular) {
                    pathRegion.AddEllipse(rectContour);
                } else {
                    // IMPLEMENTACIÓN SARA UI: Si no es circular, recortamos con esquinas redondeadas
                    float s = borderRadius * 2f;
                    if(s > rectContour.Width)
                        s = rectContour.Width;
                    if(s > rectContour.Height)
                        s = rectContour.Height;
                    if(s <= 0)
                        s = 1;

                    pathRegion.AddArc(rectContour.X, rectContour.Y, s, s, 180, 90);
                    pathRegion.AddArc(rectContour.Right - s, rectContour.Y, s, s, 270, 90);
                    pathRegion.AddArc(rectContour.Right - s, rectContour.Bottom - s, s, s, 0, 90);
                    pathRegion.AddArc(rectContour.X, rectContour.Bottom - s, s, s, 90, 90);
                    pathRegion.CloseFigure();
                }

                this.Region = new Region(pathRegion);
                penBorder.DashStyle = borderLineStyle;
                penBorder.DashCap = borderCapStyle;

                if(borderSize > 0) {
                    if(isCircular) {
                        graph.DrawEllipse(penBorder, rectBorder);
                    } else {
                        // Dibujamos el contorno del borde redondeado de forma precisa
                        float s = borderRadius * 2f;
                        if(s > rectBorder.Width)
                            s = rectBorder.Width;
                        if(s > rectBorder.Height)
                            s = rectBorder.Height;
                        if(s <= 0)
                            s = 1;

                        using(GraphicsPath pathBorder = new GraphicsPath()) {
                            pathBorder.AddArc(rectBorder.X, rectBorder.Y, s, s, 180, 90);
                            pathBorder.AddArc(rectBorder.Right - s, rectBorder.Y, s, s, 270, 90);
                            pathBorder.AddArc(rectBorder.Right - s, rectBorder.Bottom - s, s, s, 0, 90);
                            pathBorder.AddArc(rectBorder.X, rectBorder.Bottom - s, s, s, 90, 90);
                            pathBorder.CloseFigure();
                            graph.DrawPath(penBorder, pathBorder);
                        }
                    }
                }
            }
        }
    }
}
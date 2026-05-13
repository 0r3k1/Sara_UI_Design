using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Control de imagen avanzado de la suite Sara UI. 
    /// Permite renderizar imágenes circulares o rectangulares con bordes degradados personalizables.
    /// </summary>
    public class SaraUI_PictureBox:PictureBox {
        private int borderSize = 2;
        private Color borderColor = Color.RoyalBlue;
        private Color borderColor2 = Color.HotPink;
        private DashStyle borderLineStyle = DashStyle.Solid;
        private DashCap borderCapStyle = DashCap.Flat;
        private float gradientAngle = 50F;
        private bool isCircular = true;

        /// <summary>
        /// Obtiene o establece el grosor del borde decorativo en píxeles.
        /// </summary>
        [Category("Sara UI Design")]
        public int BorderSize { get => borderSize; set { borderSize = value; Invalidate(); } }

        /// <summary>
        /// Primer color del degradado para el borde.
        /// </summary>
        [Category("Sara UI Design")]
        public Color BorderColor { get => borderColor; set { borderColor = value; Invalidate(); } }

        /// <summary>
        /// Segundo color del degradado para el borde. Si es igual al primero, el borde será de un color sólido.
        /// </summary>
        [Category("Sara UI Design")]
        public Color BorderColor2 { get => borderColor2; set { borderColor2 = value; Invalidate(); } }

        /// <summary>
        /// Define el estilo de la línea del borde (Sólido, punteado, etc.).
        /// </summary>
        [Category("Sara UI Design")]
        public DashStyle BorderLineStyle { get => borderLineStyle; set { borderLineStyle = value; Invalidate(); } }

        [Category("Sara UI Design")]
        public DashCap BorderCapStyle { get => borderCapStyle; set { borderCapStyle = value; Invalidate(); } }

        /// <summary>
        /// Obtiene o establece el ángulo de inclinación (en grados) para el degradado del borde.
        /// </summary>
        [Category("Sara UI Design")]
        public float GradientAngle { get => gradientAngle; set { gradientAngle = value; Invalidate(); } }

        /// <summary>
        /// Obtiene o establece si el control debe recortar la imagen en forma de círculo. 
        /// Si es verdadero, el control mantendrá una relación de aspecto 1:1.
        /// </summary>
        [Category("Sara UI Design")]
        public bool IsCircular { get => isCircular; set { isCircular = value; Invalidate(); } }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_PictureBox"/> con un tamaño 
        /// por defecto y el modo de ajuste de imagen estirado.
        /// </summary>
        public SaraUI_PictureBox() {
            this.Size = new Size(100, 100);
            this.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        /// <summary>
        /// Asegura que el control mantenga una forma cuadrada cuando la propiedad <see cref="IsCircular"/> está activa.
        /// </summary>
        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            if(isCircular)
                this.Size = new Size(this.Width, this.Width);
        }

        /// <summary>
        /// Gestiona el dibujo del control, aplicando el recorte de región (círculo/rectángulo) 
        /// y renderizando el borde con degradado lineal.
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

                if(isCircular)
                    pathRegion.AddEllipse(rectContour);
                else
                    pathRegion.AddRectangle(rectContour);

                this.Region = new Region(pathRegion);
                penBorder.DashStyle = borderLineStyle;
                penBorder.DashCap = borderCapStyle;

                if(borderSize > 0) {
                    if(isCircular)
                        graph.DrawEllipse(penBorder, rectBorder);
                    else
                        graph.DrawRectangle(penBorder, rectBorder);
                }
            }
        }
    }
}
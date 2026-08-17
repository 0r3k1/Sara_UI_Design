using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Define las posibles posiciones del texto de valor dentro o alrededor de la barra de progreso.
    /// </summary>
    public enum TextPosition { Left, Right, Center, Sliding, None }

    /// <summary>
    /// Barra de progreso personalizada de Sara UI con soporte para canales y sliders de diferentes alturas, 
    /// bordes redondeados, degradados y visualización de texto dinámica.
    /// </summary>
    [ToolboxItem(true)]
    public class SaraUI_ProgressBar:ProgressBar {
        // Fields de diseño
        private Color channelColor = Color.LightSteelBlue;
        private Color sliderColor = Color.RoyalBlue;
        private Color foreBackColor = Color.RoyalBlue;
        private int channelHeight = 6;
        private int sliderHeight = 6;
        private TextPosition showValue = TextPosition.Right;
        private string symbolBefore = "";
        private string symbolAfter = "";
        private bool showMaximun = false;

        private bool stopPainting = false;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_ProgressBar"/> activando los estilos de doble búfer
        /// y dibujo personalizado por el usuario.
        /// </summary>
        public SaraUI_ProgressBar() {
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.ResizeRedraw, true);

            this.ForeColor = Color.DimGray; // Cambiado a gris oscuro para mejor contraste inicial
        }

        /// <summary>
        /// Obtiene o establece el color de fondo (canal) de la barra de progreso.
        /// </summary>
        [Category("Sara UI Design")]
        public Color ChannelColor {
            get => channelColor;
            set { channelColor = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el color principal del indicador de progreso (slider).
        /// </summary>
        [Category("Sara UI Design")]
        public Color SliderColor {
            get => sliderColor;
            set { sliderColor = value; this.Invalidate(); }
        }

        /// <summary>
        /// Determina si el slider debe dibujarse con un degradado lineal continuo.
        /// </summary>
        [Category("Sara UI Design")]
        public bool UseGradient { get; set; } = true;

        /// <summary>
        /// Obtiene o establece el color de finalización para el degradado del slider de progreso.
        /// </summary>
        [Category("Sara UI Design")]
        public Color SliderColorEnd { get; set; } = Color.HotPink;

        /// <summary>
        /// Obtiene o establece la altura del canal de fondo en píxeles.
        /// </summary>
        [Category("Sara UI Design")]
        public int ChannelHeight {
            get => channelHeight;
            set { channelHeight = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece la altura del indicador de progreso móvil en píxeles.
        /// </summary>
        [Category("Sara UI Design")]
        public int SliderHeight {
            get => sliderHeight;
            set { sliderHeight = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece la posición donde se mostrará el valor porcentual o numérico.
        /// </summary>
        [Category("Sara UI Design")]
        public TextPosition ShowValue {
            get => showValue;
            set { showValue = value; this.Invalidate(); }
        }

        /// <summary>
        /// Texto decorativo que se muestra antes del valor numérico (ej. "$").
        /// </summary>
        [Category("Sara UI Design")]
        public string SymbolBefore {
            get => symbolBefore;
            set { symbolBefore = value; this.Invalidate(); }
        }

        /// <summary>
        /// Texto decorativo que se muestra después del valor numérico (ej. "%" o " kg").
        /// </summary>
        [Category("Sara UI Design")]
        public string SymbolAfter {
            get => symbolAfter;
            set { symbolAfter = value; this.Invalidate(); }
        }

        /// <summary>
        /// Si es verdadero, muestra el valor actual junto al valor máximo (ej. "50/100").
        /// </summary>
        [Category("Sara UI Design")]
        public bool ShowMaximun {
            get => showMaximun;
            set { showMaximun = value; this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override Font Font {
            get => base.Font;
            set => base.Font = value;
        }

        [Category("Sara UI Design")]
        public override Color ForeColor {
            get => base.ForeColor;
            set => base.ForeColor = value;
        }

        private GraphicsPath GetFigurePath(Rectangle rect, int radius) {
            GraphicsPath path = new GraphicsPath();
            float curveSize = radius * 2F;
            if(curveSize <= 0)
                curveSize = 1;
            if(curveSize > rect.Width)
                curveSize = rect.Width;
            if(curveSize > rect.Height)
                curveSize = rect.Height;

            path.StartFigure();
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
            path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
            path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnPaintBackground(PaintEventArgs pevent) {
            if(stopPainting)
                return;
            Graphics graph = pevent.Graphics;
            graph.Clear(this.Parent?.BackColor ?? Color.White);
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics graph = e.Graphics;
            graph.SmoothingMode = SmoothingMode.AntiAlias;
            graph.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Limpiar fondo para evitar basura visual
            graph.Clear(this.Parent?.BackColor ?? Color.White);

            double scaleFactor = (((double)this.Value - this.Minimum) / ((double)this.Maximum - this.Minimum));
            int sliderWidth = (int)(this.Width * scaleFactor);

            // 1. Dibujar Canal Redondeado Suave
            Rectangle rectChannel = new Rectangle(0, (this.Height - channelHeight) / 2, this.Width, channelHeight);
            using(GraphicsPath pathChannel = GetFigurePath(rectChannel, channelHeight / 2))
            using(SolidBrush brushChannel = new SolidBrush(channelColor)) {
                graph.FillPath(brushChannel, pathChannel);
            }

            // 2. Dibujar Slider Redondeado Fluido
            if(sliderWidth > 1) {
                Rectangle rectSlider = new Rectangle(0, (this.Height - sliderHeight) / 2, sliderWidth, sliderHeight);
                using(GraphicsPath pathSlider = GetFigurePath(rectSlider, sliderHeight / 2)) {
                    if(UseGradient) {
                        using(LinearGradientBrush brushGradient = new LinearGradientBrush(rectSlider, sliderColor, SliderColorEnd, 0F))
                            graph.FillPath(brushGradient, pathSlider);
                    } else {
                        using(SolidBrush brushSlider = new SolidBrush(sliderColor))
                            graph.FillPath(brushSlider, pathSlider);
                    }
                }
            }

            // 3. Renderizar Texto
            if(showValue != TextPosition.None)
                DrawValueText(graph, sliderWidth);
        }

        private void DrawValueText(Graphics graph, int sliderWidth) {
            string text = symbolBefore + this.Value.ToString() + symbolAfter;
            if(showMaximun)
                text = text + "/" + symbolBefore + this.Maximum.ToString() + symbolAfter;

            Size textSize = TextRenderer.MeasureText(text, this.Font);
            Rectangle rectText = new Rectangle(0, (this.Height - textSize.Height) / 2, textSize.Width, textSize.Height);

            switch(showValue) {
                case TextPosition.Left:
                rectText.X = 4;
                break;
                case TextPosition.Right:
                rectText.X = this.Width - textSize.Width - 4;
                break;
                case TextPosition.Center:
                rectText.X = (this.Width - textSize.Width) / 2;
                break;
                case TextPosition.Sliding:
                // Control matemático para evitar que el texto se salga por la izquierda al iniciar
                rectText.X = Math.Max(4, sliderWidth - textSize.Width - 4);
                break;
            }

            // CORRECCIÓN SARA UI: El texto ahora flota de forma plana y transparente, eliminando el recuadro rígido
            TextRenderer.DrawText(graph, text, this.Font, rectText, this.ForeColor, Color.Transparent, TextFormatFlags.VerticalCenter);
        }

        protected override void OnParentBackColorChanged(EventArgs e) {
            base.OnParentBackColorChanged(e);
            this.Invalidate();
        }
    }
}
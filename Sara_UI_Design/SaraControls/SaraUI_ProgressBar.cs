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
    public enum TextPosition {
        Left,
        Right,
        Center,
        Sliding,
        None
    }

    /// <summary>
    /// Barra de progreso personalizada de Sara UI con soporte para canales y sliders de diferentes alturas, 
    /// bordes redondeados, degradados y visualización de texto dinámica.
    /// </summary>
    public class SaraUI_ProgressBar:ProgressBar {
        //Fields
        //-> Appearance
        private Color channelColor = Color.LightSteelBlue;
        private Color sliderColor = Color.RoyalBlue;
        private Color foreBackColor = Color.RoyalBlue;
        private int channelHeight = 6;
        private int sliderHeight = 6;
        private TextPosition showValue = TextPosition.Right;
        private string symbolBefore = "";
        private string symbolAfter = "";
        private bool showMaximun = false;

        //-> Others
        private bool paintedBack = false;
        private bool stopPainting = false;

        public SaraUI_ProgressBar() {
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.ResizeRedraw, true);

            this.ForeColor = Color.White;
        }

        //Properties

        /// <summary>
        /// Obtiene o establece el color de fondo (canal) de la barra de progreso.
        /// </summary>
        [Category("Sara UI Desing")]
        public Color ChannelColor {
            get { return channelColor; }
            set {
                channelColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el color principal del indicador de progreso (slider).
        /// </summary>
        [Category("Sara UI Desing")]
        public Color SliderColor {
            get { return sliderColor; }
            set {
                sliderColor = value;
                this.Invalidate();
            }
        }

        [Category("Sara UI Desing")]
        public Color ForeBackColor {
            get { return foreBackColor; }
            set {
                foreBackColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Determina si el slider debe dibujarse con un degradado lineal entre <see cref="SliderColor"/> y <see cref="SliderColorEnd"/>.
        /// </summary>
        [Category("Sara UI Desing")]
        public bool UseGradient { get; set; } = true;

        [Category("Sara UI Desing")]
        public Color SliderColorEnd { get; set; } = Color.RoyalBlue;

        /// <summary>
        /// Obtiene o establece la altura del canal de fondo en píxeles.
        /// </summary>
        [Category("Sara UI Desing")]
        public int ChannelHeight {
            get { return channelHeight; }
            set {
                channelHeight = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece la altura del indicador de progreso en píxeles.
        /// </summary>
        [Category("Sara UI Desing")]
        public int SliderHeight {
            get { return sliderHeight; }
            set {
                sliderHeight = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece la posición donde se mostrará el valor porcentual o numérico.
        /// </summary>
        [Category("Sara UI Desing")]
        public TextPosition ShowValue {
            get { return showValue; }
            set {
                showValue = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Texto decorativo que se muestra antes del valor numérico (ej. "$").
        /// </summary>
        [Category("Sara UI Desing")]
        public string SymbolBefore {
            get { return symbolBefore; }
            set {
                symbolBefore = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Texto decorativo que se muestra después del valor numérico (ej. "%" o " unidades").
        /// </summary>
        [Category("Sara UI Desing")]
        public string SymbolAfter {
            get { return symbolAfter; }
            set {
                symbolAfter = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Si es verdadero, muestra el valor actual junto al valor máximo (ej. "50/100").
        /// </summary>
        [Category("Sara UI Desing")]
        public bool ShowMaximun {
            get { return showMaximun; }
            set {
                showMaximun = value;
                this.Invalidate();
            }
        }

        [Category("Sara UI Desing")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override Font Font {
            get { return base.Font; }
            set {
                base.Font = value;
            }
        }

        [Category("Sara UI Desing")]
        public override Color ForeColor {
            get { return base.ForeColor; }
            set {
                base.ForeColor = value;
            }
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

        //-> Paint the background & channel
        protected override void OnPaintBackground(PaintEventArgs pevent) {
            if(stopPainting)
                return;

            Graphics graph = pevent.Graphics;
            // Usamos el color del padre para que la transparencia simulada funcione siempre
            graph.Clear(this.Parent?.BackColor ?? Color.White);

            // Dibujamos el canal aquí si quieres mantenerlo separado del slider
            Rectangle rectChannel = new Rectangle(0, 0, this.Width, channelHeight);

            // Centrar el canal verticalmente
            rectChannel.Y = (this.Height - channelHeight) / 2;

            using(var brushChannel = new SolidBrush(channelColor)) {
                graph.FillRectangle(brushChannel, rectChannel);
            }
        }

        /// <summary>
        /// Redibuja el control aplicando el diseño redondeado al canal, el slider con degradado (si está activo) 
        /// y gestiona la llamada para dibujar el texto.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e) {
            Graphics graph = e.Graphics;
            graph.SmoothingMode = SmoothingMode.AntiAlias; // Crucial para bordes suaves

            double scaleFactor = (((double)this.Value - this.Minimum) / ((double)this.Maximum - this.Minimum));
            int sliderWidth = (int)(this.Width * scaleFactor);

            // Dibujar Canal Redondeado
            Rectangle rectChannel = new Rectangle(0, (this.Height - channelHeight) / 2, this.Width, channelHeight);
            using(GraphicsPath pathChannel = GetFigurePath(rectChannel, channelHeight / 2))
            using(SolidBrush brushChannel = new SolidBrush(channelColor)) {
                graph.FillPath(brushChannel, pathChannel);
            }

            // Dibujar Slider Redondeado con Degradado
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

            if(showValue != TextPosition.None)
                DrawValueText(graph, sliderWidth, new Rectangle(0, 0, this.Width, this.Height));
        }

        /// <summary>
        /// Calcula la posición y renderiza el texto informativo del progreso basándose en la alineación seleccionada.
        /// </summary>
        /// <param name="graph">Superficie de dibujo.</param>
        /// <param name="sliderWidth">Ancho actual del indicador de progreso.</param>
        /// <param name="rectSlider">Rectángulo que define el área total del control.</param>
        private void DrawValueText(Graphics graph, int sliderWidth, Rectangle rectSlider) {
            //Fields
            string text = symbolBefore + this.Value.ToString() + symbolAfter;
            if(showMaximun)
                text = text + "/" + symbolBefore + this.Maximum.ToString() + symbolAfter;
            var textSize = TextRenderer.MeasureText(text, this.Font);
            var rectText = new Rectangle(0, 0, textSize.Width, textSize.Height + 2);
            using(var brushText = new SolidBrush(this.ForeColor))
            using(var brushTextBack = new SolidBrush(foreBackColor))
            using(var textFormat = new StringFormat()) {
                switch(showValue) {
                    case TextPosition.Left:
                    rectText.X = 0;
                    textFormat.Alignment = StringAlignment.Near;
                    break;

                    case TextPosition.Right:
                    rectText.X = this.Width - textSize.Width;
                    textFormat.Alignment = StringAlignment.Far;
                    break;

                    case TextPosition.Center:
                    rectText.X = (this.Width - textSize.Width) / 2;
                    textFormat.Alignment = StringAlignment.Center;
                    break;

                    case TextPosition.Sliding:
                    rectText.X = sliderWidth - textSize.Width;
                    textFormat.Alignment = StringAlignment.Center;
                    //Clean previous text surface
                    using(var brushClear = new SolidBrush(this.Parent.BackColor)) {
                        var rect = rectSlider;
                        rect.Y = rectText.Y;
                        rect.Height = rectText.Height;
                        graph.FillRectangle(brushClear, rect);
                    }
                    break;
                }
                //Painting
                graph.FillRectangle(brushTextBack, rectText);
                graph.DrawString(text, this.Font, brushText, rectText, textFormat);
            }
        }

        protected override void OnParentBackColorChanged(EventArgs e) {
            base.OnParentBackColorChanged(e);
            this.Invalidate(); // Fuerza al control a redibujarse completamente
        }
    }
}

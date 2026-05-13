using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {
    [DefaultEvent("Click")] // define el evento por defecto

    /// <summary>
    /// Representa un botón personalizado de la suite Sara UI con soporte para bordes redondeados, 
    /// bordes de color y renderizado de iconos integrados.
    /// </summary>
    public class SaraUI_Button:Button {
        // Campos privados
        private int borderSize = 0;
        private int borderRadius = 20;
        private Color borderColor = Color.PaleVioletRed;

        
        private int iconSize = 16;
        private int iconPadding = 8; // Espacio entre icono y texto
        private Color iconColor = Color.White;
        private SaraIconLocation iconLocation = SaraIconLocation.Left;
        private SaraUI_IconLibrary.SaraIconStyle iconStyle = SaraUI_IconLibrary.SaraIconStyle.Outline;

        private string iconName = "None";

        public enum SaraIconLocation { Left, Right }

        // Campos para efectos visuales
        private bool isMouseOver = false;
        private bool isPressed = false;

        //// Propiedades con Categoría
        //[Category("Sara UI Design")]
        //public int Forecolor {
        //    get => borderSize;
        //    set { borderSize = value; Invalidate(); }
        //}

        /// <summary>
        /// Obtiene o establece el grosor del borde del botón en píxeles.
        /// </summary>
        [Category("Sara UI Design")]
        public int BorderSize {
            get => borderSize;
            set { borderSize = value; Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el radio de redondeo de las esquinas. 
        /// El valor se limita automáticamente a la altura del control para mantener la estética.
        /// </summary>
        [Category("Sara UI Design")]
        public int BorderRadius {
            get => borderRadius;
            set {
                borderRadius = (value <= Height) ? value : Height;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el color del borde del botón.
        /// </summary>
        [Category("Sara UI Design")]
        public Color BorderColor {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el nombre del icono a mostrar. 
        /// Utiliza "None" para no mostrar ningún icono.
        /// </summary>
        [Category("Sara UI Design")]
        [TypeConverter(typeof(IconNameConverter))] 
        public string IconName {
            get => iconName;
            set { iconName = value; Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el tamaño (en píxeles) del icono dentro del botón.
        /// </summary>
        [Category("Sara UI Design")]
        public int IconSize {
            get => iconSize;
            set { iconSize = value; Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el espacio de separación entre el icono y el texto del botón.
        /// </summary>
        [Category("Sara UI Design")]
        public int IconPadding {
            get => iconPadding;
            set { iconPadding = value; Invalidate(); }
        }

        /// <summary>
        /// Define si el icono se posiciona a la izquierda o a la derecha del texto.
        /// </summary>
        [Category("Sara UI Design")]
        public SaraIconLocation IconLocation {
            get => iconLocation;
            set { iconLocation = value; Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el estilo visual del icono (Contorno o Sólido).
        /// </summary>
        [Category("Sara UI Design")]
        public SaraUI_IconLibrary.SaraIconStyle IconStyle {
            get => iconStyle;
            set { iconStyle = value; Invalidate(); }
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="SaraUI_Button"/> 
        /// con estilos por defecto y doble búfer para evitar parpadeos.
        /// </summary>
        public SaraUI_Button() {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Size = new Size(150, 40);
            this.BackColor = Color.MediumSlateBlue;
            this.ForeColor = Color.White;

            // Crucial para evitar parpadeo
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
        }


        protected override void OnMouseEnter(EventArgs e) {
            base.OnMouseEnter(e);
            isMouseOver = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            isMouseOver = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent) {
            base.OnMouseDown(mevent);
            isPressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent) {
            base.OnMouseUp(mevent);
            isPressed = false;
            Invalidate();
        }

        /// <summary>
        /// Genera el camino geométrico para crear los bordes redondeados basándose en un rectángulo y un radio.
        /// </summary>
        /// <param name="rect">El área del rectángulo a redondear.</param>
        /// <param name="radius">El radio de la curvatura.</param>
        /// <returns>Un objeto <see cref="GraphicsPath"/> con la forma del botón.</returns>
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

        /// <summary>
        /// Redibuja el control aplicando los efectos de redondeo, bordes, efectos de hover e iconos.
        /// </summary>
        /// <param name="pevent">Argumentos del evento de dibujo.</param>
        protected override void OnPaint(PaintEventArgs pevent) {
            Graphics graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Limpiar el fondo para evitar el efecto "fantasma"
            graphics.Clear(this.Parent?.BackColor ?? Color.White);

            Rectangle rectSurface = this.ClientRectangle;
            Rectangle rectBorder = Rectangle.Inflate(rectSurface, -borderSize, -borderSize);

            // 1. DIBUJAR FONDO Y BORDES
            using(SolidBrush brushBackground = new SolidBrush(this.BackColor)) {
                if(borderRadius > 2) {
                    using(GraphicsPath pathSurface = GetFigurePath(rectSurface, borderRadius))
                    using(GraphicsPath pathBorder = GetFigurePath(rectBorder, borderRadius - borderSize))
                    using(Pen penSurface = new Pen(this.Parent?.BackColor ?? Color.White, 2))
                    using(Pen penBorder = new Pen(borderColor, borderSize)) {
                        this.Region = new Region(pathSurface);
                        graphics.FillPath(brushBackground, pathSurface);
                        graphics.DrawPath(penSurface, pathSurface);
                        if(borderSize >= 1)
                            graphics.DrawPath(penBorder, pathBorder);
                    }
                } else {
                    this.Region = new Region(rectSurface);
                    graphics.FillRectangle(brushBackground, rectSurface);
                    if(borderSize >= 1) {
                        using(Pen penBorder = new Pen(borderColor, borderSize)) {
                            penBorder.Alignment = PenAlignment.Inset;
                            graphics.DrawRectangle(penBorder, 0, 0, Width - 1, Height - 1);
                        }
                    }
                }
            }

            // 2. EFECTO HOVER
            if(isMouseOver) {
                using(SolidBrush hoverBrush = new SolidBrush(Color.FromArgb(30, Color.White))) {
                    graphics.FillRectangle(hoverBrush, rectSurface);
                }
            }

            // 3. LÓGICA DE ICONO + TEXTO UNIDOS
            Size textSize = TextRenderer.MeasureText(this.Text, this.Font);
            bool hasIcon = !string.IsNullOrEmpty(iconName) && iconName != "None";

            // Calcular el ancho total del bloque (Icono + Padding + Texto)
            int totalContentWidth = !hasIcon ? textSize.Width : iconSize + iconPadding + textSize.Width;

            int startX = (this.Width - totalContentWidth) / 2;
            int currentX = startX;
            int iconY = (this.Height - iconSize) / 2;
            int textY = (this.Height - textSize.Height) / 2;

            if(hasIcon) {
                iconColor = ForeColor;
                if(iconLocation == SaraIconLocation.Left) {
                    // CASO: ICONO IZQUIERDA
                    Rectangle iconRect = new Rectangle(currentX, iconY, iconSize, iconSize);
                    SaraUI_IconLibrary.DrawIcon(iconName, graphics, iconRect, iconColor, iconStyle);

                    currentX += iconSize + iconPadding;
                    DrawButtonText(graphics, currentX, textY, textSize);
                } else {
                    // CASO: ICONO DERECHA
                    DrawButtonText(graphics, currentX, textY, textSize);

                    currentX += textSize.Width + iconPadding;
                    Rectangle iconRect = new Rectangle(currentX, iconY, iconSize, iconSize);
                    SaraUI_IconLibrary.DrawIcon(iconName, graphics, iconRect, iconColor, iconStyle);
                }
            } else {
                // CASO: SIN ICONO (Solo texto centrado)
                DrawButtonText(graphics, currentX, textY, textSize);
            }
        }

        /// <summary>
        /// Método auxiliar para renderizar el texto del botón de forma nítida usando TextRenderer.
        /// </summary>
        /// <param name="g">Superficie de dibujo.</param>
        /// <param name="x">Posición horizontal.</param>
        /// <param name="y">Posición vertical.</param>
        /// <param name="tSize">Dimensiones medidas del texto.</param>
        private void DrawButtonText(Graphics g, int x, int y, Size tSize) {
            Rectangle textRect = new Rectangle(x, y, tSize.Width, tSize.Height);
            TextRenderer.DrawText(g, this.Text, this.Font, textRect, this.ForeColor, Color.Transparent, TextFormatFlags.Default);
        }
    }
}
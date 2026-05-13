using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Barra de desplazamiento (ScrollBar) personalizada de la suite Sara UI. 
    /// Permite un control total sobre la estética del canal y el deslizador (thumb), 
    /// con soporte para orientación vertical y horizontal.
    /// </summary>
    [DefaultEvent("ValueChanged")]
    public class SaraUI_ScrollBar:Control {
        // Enums
        public enum ScrollOrientation { Horizontal, Vertical }

        // Fields de Lógica
        private int minimum = 0;
        private int maximum = 100;
        private int value = 0;
        private int largeChange = 10;
        private ScrollOrientation orientation = ScrollOrientation.Vertical;

        // Fields de Diseño
        private Color channelColor = Color.FromArgb(224, 224, 230);
        private Color thumbColor = Color.MediumSlateBlue;
        private int borderRadius = 5;

        // Estado del mouse
        private bool isDragging = false;
        private Point dragPoint;

        // Eventos

        /// <summary>
        /// Ocurre cuando la propiedad <see cref="Value"/> ha cambiado, ya sea por interacción del usuario o por código.
        /// </summary>
        public event EventHandler ValueChanged;

        // Propiedades de Lógica en Sara UI Design

        /// <summary>
        /// Obtiene o establece el valor límite inferior del rango de desplazamiento.
        /// </summary>
        [Category("Sara UI Design Logic")]
        public int Minimum { get => minimum; set { minimum = value; this.Invalidate(); } }

        /// <summary>
        /// Obtiene o establece el valor límite superior del rango de desplazamiento.
        /// </summary>
        [Category("Sara UI Design Logic")]
        public int Maximum { get => maximum; set { maximum = value; this.Invalidate(); } }

        /// <summary>
        /// Obtiene o establece la posición actual del deslizador. 
        /// El valor se ajusta automáticamente para permanecer dentro del rango definido.
        /// </summary>
        [Category("Sara UI Design Logic")]
        public int Value {
            get => value;
            set {
                this.value = Math.Max(minimum, Math.Min(maximum, value));
                ValueChanged?.Invoke(this, EventArgs.Empty);
                this.Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece la cantidad que se suma o resta al valor cuando se hace clic 
        /// en el canal o se usa para calcular el tamaño proporcional del deslizador.
        /// </summary>
        [Category("Sara UI Design Logic")]
        public int LargeChange { get => largeChange; set { largeChange = value; this.Invalidate(); } }

        /// <summary>
        /// Define si la barra de desplazamiento se orienta de forma Vertical u Horizontal. 
        /// Al cambiarla, el control intercambia automáticamente sus dimensiones de ancho y alto.
        /// </summary>
        [Category("Sara UI Design Logic")]
        public ScrollOrientation Orientation {
            get => orientation;
            set { orientation = value; this.Size = new Size(this.Height, this.Width); this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el color de fondo del canal por donde se mueve el deslizador.
        /// </summary>
        [Category("Sara UI Design Appearance")]
        public Color ChannelColor { get => channelColor; set { channelColor = value; this.Invalidate(); } }

        /// <summary>
        /// Obtiene o establece el color de la pieza móvil (deslizador) de la barra.
        /// </summary>
        [Category("Sara UI Design Appearance")]
        public Color ThumbColor { get => thumbColor; set { thumbColor = value; this.Invalidate(); } }

        /// <summary>
        /// Obtiene o establece el radio de redondeo para los extremos del canal y del deslizador.
        /// </summary>
        [Category("Sara UI Design Appearance")]
        public int BorderRadius { get => borderRadius; set { borderRadius = value; this.Invalidate(); } }

        public SaraUI_ScrollBar() {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.Size = new Size(10, 150); // Tamaño inicial por defecto
            this.BackColor = Color.White;
        }

        // --- Lógica del Mouse ---
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            Rectangle thumbRect = GetThumbRectangle();
            if(thumbRect.Contains(e.Location)) {
                isDragging = true;
                dragPoint = e.Location;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if(isDragging) {
                float totalSpace = maximum - minimum;
                if(totalSpace <= 0)
                    return;

                if(orientation == ScrollOrientation.Vertical) {
                    float availableHeight = this.Height - GetThumbSize();
                    int deltaY = e.Y - dragPoint.Y;
                    float percentShift = deltaY / availableHeight;
                    Value += (int)(percentShift * totalSpace);
                    dragPoint = e.Location; // Actualizar punto de arrastre
                } else {
                    float availableWidth = this.Width - GetThumbSize();
                    int deltaX = e.X - dragPoint.X;
                    float percentShift = deltaX / availableWidth;
                    Value += (int)(percentShift * totalSpace);
                    dragPoint = e.Location; // Actualizar punto de arrastre
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);
            isDragging = false;
        }

        // --- Lógica de Dibujo ---

        /// <summary>
        /// Renderiza los componentes de la barra de desplazamiento: el canal de fondo y el deslizador móvil.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(this.Parent?.BackColor ?? Color.White);

            Rectangle rectClient = this.ClientRectangle;

            // 1. Dibujar Canal (Fondo del scroll)
            using(GraphicsPath pathChannel = GetFigurePath(rectClient, borderRadius))
            using(SolidBrush brushChannel = new SolidBrush(channelColor)) {
                g.FillPath(brushChannel, pathChannel);
            }

            // 2. Dibujar el Thumb (El deslizador)
            if(maximum > minimum) {
                Rectangle thumbRect = GetThumbRectangle();
                using(GraphicsPath pathThumb = GetFigurePath(thumbRect, borderRadius - 1))
                using(SolidBrush brushThumb = new SolidBrush(thumbColor)) {
                    g.FillPath(brushThumb, pathThumb);
                }
            }
        }

        /// <summary>
        /// Calcula el tamaño (largo o ancho) del deslizador de forma proporcional al rango 
        /// definido por el <see cref="Maximum"/> y <see cref="LargeChange"/>.
        /// </summary>
        /// <returns>Tamaño en píxeles del deslizador.</returns>
        private int GetThumbSize() {
            if(maximum <= minimum)
                return 0;
            float ratio = (float)largeChange / (maximum - minimum + largeChange);
            int size = (orientation == ScrollOrientation.Vertical ? this.Height : this.Width);
            int thumbSize = (int)(size * ratio);
            return Math.Max(thumbSize, 20); // Tamaño mínimo de 20px
        }

        /// <summary>
        /// Calcula la posición y el área rectangular exacta que debe ocupar el deslizador 
        /// basándose en el valor actual y la orientación.
        /// </summary>
        /// <returns>Un objeto <see cref="Rectangle"/> con las coordenadas del deslizador.</returns>
        private Rectangle GetThumbRectangle() {
            int thumbSize = GetThumbSize();
            float ratio = (float)(value - minimum) / (maximum - minimum);

            if(orientation == ScrollOrientation.Vertical) {
                int availableHeight = this.Height - thumbSize;
                int y = (int)(ratio * availableHeight);
                return new Rectangle(0, y, this.Width, thumbSize);
            } else {
                int availableWidth = this.Width - thumbSize;
                int x = (int)(ratio * availableWidth);
                return new Rectangle(x, 0, thumbSize, this.Height);
            }
        }

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
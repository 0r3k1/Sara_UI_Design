using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Barra de desplazamiento (ScrollBar) personalizada de la suite Sara UI con control preciso de arrastre matemático.
    /// </summary>
    [DefaultEvent("ValueChanged")]
    public class SaraUI_ScrollBar:Control {

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

        private bool isDragging = false;
        private int dragOffset = 0; // Guardar el punto exacto de presión interna en el Thumb

        public event EventHandler ValueChanged;

        [Category("Sara UI Design Logic")]
        public int Minimum { get => minimum; set { minimum = value; this.Invalidate(); } }

        [Category("Sara UI Design Logic")]
        public int Maximum { get => maximum; set { maximum = value; this.Invalidate(); } }

        [Category("Sara UI Design Logic")]
        public int Value {
            get => value;
            set {
                int newValue = Math.Max(minimum, Math.Min(maximum, value));
                if(this.value != newValue) {
                    this.value = newValue;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                    this.Invalidate();
                }
            }
        }

        [Category("Sara UI Design Logic")]
        public int LargeChange { get => largeChange; set { largeChange = value; this.Invalidate(); } }

        [Category("Sara UI Design Logic")]
        public ScrollOrientation Orientation {
            get => orientation;
            set {
                orientation = value;
                this.Size = new Size(this.Height, this.Width);
                this.Invalidate();
            }
        }

        [Category("Sara UI Design Appearance")]
        public Color ChannelColor { get => channelColor; set { channelColor = value; this.Invalidate(); } }

        [Category("Sara UI Design Appearance")]
        public Color ThumbColor { get => thumbColor; set { thumbColor = value; this.Invalidate(); } }

        [Category("Sara UI Design Appearance")]
        public int BorderRadius { get => borderRadius; set { borderRadius = value; this.Invalidate(); } }

        public SaraUI_ScrollBar() {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.Size = new Size(10, 150);
            this.BackColor = Color.White;
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            Rectangle thumbRect = GetThumbRectangle();
            if(thumbRect.Contains(e.Location)) {
                isDragging = true;
                // Guardamos la distancia exacta desde el borde del thumb al clic para un movimiento natural
                dragOffset = (orientation == ScrollOrientation.Vertical) ? e.Y - thumbRect.Y : e.X - thumbRect.X;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if(isDragging) {
                int totalSpace = maximum - minimum;
                if(totalSpace <= 0)
                    return;

                // REFACTORIZACIÓN MATEMÁTICA: Arrastre absoluto sin saltos ni desbordamientos por delta
                if(orientation == ScrollOrientation.Vertical) {
                    int availableHeight = this.Height - GetThumbSize();
                    if(availableHeight > 0) {
                        int trackY = e.Y - dragOffset;
                        float ratio = (float)trackY / availableHeight;
                        Value = minimum + (int)(ratio * totalSpace);
                    }
                } else {
                    int availableWidth = this.Width - GetThumbSize();
                    if(availableWidth > 0) {
                        int trackX = e.X - dragOffset;
                        float ratio = (float)trackX / availableWidth;
                        Value = minimum + (int)(ratio * totalSpace);
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);
            isDragging = false;
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(this.Parent?.BackColor ?? Color.White);

            Rectangle rectClient = this.ClientRectangle;

            using(GraphicsPath pathChannel = GetFigurePath(rectClient, borderRadius))
            using(SolidBrush brushChannel = new SolidBrush(channelColor)) {
                g.FillPath(brushChannel, pathChannel);
            }

            if(maximum > minimum) {
                Rectangle thumbRect = GetThumbRectangle();
                using(GraphicsPath pathThumb = GetFigurePath(thumbRect, borderRadius - 1))
                using(SolidBrush brushThumb = new SolidBrush(thumbColor)) {
                    g.FillPath(brushThumb, pathThumb);
                }
            }
        }

        private int GetThumbSize() {
            if(maximum <= minimum)
                return 0;
            float ratio = (float)largeChange / (maximum - minimum + largeChange);
            int size = (orientation == ScrollOrientation.Vertical ? this.Height : this.Width);
            int thumbSize = (int)(size * ratio);
            return Math.Max(thumbSize, 20);
        }

        private Rectangle GetThumbRectangle() {
            int thumbSize = GetThumbSize();
            float ratio = (maximum > minimum) ? (float)(value - minimum) / (maximum - minimum) : 0;

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
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Selector de fecha personalizado de la suite Sara UI con soporte para esquinas redondeadas, 
    /// personalización de colores de fondo y bordes, e integración de iconos vectoriales dinámicos.
    /// </summary>
    [ToolboxItem(true)]
    public class SaraUI_DatePicker:DateTimePicker {
        // Fields
        private Color skinColor = Color.MediumSlateBlue;
        private Color textColor = Color.White;
        private Color borderColor = Color.PaleVioletRed;
        private int borderSize = 0;
        private int borderRadius = 12; // Subido a 12 para un acabado más suave y moderno
        private bool droppedDown = false;

        /// <summary>
        /// Obtiene o establece el color de fondo principal del control.
        /// </summary>
        [Category("Sara UI Design")]
        public Color SkinColor {
            get => skinColor;
            set { skinColor = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el color de la fuente y del icono del calendario.
        /// </summary>
        [Category("Sara UI Design")]
        public Color TextColor {
            get => textColor;
            set { textColor = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el color del borde decorativo del control.
        /// </summary>
        [Category("Sara UI Design")]
        public Color BorderColor {
            get => borderColor;
            set { borderColor = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el grosor del borde en píxeles. Use 0 para eliminar el borde.
        /// </summary>
        [Category("Sara UI Design")]
        public int BorderSize {
            get => borderSize;
            set { borderSize = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el radio de curvatura de las esquinas del control.
        /// </summary>
        [Category("Sara UI Design")]
        public int BorderRadius {
            get => borderRadius;
            set { borderRadius = value; this.Invalidate(); }
        }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_DatePicker"/> activando el dibujo de usuario 
        /// y definiendo un tamaño y fuente predeterminados para un diseño moderno.
        /// </summary>
        public SaraUI_DatePicker() {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.MinimumSize = new Size(0, 35);
            this.Font = new Font("Segoe UI", 9.5F);
        }

        protected override void OnDropDown(EventArgs eventargs) { base.OnDropDown(eventargs); droppedDown = true; this.Invalidate(); }
        protected override void OnCloseUp(EventArgs eventargs) { base.OnCloseUp(eventargs); droppedDown = false; this.Invalidate(); }

        /// <summary>
        /// Invalida la entrada por teclado para asegurar que el control funcione solo mediante la selección visual.
        /// </summary>
        protected override void OnKeyPress(KeyPressEventArgs e) { base.OnKeyPress(e); e.Handled = true; }

        /// <summary>
        /// Redibuja completamente el control aplicando el fondo redondeado, la cadena de fecha, 
        /// el icono de calendario desde la librería dinámica de SaraUI y el borde opcional.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e) {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Evitamos imperfecciones pintando el fondo exterior con el color del contenedor padre
            graphics.Clear(this.Parent?.BackColor ?? Color.White);

            Rectangle rectClient = this.ClientRectangle;

            // 1. Dibujar Fondo Redondeado
            using(GraphicsPath pathBack = GetFigurePath(rectClient, borderRadius)) {
                using(SolidBrush brushBack = new SolidBrush(skinColor)) {
                    graphics.FillPath(brushBack, pathBack);
                }
            }

            // 2. Dibujar Texto Dinámico (Ajuste de margen inteligente basado en el radio)
            int textLeftPadding = Math.Max(12, borderRadius / 2 + 6);
            using(SolidBrush brushText = new SolidBrush(textColor))
            using(StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center }) {
                Rectangle textRect = new Rectangle(textLeftPadding, 0, this.Width - (textLeftPadding + 35), this.Height);
                graphics.DrawString(this.Text, this.Font, brushText, textRect, sf);
            }

            // 3. LLAMADA INTEGRADA A TU NUEVA ICONLIBRARY
            Rectangle iconRect = new Rectangle(this.Width - 28, (this.Height - 16) / 2, 16, 16);
            SaraUI_IconLibrary.DrawIcon("Calendar", graphics, iconRect, textColor);

            // 4. Borde Vectorial Inteligente
            if(borderSize > 0) {
                using(Pen penBorder = new Pen(borderColor, borderSize)) {
                    penBorder.Alignment = PenAlignment.Inset;
                    using(GraphicsPath pathBorder = GetFigurePath(rectClient, borderRadius))
                        graphics.DrawPath(penBorder, pathBorder);
                }
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
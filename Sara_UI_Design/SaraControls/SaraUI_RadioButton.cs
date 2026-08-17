using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Representa un botón de opción (RadioButton) personalizado de la suite Sara UI con estilos modernos 
    /// y soporte para colores personalizados en estados marcados, no marcados y al pasar el ratón.
    /// </summary>
    [ToolboxItem(true)]
    public class SaraUI_RadioButton:RadioButton {
        private Color checkedColor = Color.MediumSlateBlue;
        private Color unCheckedColor = Color.Gray;
        private bool isHovering = false;

        /// <summary>
        /// Obtiene o establece el color del círculo interno y del borde cuando el control está seleccionado (Checked).
        /// </summary>
        [Category("Sara UI Design")]
        public Color CheckedColor { get => checkedColor; set { checkedColor = value; Invalidate(); } }

        /// <summary>
        /// Obtiene o establece el color del borde cuando el control no está seleccionado.
        /// </summary>
        [Category("Sara UI Design")]
        public Color UnCheckedColor { get => unCheckedColor; set { unCheckedColor = value; Invalidate(); } }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_RadioButton"/> definiendo un tamaño mínimo, 
        /// espaciado de texto y cambiando el cursor a mano para una mejor experiencia de usuario.
        /// </summary>
        public SaraUI_RadioButton() {
            this.MinimumSize = new Size(0, 21);
            this.Padding = new Padding(10, 0, 0, 0);
            this.Cursor = Cursors.Hand;

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); isHovering = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); isHovering = false; Invalidate(); }

        /// <summary>
        /// Redibuja completamente el control, calculando las áreas para el círculo exterior, 
        /// el círculo de selección interno y el texto alineado de forma nítida.
        /// </summary>
        protected override void OnPaint(PaintEventArgs pevent) {
            Graphics graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // CORRECCIÓN BUG FONDO: Limpieza basada en el contenedor padre para evitar el recuadro gris
            graphics.Clear(this.Parent?.BackColor ?? Color.White);

            float rbSize = 18F;
            float checkSize = 10F;
            RectangleF rectBorder = new RectangleF(1, (Height - rbSize) / 2f, rbSize, rbSize);
            RectangleF rectCheck = new RectangleF(rectBorder.X + (rbSize - checkSize) / 2f, (Height - checkSize) / 2f, checkSize, checkSize);

            Color renderColor = Checked ? checkedColor : (isHovering ? Color.DarkGray : unCheckedColor);

            using(Pen penBorder = new Pen(renderColor, 2F))
            using(SolidBrush brushCheck = new SolidBrush(checkedColor)) {
                graphics.DrawEllipse(penBorder, rectBorder);
                if(Checked)
                    graphics.FillEllipse(brushCheck, rectCheck);

                // OPTIMIZACIÓN SARA UI: Dibujado de texto usando TextRenderer nativo
                Size textSize = TextRenderer.MeasureText(this.Text, this.Font);
                int textX = (int)rbSize + 8;
                int textY = (Height - textSize.Height) / 2;
                Rectangle textRect = new Rectangle(textX, textY, this.Width - textX, textSize.Height);

                TextRenderer.DrawText(graphics, this.Text, this.Font, textRect, this.ForeColor, Color.Transparent, TextFormatFlags.VerticalCenter);
            }
        }
    }
}
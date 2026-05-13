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

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        /// <summary>
        /// Activa el estado de resaltado (Hover) para cambiar visualmente el color del borde.
        /// </summary>
        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); isHovering = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); isHovering = false; Invalidate(); }

        /// <summary>
        /// Redibuja completamente el control, calculando las áreas para el círculo exterior, 
        /// el círculo de selección interno y el texto alineado.
        /// </summary>
        /// <param name="pevent">Argumentos del evento de dibujo.</param>
        protected override void OnPaint(PaintEventArgs pevent) {
            Graphics graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(this.BackColor);

            float rbSize = 18F;
            float checkSize = 10F;
            RectangleF rectBorder = new RectangleF(1, (Height - rbSize) / 2, rbSize, rbSize);
            RectangleF rectCheck = new RectangleF(rectBorder.X + (rbSize - checkSize) / 2, (Height - checkSize) / 2, checkSize, checkSize);

            Color renderColor = Checked ? checkedColor : (isHovering ? Color.DarkGray : unCheckedColor);

            using(Pen penBorder = new Pen(renderColor, 2F))
            using(SolidBrush brushCheck = new SolidBrush(checkedColor))
            using(SolidBrush brushText = new SolidBrush(this.ForeColor)) {
                graphics.DrawEllipse(penBorder, rectBorder);
                if(Checked)
                    graphics.FillEllipse(brushCheck, rectCheck);

                Size textSize = TextRenderer.MeasureText(this.Text, this.Font);
                graphics.DrawString(Text, Font, brushText, rbSize + 8, (Height - textSize.Height) / 2);
            }
        }
    }
}
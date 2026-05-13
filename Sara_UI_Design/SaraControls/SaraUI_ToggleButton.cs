using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Representa un interruptor de palanca (Toggle Switch) personalizado de la suite Sara UI. 
    /// Sustituye la apariencia del CheckBox estándar por una interfaz de deslizamiento 
    /// con estilos sólidos o de contorno.
    /// </summary>
    public class SaraUI_ToggleButton:CheckBox {
        // Campos
        private Color onBackColor = Color.MediumSlateBlue;
        private Color onToggleColor = Color.WhiteSmoke;
        private Color offBackColor = Color.Gray;
        private Color offToggleColor = Color.Gainsboro;
        private bool solidStyle = true;

        // Propiedades

        /// <summary>
        /// Obtiene o establece el color de fondo del interruptor cuando está en estado activado (Checked).
        /// </summary>
        [Category("Sara UI Design")]
        public Color OnBackColor {
            get => onBackColor;
            set { onBackColor = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el color del círculo deslizante cuando el interruptor está activado.
        /// </summary>
        [Category("Sara UI Design")]
        public Color OnToggleColor {
            get => onToggleColor;
            set { onToggleColor = value; this.Invalidate(); }
        }


        /// <summary>
        /// Obtiene o establece el color de fondo del interruptor cuando está en estado desactivado.
        /// </summary>
        [Category("Sara UI Design")]
        public Color OffBackColor {
            get => offBackColor;
            set { offBackColor = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el color del círculo deslizante cuando el interruptor está desactivado.
        /// </summary>
        [Category("Sara UI Design")]
        public Color OffToggleColor {
            get => offToggleColor;
            set { offToggleColor = value; this.Invalidate(); }
        }

        [Browsable(false)]
        public override string Text {
            get => base.Text;
            set { }
        }


        /// <summary>
        /// Obtiene o establece si el interruptor se dibuja con un relleno sólido (true) 
        /// o únicamente con un contorno (false).
        /// </summary>
        [Category("Sara UI Design")]
        [DefaultValue(true)]
        public bool SolidStyle {
            get => solidStyle;
            set { solidStyle = value; this.Invalidate(); }
        }


        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_ToggleButton"/> definiendo un tamaño mínimo, 
        /// el cursor de mano y habilitando el doble búfer para transiciones visuales suaves.
        /// </summary>
        public SaraUI_ToggleButton() {
            this.MinimumSize = new Size(45, 22);
            this.Cursor = Cursors.Hand;
            // Evitar parpadeo (Flickering)
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
        }

        // Métodos

        /// <summary>
        /// Genera el trazado geométrico en forma de cápsula para el cuerpo del interruptor, 
        /// calculando arcos basados en la altura del control.
        /// </summary>
        /// <returns>Un objeto <see cref="GraphicsPath"/> con la silueta del control.</returns>
        private GraphicsPath GetFigurePath() {
            int arcSize = this.Height - 1;
            Rectangle leftArc = new Rectangle(0, 0, arcSize, arcSize);
            Rectangle rightArc = new Rectangle(this.Width - arcSize - 1, 0, arcSize, arcSize);

            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(leftArc, 90, 180);
            path.AddArc(rightArc, 270, 180);
            path.CloseFigure();

            return path;
        }

        protected override void OnPaint(PaintEventArgs pevent) {
            Graphics graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Limpiar el fondo con el color del padre para evitar bordes extraños
            graphics.Clear(this.Parent?.BackColor ?? Color.White);

            int toggleSize = this.Height - 5;
            Rectangle rectToggle;

            if(this.Checked) { // ESTADO ON
                // Dibujar superficie
                if(solidStyle) {
                    using(SolidBrush brush = new SolidBrush(onBackColor))
                        graphics.FillPath(brush, GetFigurePath());
                } else {
                    using(Pen pen = new Pen(onBackColor, 2))
                        graphics.DrawPath(pen, GetFigurePath());
                }

                // Dibujar el círculo (Toggle) a la derecha
                rectToggle = new Rectangle(this.Width - this.Height + 2, 2, toggleSize, toggleSize);
                using(SolidBrush brush = new SolidBrush(onToggleColor))
                    graphics.FillEllipse(brush, rectToggle);

            } else { // ESTADO OFF
                // Dibujar superficie
                if(solidStyle) {
                    using(SolidBrush brush = new SolidBrush(offBackColor))
                        graphics.FillPath(brush, GetFigurePath());
                } else {
                    using(Pen pen = new Pen(offBackColor, 2))
                        graphics.DrawPath(pen, GetFigurePath());
                }

                // Dibujar el círculo (Toggle) a la izquierda
                rectToggle = new Rectangle(3, 2, toggleSize, toggleSize);
                using(SolidBrush brush = new SolidBrush(offToggleColor))
                    graphics.FillEllipse(brush, rectToggle);
            }
        }
    }
}
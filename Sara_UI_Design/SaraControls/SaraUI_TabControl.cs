using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Linq;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Control de pestañas personalizado de la suite Sara UI. 
    /// Ofrece un diseño moderno con bordes redondeados, efectos de hover, 
    /// soporte para iconos y la capacidad de expandir las pestañas para ocupar todo el ancho disponible.
    /// </summary>
    public class SaraUI_TabControl:TabControl {
        // Campos
        private Color selectedTabColor = Color.MediumSlateBlue;
        private Color unselectedTabColor = Color.FromArgb(230, 230, 240);
        private Color selectedTextColor = Color.White;
        private bool stretchTabs = false;
        private int hoverIndex = -1;
        private Color contentBackColor = Color.White;

        // Propiedades

        /// <summary>
        /// Obtiene o establece el color de fondo del área de contenido (TabPage).
        /// </summary>
        [Category("Sara UI Design")]
        public Color ContentBackColor {
            get => contentBackColor;
            set {
                contentBackColor = value;
                UpdateTabPagesColor(); // Actualizamos todas las páginas
                this.Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el color de fondo de la pestaña que se encuentra seleccionada actualmente.
        /// </summary>
        [Category("Sara UI Design")]
        public Color SelectedTabColor {
            get => selectedTabColor;
            set { selectedTabColor = value; this.Invalidate(); }
        }


        /// <summary>
        /// Obtiene o establece si las pestañas deben estirarse automáticamente para cubrir 
        /// todo el ancho horizontal del control.
        /// </summary>
        [Category("Sara UI Design")]
        public bool StretchTabs {
            get => stretchTabs;
            set {
                stretchTabs = value;
                this.SizeMode = value ? TabSizeMode.Fixed : TabSizeMode.Normal;
                UpdateTabSize();
                this.Invalidate();
            }
        }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_TabControl"/>, activando el modo 
        /// de dibujo por usuario (OwnerDraw) y configurando el búfer doble para evitar parpadeos.
        /// </summary>
        public SaraUI_TabControl() {
            this.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.Padding = new Point(20, 10);
        }

        private void UpdateTabPagesColor() {
            foreach(TabPage page in this.TabPages) {
                page.BackColor = contentBackColor;
            }
        }

        protected override void OnControlAdded(ControlEventArgs e) {
            base.OnControlAdded(e);
            if(e.Control is TabPage page) {
                page.BackColor = contentBackColor;
            }
        }


        /// <summary>
        /// Recalcula el tamaño de cada pestaña basándose en el ancho total del control 
        /// cuando la propiedad <see cref="StretchTabs"/> está activa.
        /// </summary>
        private void UpdateTabSize() {
            if(stretchTabs && this.Width > 0 && this.TabCount > 0) {
                int newWidth = (this.Width / this.TabCount) - 1;
                this.ItemSize = new Size(newWidth, 35);
            }
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            UpdateTabSize();
        }

        /// <summary>
        /// Gestiona la detección de la pestaña bajo el cursor para aplicar el efecto visual de "Hover".
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            for(int i = 0; i < this.TabCount; i++) {
                if(GetTabRect(i).Contains(e.Location)) {
                    if(hoverIndex != i) {
                        hoverIndex = i;
                        this.Invalidate();
                    }
                    return;
                }
            }
            if(hoverIndex != -1) {
                hoverIndex = -1;
                this.Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            hoverIndex = -1;
            this.Invalidate();
        }

        /// <summary>
        /// Redibuja completamente el control, incluyendo el fondo del área de pestañas, 
        /// el cuerpo de cada pestaña (seleccionada, normal o hover), los iconos y el texto centrado.
        /// </summary>
        /// <param name="e">Argumentos del evento de dibujo.</param>
        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Fondo del área de pestañas (usamos el color del padre para limpieza)
            g.Clear(this.Parent?.BackColor ?? Color.White);

            // 2. Dibujar cada pestaña
            for(int i = 0; i < this.TabCount; i++) {
                Rectangle tabRect = GetTabRect(i);
                bool isSelected = (this.SelectedIndex == i);
                bool isHover = (hoverIndex == i);

                Color bgColor = isSelected ? selectedTabColor : (isHover ? Color.FromArgb(210, 210, 230) : unselectedTabColor);

                using(GraphicsPath path = GetTabPath(tabRect))
                using(SolidBrush brush = new SolidBrush(bgColor)) {
                    g.FillPath(brush, path);
                }

                if(isSelected) {
                    using(SolidBrush accentBrush = new SolidBrush(Color.White)) {
                        g.FillRectangle(accentBrush, tabRect.X + 15, tabRect.Bottom - 4, tabRect.Width - 30, 3);
                    }
                }

                // Icono
                int iconOffset = 10;
                if(this.ImageList != null && this.TabPages[i].ImageIndex >= 0) {
                    Image img = this.ImageList.Images[this.TabPages[i].ImageIndex];
                    g.DrawImage(img, tabRect.X + 10, tabRect.Y + (tabRect.Height - 16) / 2, 16, 16);
                    iconOffset = 30;
                }

                // Texto
                TextRenderer.DrawText(g, this.TabPages[i].Text, this.Font, tabRect,
                     isSelected ? selectedTextColor : Color.DimGray,
                     TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            }

            // 3. Sincronizar fondo de la página activa (AHORA USA LA VARIABLE)
            if(this.SelectedTab != null && this.SelectedTab.BackColor != contentBackColor) {
                this.SelectedTab.BackColor = contentBackColor;
            }
        }

        /// <summary>
        /// Genera el trazado geométrico para una pestaña, aplicando esquinas redondeadas 
        /// únicamente en la parte superior para mantener la unión con el panel de contenido.
        /// </summary>
        /// <param name="rect">Rectángulo que define el área de la pestaña.</param>
        /// <returns>Un objeto <see cref="GraphicsPath"/> con la forma de la pestaña.</returns>
        private GraphicsPath GetTabPath(Rectangle rect) {
            GraphicsPath path = new GraphicsPath();
            int radius = 8;
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
            path.CloseFigure();
            return path;
        }
    }
}
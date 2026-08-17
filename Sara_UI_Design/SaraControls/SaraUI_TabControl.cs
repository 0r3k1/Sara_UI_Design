using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Control de pestañas personalizado de la suite Sara UI. 
    /// Ofrece un diseño moderno con bordes redondeados, efectos de hover, 
    /// soporte para iconos organizados y alineación fluida expansiva.
    /// </summary>
    [ToolboxItem(true)]
    public class SaraUI_TabControl:TabControl {
        // Fields
        private Color selectedTabColor = Color.MediumSlateBlue;
        private Color unselectedTabColor = Color.FromArgb(230, 230, 240);
        private Color selectedTextColor = Color.White;
        private bool stretchTabs = false;
        private int hoverIndex = -1;
        private Color contentBackColor = Color.White;
        private int tabRadius = 10; // ¡NUEVO!: Control de redondeado premium

        /// <summary>
        /// Obtiene o establece el color de fondo del área de contenido (TabPage).
        /// </summary>
        [Category("Sara UI Design")]
        public Color ContentBackColor {
            get => contentBackColor;
            set {
                contentBackColor = value;
                UpdateTabPagesColor();
                this.Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el color de fondo de la pestaña seleccionada actualmente.
        /// </summary>
        [Category("Sara UI Design")]
        public Color SelectedTabColor {
            get => selectedTabColor;
            set { selectedTabColor = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el radio de curvatura superior para las pestañas de navegación.
        /// </summary>
        [Category("Sara UI Design")]
        public int TabRadius {
            get => tabRadius;
            set { tabRadius = (value >= 0) ? value : 0; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece si las pestañas deben estirarse automáticamente para cubrir todo el ancho horizontal del control.
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

        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Limpieza exterior del área de pestañas
            g.Clear(this.Parent?.BackColor ?? Color.White);

            // 2. Renderizado individual de pestañas
            for(int i = 0; i < this.TabCount; i++) {
                Rectangle tabRect = GetTabRect(i);
                bool isSelected = (this.SelectedIndex == i);
                bool isHover = (hoverIndex == i);

                // Variación fluida de color según el estado
                Color bgColor = isSelected ? selectedTabColor : (isHover ? Color.FromArgb(215, 215, 235) : unselectedTabColor);

                using(GraphicsPath path = GetTabPath(tabRect, tabRadius))
                using(SolidBrush brush = new SolidBrush(bgColor)) {
                    g.FillPath(brush, path);
                }

                // Línea indicadora blanca inferior para la pestaña activa
                if(isSelected) {
                    using(SolidBrush accentBrush = new SolidBrush(Color.White)) {
                        g.FillRectangle(accentBrush, tabRect.X + 15, tabRect.Bottom - 4, tabRect.Width - 30, 3);
                    }
                }

                // Cálculo dinámico para evitar colisión de Iconos y Texto
                int iconOffset = 0;
                if(this.ImageList != null && this.TabPages[i].ImageIndex >= 0) {
                    Image img = this.ImageList.Images[this.TabPages[i].ImageIndex];
                    int imgY = tabRect.Y + (tabRect.Height - 16) / 2;
                    g.DrawImage(img, tabRect.X + 12, imgY, 16, 16);
                    iconOffset = 24; // Espacio que consume el icono
                }

                // ¡SOLUCIÓN BUG VISUAL!: Desplazamos el área de texto si hay un icono presente
                Rectangle textRect = new Rectangle(
                    tabRect.X + iconOffset,
                    tabRect.Y,
                    tabRect.Width - iconOffset,
                    tabRect.Height
                );

                TextRenderer.DrawText(g, this.TabPages[i].Text, this.Font, textRect,
                     isSelected ? selectedTextColor : Color.DimGray,
                     TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
            }

            // 3. Sincronizar el contenedor de la página activa
            if(this.SelectedTab != null && this.SelectedTab.BackColor != contentBackColor) {
                this.SelectedTab.BackColor = contentBackColor;
            }
        }

        private GraphicsPath GetTabPath(Rectangle rect, int radius) {
            GraphicsPath path = new GraphicsPath();
            float s = radius * 2F;

            // Limitadores matemáticos para evitar deformación por radios excesivos
            if(s > rect.Height)
                s = rect.Height;
            if(s > rect.Width)
                s = rect.Width;
            if(s <= 0)
                s = 1;

            path.AddArc(rect.X, rect.Y, s, s, 180, 90);
            path.AddArc(rect.Right - s, rect.Y, s, s, 270, 90);
            path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
            path.CloseFigure();
            return path;
        }
    }
}
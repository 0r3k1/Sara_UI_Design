using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Linq;

namespace Sara_UI_Design.SaraControls {
    public class SaraUI_TabControl:TabControl {
        // Campos
        private Color selectedTabColor = Color.MediumSlateBlue;
        private Color unselectedTabColor = Color.FromArgb(230, 230, 240);
        private Color selectedTextColor = Color.White;
        private bool stretchTabs = false;
        private int hoverIndex = -1;

        // Propiedades
        [Category("Sara UI Design")]
        public Color SelectedTabColor {
            get => selectedTabColor;
            set { selectedTabColor = value; this.Invalidate(); }
        }

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

        public SaraUI_TabControl() {
            this.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.Padding = new Point(20, 10);
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

            // 1. Fondo del área de pestañas
            g.Clear(this.Parent?.BackColor ?? Color.White);

            // 2. Dibujar cada pestaña
            for(int i = 0; i < this.TabCount; i++) {
                Rectangle tabRect = GetTabRect(i);
                bool isSelected = (this.SelectedIndex == i);
                bool isHover = (hoverIndex == i);

                // Fondo de la pestaña
                Color bgColor = isSelected ? selectedTabColor : (isHover ? Color.FromArgb(210, 210, 230) : unselectedTabColor);

                using(GraphicsPath path = GetTabPath(tabRect))
                using(SolidBrush brush = new SolidBrush(bgColor)) {
                    g.FillPath(brush, path);
                }

                // Indicador de selección (Barra inferior blanca/brillante)
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

            // 3. Sincronizar fondo de la página activa
            if(this.SelectedTab != null) {
                this.SelectedTab.BackColor = Color.White; // O el color que desees para el contenido
            }
        }

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
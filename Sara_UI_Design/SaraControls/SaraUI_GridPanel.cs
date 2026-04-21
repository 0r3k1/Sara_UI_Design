using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {
    public class SaraUI_GridPanel:Panel {
        private string columnsConfig = "1fr, 1fr";
        private string rowsConfig = "1fr";
        private int columnGap = 10;
        private int rowGap = 10;
        private int borderRadius = 0;

        [Category("Sara UI Design")]
        public string ColumnsConfig { get => columnsConfig; set { columnsConfig = value; PerformLayout(); } }

        [Category("Sara UI Design")]
        public string RowsConfig { get => rowsConfig; set { rowsConfig = value; PerformLayout(); } }

        [Category("Sara UI Design")]
        public int ColumnGap { get => columnGap; set { columnGap = value; PerformLayout(); } }

        [Category("Sara UI Design")]
        public int RowGap { get => rowGap; set { rowGap = value; PerformLayout(); } }

        [Category("Sara UI Design")]
        public int BorderRadius { get => borderRadius; set { borderRadius = value; Invalidate(); } }

        public SaraUI_GridPanel() {
            this.DoubleBuffered = true;
            this.Padding = new Padding(10);
        }

        protected override void OnLayout(LayoutEventArgs levent) {
            base.OnLayout(levent);
            var visibleControls = Controls.Cast<Control>().Where(c => c.Visible).Reverse().ToList();
            if(!visibleControls.Any())
                return;

            // 1. Calcular anchos de columnas
            var colDefinitions = ParseConfig(columnsConfig, Width - Padding.Left - Padding.Right, columnGap);
            // 2. Calcular altos de filas
            var rowDefinitions = ParseConfig(rowsConfig, Height - Padding.Top - Padding.Bottom, rowGap);

            // 3. Posicionar controles
            int controlIndex = 0;
            foreach(Control ctrl in visibleControls) {
                int r = 0, c = 0;

                // Intentar leer coordenadas del Tag (formato: "fila,columna")
                if(ctrl.Tag != null && ctrl.Tag.ToString().Contains(",")) {
                    var coords = ctrl.Tag.ToString().Split(',');
                    int.TryParse(coords[0], out r);
                    int.TryParse(coords[1], out c);
                } else {
                    // Si no hay Tag, fluir automáticamente
                    r = controlIndex / colDefinitions.Count;
                    c = controlIndex % colDefinitions.Count;
                }

                if(r < rowDefinitions.Count && c < colDefinitions.Count) {
                    float x = Padding.Left + colDefinitions.Take(c).Sum(d => d + columnGap);
                    float y = Padding.Top + rowDefinitions.Take(r).Sum(d => d + rowGap);

                    ctrl.Location = new Point((int)x, (int)y);
                    ctrl.Size = new Size((int)colDefinitions[c], (int)rowDefinitions[r]);
                }
                controlIndex++;
            }
        }

        private List<float> ParseConfig(string config, float availableSize, int gap) {
            string[] parts = config.Split(',');
            int n = parts.Length;
            float totalGap = gap * (n - 1);
            float netSize = availableSize - totalGap;

            float fixedSum = 0;
            float totalFr = 0;
            var definitions = new List<GridUnit>();

            foreach(var part in parts) {
                string p = part.Trim().ToLower();
                if(p.EndsWith("fr")) {
                    float val = float.Parse(p.Replace("fr", ""));
                    totalFr += val;
                    definitions.Add(new GridUnit { Value = val, IsFr = true });
                } else {
                    float val = float.Parse(p);
                    fixedSum += val;
                    definitions.Add(new GridUnit { Value = val, IsFr = false });
                }
            }

            float pxPerFr = totalFr > 0 ? (netSize - fixedSum) / totalFr : 0;
            return definitions.Select(d => d.IsFr ? d.Value * pxPerFr : d.Value).ToList();
        }

        private struct GridUnit { public float Value; public bool IsFr; }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            if(borderRadius <= 0)
                return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using(GraphicsPath path = GetFigurePath(this.ClientRectangle, borderRadius)) {
                this.Region = new Region(path);
            }
        }

        private GraphicsPath GetFigurePath(Rectangle rect, int radius) {
            GraphicsPath path = new GraphicsPath();
            float s = radius * 2F;
            if(s <= 0)
                s = 1;
            path.AddArc(rect.X, rect.Y, s, s, 180, 90);
            path.AddArc(rect.Right - s, rect.Y, s, s, 270, 90);
            path.AddArc(rect.Right - s, rect.Bottom - s, s, s, 0, 90);
            path.AddArc(rect.X, rect.Bottom - s, s, s, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
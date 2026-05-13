using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Contenedor de diseño basado en rejilla (Grid) inspirado en CSS. 
    /// Permite definir filas y columnas usando unidades fraccionales (fr) o píxeles fijos, 
    /// organizando los controles hijos automáticamente.
    /// </summary>
    public class SaraUI_GridPanel:Panel {
        /// <summary>
        /// SUGERENCIA DE USO: Para posicionar un control manualmente en el Grid, 
        /// use la propiedad 'Tag' del control hijo con el formato "fila,columna" (ej. "0,1").
        /// Si no se especifica, el control seguirá el flujo automático.
        /// </summary>
        private string columnsConfig = "1fr, 1fr";
        private string rowsConfig = "1fr";
        private int columnGap = 10;
        private int rowGap = 10;
        private int borderRadius = 0;

        /// <summary>
        /// Obtiene o establece la configuración de columnas. 
        /// Ejemplo: "1fr, 200, 1fr" crea tres columnas donde la del medio es fija y las laterales se reparten el espacio.
        /// </summary>
        [Category("Sara UI Design")]
        public string ColumnsConfig { get => columnsConfig; set { columnsConfig = value; PerformLayout(); } }

        /// <summary>
        /// Obtiene o establece la configuración de filas. 
        /// Ejemplo: "50, 1fr" define una fila superior fija y una inferior que ocupa el resto del panel.
        /// </summary>
        [Category("Sara UI Design")]
        public string RowsConfig { get => rowsConfig; set { rowsConfig = value; PerformLayout(); } }

        /// <summary>
        /// Obtiene o establece el espacio de separación horizontal entre las celdas del grid.
        /// </summary>
        [Category("Sara UI Design")]
        public int ColumnGap { get => columnGap; set { columnGap = value; PerformLayout(); } }

        /// <summary>
        /// Obtiene o establece el espacio de separación vertical entre las celdas del grid.
        /// </summary>
        [Category("Sara UI Design")]
        public int RowGap { get => rowGap; set { rowGap = value; PerformLayout(); } }

        /// <summary>
        /// Obtiene o establece el radio de las esquinas redondeadas del panel. 
        /// </summary>
        [Category("Sara UI Design")]
        public int BorderRadius { get => borderRadius; set { borderRadius = value; Invalidate(); } }

        public SaraUI_GridPanel() {
            this.DoubleBuffered = true;
            this.Padding = new Padding(10);
        }

        /// <summary>
        /// Ejecuta el motor de cálculo del Grid. Calcula dimensiones basándose en las unidades 'fr' 
        /// y posiciona los controles hijos en sus celdas correspondientes.
        /// </summary>
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
                bool hasValidTag = false;

                // Intentar extraer coordenadas: "fila,columna"
                if(ctrl.Tag != null) {
                    string tagValue = ctrl.Tag.ToString();
                    if(tagValue.Contains(",")) {
                        var coords = tagValue.Split(',');
                        // Usamos TryParse y Trim para limpiar espacios extra
                        if(int.TryParse(coords[0].Trim(), out int row) &&
                            int.TryParse(coords[1].Trim(), out int col)) {
                            r = row;
                            c = col;
                            hasValidTag = true;
                        }
                    }
                }

                // Si no tiene Tag válido, aplicamos el flujo automático (Auto-flow)
                if(!hasValidTag) {
                    r = controlIndex / colDefinitions.Count;
                    c = controlIndex % colDefinitions.Count;
                }

                // Posicionamiento final
                if(r < rowDefinitions.Count && c < colDefinitions.Count) {
                    float x = Padding.Left + colDefinitions.Take(c).Sum(d => d + columnGap);
                    float y = Padding.Top + rowDefinitions.Take(r).Sum(d => d + rowGap);

                    ctrl.Location = new Point((int)x, (int)y);
                    ctrl.Size = new Size((int)colDefinitions[c], (int)rowDefinitions[r]);
                }

                // Solo aumentamos el índice si el control NO fue posicionado manualmente
                // Esto permite que el flujo automático rellene los huecos
                if(!hasValidTag)
                    controlIndex++;
            }
        }

        /// <summary>
        /// Analiza las cadenas de configuración (ej. "1fr, 100") y las traduce a valores reales de píxeles 
        /// basándose en el espacio disponible en el panel.
        /// </summary>
        /// <param name="config">Cadena de configuración de filas o columnas.</param>
        /// <param name="availableSize">Tamaño total disponible (ancho o alto).</param>
        /// <param name="gap">Espacio de separación entre elementos.</param>
        /// <returns>Una lista con los tamaños calculados para cada división.</returns>
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

        /// <summary>
        /// Crea el trazado geométrico necesario para recortar el panel con bordes redondeados.
        /// </summary>
        private GraphicsPath GetFigurePath(Rectangle rect, int radius) {
            GraphicsPath path = new GraphicsPath();
            float s = radius * 2F;

            if(s > rect.Width)
                s = rect.Width;
            if(s > rect.Height)
                s = rect.Height;

            if(s <= 0)
                s = 1;
            path.AddArc(rect.X, rect.Y, s, s, 180, 90);
            path.AddArc(rect.Right - s, rect.Y, s, s, 270, 90);
            path.AddArc(rect.Right - s, rect.Bottom - s, s, s, 0, 90);
            path.AddArc(rect.X, rect.Bottom - s, s, s, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Método de utilidad para asignar la posición de un control dentro del Grid.
        /// </summary>
        /// <param name="ctrl">Control hijo al que se le asignará la posición.</param>
        /// <param name="row">Índice de la fila (empezando en 0).</param>
        /// <param name="col">Índice de la columna (empezando en 0).</param>
        public static void SetGridPosition(Control ctrl, int row, int col) {
            if(ctrl == null)
                return;
            ctrl.Tag = $"{row},{col}";
        }
    }
}
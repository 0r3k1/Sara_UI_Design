using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Renderiza barras y menús desplegables sin modificar las propiedades de
    /// los elementos durante el proceso de pintado.
    /// </summary>
    public class SaraUI_MenuRenderer:ToolStripProfessionalRenderer {
        private readonly bool _isMainMenu;
        private readonly Color _primaryColor;
        private readonly Color _textColor;
        private readonly Color _disabledTextColor;
        private readonly Color _selectedTextColor;
        private readonly Color _backColor;
        private readonly int _cornerRadius;
        private readonly int _selectionOpacity;
        private readonly bool _showImageMargin;
        private readonly int _arrowThickness;

        /// <summary>Inicializa el renderizador conservando la firma histórica.</summary>
        /// <param name="isMainMenu">Indica si se usa en una barra principal.</param>
        /// <param name="primaryColor">Color de selección y elementos activos.</param>
        /// <param name="textColor">Color normal o <see cref="Color.Empty"/> para calcularlo.</param>
        /// <param name="backColor">Color de fondo o <see cref="Color.Empty"/> para calcularlo.</param>
        public SaraUI_MenuRenderer(
            bool isMainMenu,
            Color primaryColor,
            Color textColor,
            Color backColor)
            :this(
                isMainMenu,
                primaryColor,
                textColor,
                SystemColors.GrayText,
                backColor,
                6,
                36,
                true) {
        }

        /// <summary>Inicializa un renderizador con todos sus parámetros visuales.</summary>
        /// <param name="isMainMenu">Indica si se usa en una barra principal.</param>
        /// <param name="primaryColor">Color de selección y elementos activos.</param>
        /// <param name="textColor">Color normal o <see cref="Color.Empty"/> para calcularlo.</param>
        /// <param name="disabledTextColor">Color del texto deshabilitado.</param>
        /// <param name="backColor">Color de fondo o <see cref="Color.Empty"/> para calcularlo.</param>
        /// <param name="cornerRadius">Radio de la selección, en píxeles.</param>
        /// <param name="selectionOpacity">Opacidad de la selección entre cero y 255.</param>
        /// <param name="showImageMargin">Indica si se dibuja el margen de imágenes.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce con un radio negativo o una opacidad fuera del intervalo permitido.
        /// </exception>
        public SaraUI_MenuRenderer(
            bool isMainMenu,
            Color primaryColor,
            Color textColor,
            Color disabledTextColor,
            Color backColor,
            int cornerRadius,
            int selectionOpacity,
            bool showImageMargin)
            :base(new SaraUI_MenuColorTable(
                isMainMenu,
                primaryColor,
                backColor,
                selectionOpacity)) {
            if(cornerRadius < 0) {
                throw new ArgumentOutOfRangeException(
                    nameof(cornerRadius),
                    cornerRadius,
                    "El radio no puede ser negativo.");
            }

            _isMainMenu = isMainMenu;
            _primaryColor = primaryColor.IsEmpty
                ? Color.MediumSlateBlue
                : primaryColor;
            _textColor = textColor.IsEmpty
                ? (isMainMenu ? Color.Gainsboro : Color.DimGray)
                : textColor;
            _disabledTextColor = disabledTextColor.IsEmpty
                ? SystemColors.GrayText
                : disabledTextColor;
            _backColor = backColor.IsEmpty
                ? (isMainMenu ? Color.FromArgb(37, 39, 60) : Color.White)
                : backColor;
            _selectedTextColor = GetContrastTextColor(
                Blend(_primaryColor, _backColor, selectionOpacity));
            _cornerRadius = cornerRadius;
            _selectionOpacity = selectionOpacity;
            _showImageMargin = showImageMargin;
            _arrowThickness = isMainMenu ? 3 : 2;
            RoundedEdges = false;
        }

        /// <summary>Obtiene si el renderizador corresponde a un menú principal.</summary>
        public bool IsMainMenu => _isMainMenu;

        /// <summary>Obtiene el color principal utilizado por el renderizador.</summary>
        public Color PrimaryColor => _primaryColor;

        /// <inheritdoc/>
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
            ToolStripItem? item = e.Item;

            if(item?.Enabled == false) {
                e.TextColor = _disabledTextColor;
            } else if(item?.Selected == true || item?.Pressed == true) {
                e.TextColor = _selectedTextColor;
            } else {
                e.TextColor = _textColor;
            }

            System.Drawing.Text.TextRenderingHint previousHint =
                e.Graphics.TextRenderingHint;
            e.Graphics.TextRenderingHint =
                System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            try {
                base.OnRenderItemText(e);
            } finally {
                e.Graphics.TextRenderingHint = previousHint;
            }
        }

        /// <inheritdoc/>
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
            ToolStripItem? item = e.Item;

            if(item == null || (!item.Selected && !item.Pressed)) {
                return;
            }

            Rectangle bounds = new Rectangle(
                2,
                1,
                Math.Max(0, item.Width - 4),
                Math.Max(0, item.Height - 2));

            if(bounds.Width < 1 || bounds.Height < 1) {
                return;
            }

            Color fillColor = item.Enabled
                ? Color.FromArgb(
                    Math.Min(255, _selectionOpacity + (item.Pressed ? 28 : 0)),
                    _primaryColor)
                : Color.FromArgb(_selectionOpacity, _disabledTextColor);

            SmoothingMode previousSmoothing = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using GraphicsPath path = CreateRoundedPath(bounds, _cornerRadius);
            using SolidBrush brush = new SolidBrush(fillColor);
            e.Graphics.FillPath(brush, path);

            if(item.Pressed && item.Enabled) {
                using Pen borderPen = new Pen(
                    Color.FromArgb(Math.Min(255, _selectionOpacity + 80), _primaryColor),
                    1f);
                e.Graphics.DrawPath(borderPen, path);
            }

            e.Graphics.SmoothingMode = previousSmoothing;
        }

        /// <inheritdoc/>
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) {
            Rectangle bounds = e.ArrowRectangle;

            if(bounds.Width < 3 || bounds.Height < 3) {
                return;
            }

            ToolStripItem? item = e.Item;
            Color arrowColor = item?.Enabled == false
                ? _disabledTextColor
                : (item?.Selected == true ? _selectedTextColor : _textColor);
            Point[] points = CreateChevron(bounds, e.Direction);
            SmoothingMode previousSmoothing = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using Pen pen = new Pen(arrowColor, _arrowThickness) {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            e.Graphics.DrawLines(pen, points);
            e.Graphics.SmoothingMode = previousSmoothing;
        }

        /// <inheritdoc/>
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e) {
            Rectangle bounds = e.Item.ContentRectangle;
            Color color = Color.FromArgb(90, _textColor);

            using Pen pen = new Pen(color, 1f);

            if(e.Vertical) {
                int x = bounds.Left + (bounds.Width / 2);
                e.Graphics.DrawLine(pen, x, bounds.Top + 3, x, bounds.Bottom - 3);
            } else {
                int y = bounds.Top + (bounds.Height / 2);
                e.Graphics.DrawLine(pen, bounds.Left + 6, y, bounds.Right - 6, y);
            }
        }

        /// <inheritdoc/>
        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) {
            if(_showImageMargin) {
                base.OnRenderImageMargin(e);
            }
        }

        /// <inheritdoc/>
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e) {
            using SolidBrush brush = new SolidBrush(_backColor);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        /// <inheritdoc/>
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) {
            if(!_isMainMenu) {
                base.OnRenderToolStripBorder(e);
            }
        }

        private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius) {
            GraphicsPath path = new GraphicsPath();

            if(bounds.Width < 1 || bounds.Height < 1) {
                return path;
            }

            int safeRadius = Math.Max(
                0,
                Math.Min(radius, Math.Min(bounds.Width, bounds.Height) / 2));

            if(safeRadius == 0) {
                path.AddRectangle(bounds);
                return path;
            }

            int diameter = safeRadius * 2;
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(
                bounds.Right - diameter,
                bounds.Bottom - diameter,
                diameter,
                diameter,
                0,
                90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static Point[] CreateChevron(Rectangle bounds, ArrowDirection direction) {
            int centerX = bounds.Left + (bounds.Width / 2);
            int centerY = bounds.Top + (bounds.Height / 2);
            int horizontal = Math.Max(2, Math.Min(4, bounds.Width / 3));
            int vertical = Math.Max(2, Math.Min(5, bounds.Height / 3));

            switch(direction) {
                case ArrowDirection.Left:
                    return new[] {
                        new Point(centerX + horizontal / 2, centerY - vertical),
                        new Point(centerX - horizontal / 2, centerY),
                        new Point(centerX + horizontal / 2, centerY + vertical)
                    };
                case ArrowDirection.Up:
                    return new[] {
                        new Point(centerX - horizontal, centerY + vertical / 2),
                        new Point(centerX, centerY - vertical / 2),
                        new Point(centerX + horizontal, centerY + vertical / 2)
                    };
                case ArrowDirection.Down:
                    return new[] {
                        new Point(centerX - horizontal, centerY - vertical / 2),
                        new Point(centerX, centerY + vertical / 2),
                        new Point(centerX + horizontal, centerY - vertical / 2)
                    };
                default:
                    return new[] {
                        new Point(centerX - horizontal / 2, centerY - vertical),
                        new Point(centerX + horizontal / 2, centerY),
                        new Point(centerX - horizontal / 2, centerY + vertical)
                    };
            }
        }

        private static Color Blend(Color foreground, Color background, int opacity) {
            float ratio = opacity / 255f;
            float inverse = 1f - ratio;

            return Color.FromArgb(
                255,
                ClampByte((int)Math.Round((foreground.R * ratio) + (background.R * inverse))),
                ClampByte((int)Math.Round((foreground.G * ratio) + (background.G * inverse))),
                ClampByte((int)Math.Round((foreground.B * ratio) + (background.B * inverse))));
        }

        private static Color GetContrastTextColor(Color background) {
            double luminance =
                (0.2126 * background.R) +
                (0.7152 * background.G) +
                (0.0722 * background.B);
            return luminance >= 145d ? Color.Black : Color.White;
        }

        private static int ClampByte(int value) {
            return Math.Max(0, Math.Min(255, value));
        }
    }
}

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Panel flexible que dibuja una superficie redondeada con sombra difusa,
    /// borde opcional y espacio de contenido independiente.
    /// </summary>
    [ToolboxItem(true)]
    public class SaraUI_ShadowPanel:SaraUI_FlexPanel {
        private const int MaximumShadowSize = 1024;
        private const int MaximumShadowOffset = 32768;
        private const int MaximumContentPadding = 1000000;
        private int _shadowSize = 10;
        private Color _shadowColor = Color.FromArgb(64, 64, 64);
        private int _shadowOpacity = 100;
        private int _shadowOffsetX;
        private int _shadowOffsetY = 5;
        private float _shadowFocusScale = 0.75f;
        private Color _borderColor = Color.Transparent;
        private int _borderThickness;
        private Padding _contentPadding = System.Windows.Forms.Padding.Empty;
        private bool _updatingEffectivePadding;
        private bool _disposingResources;
        private bool _shadowCacheValid;
        private Bitmap? _shadowBitmap;
        private Size _cachedClientSize = Size.Empty;
        private int _cachedBorderRadius = -1;

        /// <summary>Inicializa un panel con superficie blanca y espacio reservado para la sombra.</summary>
        public SaraUI_ShadowPanel() {
            BackColor = Color.White;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.UserPaint,
                true);
            UpdateEffectivePadding();
        }

        /// <summary>Obtiene o establece la extensión de la sombra alrededor de la superficie.</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce al asignar un valor menor que cero o mayor que 1024.
        /// </exception>
        [Category("Sara UI Design")]
        [DefaultValue(10)]
        public int ShadowSize {
            get => _shadowSize;
            set {
                EnsureRange(value, 0, MaximumShadowSize, nameof(ShadowSize));

                if(_shadowSize == value) {
                    return;
                }

                _shadowSize = value;
                UpdateEffectivePadding();
                InvalidateShadowCache();
            }
        }

        /// <summary>Obtiene o establece el color base de la sombra.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "64, 64, 64")]
        public Color ShadowColor {
            get => _shadowColor;
            set {
                if(_shadowColor == value) {
                    return;
                }

                _shadowColor = value;
                InvalidateShadowCache();
            }
        }

        /// <summary>Obtiene o establece la opacidad adicional de la sombra, entre 0 y 255.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor fuera del rango.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(100)]
        public int ShadowOpacity {
            get => _shadowOpacity;
            set {
                EnsureRange(value, 0, 255, nameof(ShadowOpacity));

                if(_shadowOpacity == value) {
                    return;
                }

                _shadowOpacity = value;
                InvalidateShadowCache();
            }
        }

        /// <summary>Obtiene o establece el desplazamiento horizontal de la sombra.</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce al asignar un valor fuera del rango de −32768 a 32768.
        /// </exception>
        [Category("Sara UI Design")]
        [DefaultValue(0)]
        public int ShadowOffsetX {
            get => _shadowOffsetX;
            set => SetShadowOffset(value, _shadowOffsetY);
        }

        /// <summary>Obtiene o establece el desplazamiento vertical de la sombra.</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce al asignar un valor fuera del rango de −32768 a 32768.
        /// </exception>
        [Category("Sara UI Design")]
        [DefaultValue(5)]
        public int ShadowOffsetY {
            get => _shadowOffsetY;
            set => SetShadowOffset(_shadowOffsetX, value);
        }

        /// <summary>
        /// Obtiene o establece la proporción central del degradado. Los valores menores
        /// producen una transición más extensa hacia el borde.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al asignar un valor fuera de (0, 1].</exception>
        [Category("Sara UI Design")]
        [DefaultValue(0.75f)]
        public float ShadowFocusScale {
            get => _shadowFocusScale;
            set {
                if(float.IsNaN(value) || float.IsInfinity(value) || value <= 0f || value > 1f) {
                    throw new ArgumentOutOfRangeException(
                        nameof(ShadowFocusScale),
                        value,
                        "La escala debe ser mayor que cero y menor o igual que uno.");
                }

                if(Math.Abs(_shadowFocusScale - value) < 0.0001f) {
                    return;
                }

                _shadowFocusScale = value;
                InvalidateShadowCache();
            }
        }

        /// <summary>Obtiene o establece el color del borde de la superficie.</summary>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Color), "Transparent")]
        public Color BorderColor {
            get => _borderColor;
            set {
                if(_borderColor == value) {
                    return;
                }

                _borderColor = value;
                Invalidate();
            }
        }

        /// <summary>Obtiene o establece el grosor interior del borde.</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce al asignar un valor menor que cero o mayor que 1024.
        /// </exception>
        [Category("Sara UI Design")]
        [DefaultValue(0)]
        public int BorderThickness {
            get => _borderThickness;
            set {
                EnsureRange(value, 0, MaximumShadowSize, nameof(BorderThickness));

                if(_borderThickness == value) {
                    return;
                }

                _borderThickness = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el espacio entre la superficie visible y los controles hijos.
        /// La reserva necesaria para la sombra se administra internamente.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce si alguno de sus lados es negativo.</exception>
        [Category("Sara UI Design")]
        [DefaultValue(typeof(Padding), "0, 0, 0, 0")]
        public new Padding Padding {
            get => _contentPadding;
            set {
                EnsureContentPadding(value);

                if(_contentPadding == value) {
                    return;
                }

                _contentPadding = value;
                UpdateEffectivePadding();
            }
        }

        /// <summary>Obtiene la reserva calculada para la sombra en cada lado.</summary>
        [Browsable(false)]
        public Padding ShadowInsets => CalculateShadowInsets();

        /// <summary>Obtiene los límites actuales de la superficie visible.</summary>
        [Browsable(false)]
        public Rectangle SurfaceBounds => CalculateSurfaceBounds();

        /// <summary>Actualiza ambos desplazamientos de la sombra en una sola operación de diseño.</summary>
        /// <param name="horizontal">Desplazamiento horizontal entre −32768 y 32768.</param>
        /// <param name="vertical">Desplazamiento vertical entre −32768 y 32768.</param>
        /// <exception cref="ArgumentOutOfRangeException">Se produce si algún valor está fuera del rango.</exception>
        public void SetShadowOffset(int horizontal, int vertical) {
            EnsureRange(
                horizontal,
                -MaximumShadowOffset,
                MaximumShadowOffset,
                nameof(horizontal));
            EnsureRange(
                vertical,
                -MaximumShadowOffset,
                MaximumShadowOffset,
                nameof(vertical));

            if(_shadowOffsetX == horizontal && _shadowOffsetY == vertical) {
                return;
            }

            _shadowOffsetX = horizontal;
            _shadowOffsetY = vertical;
            UpdateEffectivePadding();
            InvalidateShadowCache();
        }

        /// <inheritdoc/>
        [Browsable(false)]
        protected override bool ApplyRoundedControlRegion => false;

        /// <inheritdoc/>
        protected override void OnPaddingChanged(EventArgs e) {
            base.OnPaddingChanged(e);

            if(_updatingEffectivePadding || _disposingResources || IsDisposed) {
                return;
            }

            Padding requestedPadding = base.Padding;
            EnsureContentPadding(requestedPadding);
            _contentPadding = requestedPadding;
            UpdateEffectivePadding();
        }

        /// <inheritdoc/>
        protected override void OnSizeChanged(EventArgs e) {
            base.OnSizeChanged(e);
            InvalidateShadowCache();
        }

        /// <inheritdoc/>
        protected override void OnPaintBackground(PaintEventArgs e) {
            Color outsideColor = Parent?.BackColor ?? SystemColors.Control;
            using SolidBrush outsideBrush = new SolidBrush(outsideColor);
            e.Graphics.FillRectangle(outsideBrush, ClientRectangle);
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e) {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            EnsureShadowBitmap();

            if(_shadowBitmap != null) {
                graphics.DrawImageUnscaled(_shadowBitmap, Point.Empty);
            }

            DrawSurface(graphics);
            base.OnPaint(e);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            if(disposing && !_disposingResources) {
                _disposingResources = true;
                _shadowBitmap?.Dispose();
                _shadowBitmap = null;
                _shadowCacheValid = false;
            }

            base.Dispose(disposing);
        }

        private void UpdateEffectivePadding() {
            if(_disposingResources || IsDisposed) {
                return;
            }

            Padding insets = CalculateShadowInsets();
            Padding effectivePadding = new Padding(
                SafeAdd(insets.Left, _contentPadding.Left),
                SafeAdd(insets.Top, _contentPadding.Top),
                SafeAdd(insets.Right, _contentPadding.Right),
                SafeAdd(insets.Bottom, _contentPadding.Bottom));

            if(base.Padding == effectivePadding) {
                return;
            }

            _updatingEffectivePadding = true;

            try {
                base.Padding = effectivePadding;
            } finally {
                _updatingEffectivePadding = false;
            }

            Invalidate();
        }

        private Padding CalculateShadowInsets() {
            if(_shadowSize == 0) {
                return System.Windows.Forms.Padding.Empty;
            }

            int left = SafeAdd(_shadowSize, Math.Max(0, -_shadowOffsetX));
            int top = SafeAdd(_shadowSize, Math.Max(0, -_shadowOffsetY));
            int right = SafeAdd(_shadowSize, Math.Max(0, _shadowOffsetX));
            int bottom = SafeAdd(_shadowSize, Math.Max(0, _shadowOffsetY));
            return new Padding(left, top, right, bottom);
        }

        private Rectangle CalculateSurfaceBounds() {
            Padding insets = CalculateShadowInsets();
            int left = Math.Min(ClientSize.Width, insets.Left);
            int top = Math.Min(ClientSize.Height, insets.Top);
            int right = Math.Max(left, ClientSize.Width - insets.Right);
            int bottom = Math.Max(top, ClientSize.Height - insets.Bottom);
            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        private void EnsureShadowBitmap() {
            if(_shadowCacheValid &&
                _cachedClientSize == ClientSize &&
                _cachedBorderRadius == BorderRadius) {
                return;
            }

            _shadowBitmap?.Dispose();
            _shadowBitmap = null;
            _cachedClientSize = ClientSize;
            _cachedBorderRadius = BorderRadius;
            _shadowCacheValid = true;

            Rectangle surfaceBounds = CalculateSurfaceBounds();

            if(ClientSize.Width <= 0 || ClientSize.Height <= 0 ||
                surfaceBounds.Width <= 0 || surfaceBounds.Height <= 0 ||
                _shadowSize == 0 || _shadowOpacity == 0 || _shadowColor.A == 0) {
                return;
            }

            Bitmap shadowBitmap = new Bitmap(ClientSize.Width, ClientSize.Height);

            using(Graphics shadowGraphics = Graphics.FromImage(shadowBitmap)) {
                shadowGraphics.Clear(Color.Transparent);
                shadowGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                DrawShadow(shadowGraphics, surfaceBounds);
            }

            _shadowBitmap = shadowBitmap;
        }

        private void DrawShadow(Graphics graphics, Rectangle surfaceBounds) {
            Rectangle shadowBounds = surfaceBounds;
            shadowBounds.Offset(_shadowOffsetX, _shadowOffsetY);
            shadowBounds.Inflate(_shadowSize, _shadowSize);

            if(shadowBounds.Width <= 0 || shadowBounds.Height <= 0) {
                return;
            }

            int shadowRadius = SafeAdd(BorderRadius, _shadowSize);
            int effectiveAlpha = (int)Math.Round(
                _shadowOpacity * (_shadowColor.A / 255d));
            Color centerColor = Color.FromArgb(effectiveAlpha, _shadowColor);
            Color edgeColor = Color.FromArgb(0, _shadowColor);

            using GraphicsPath shadowPath = GetFigurePath(shadowBounds, shadowRadius);
            using PathGradientBrush shadowBrush = new PathGradientBrush(shadowPath) {
                CenterColor = centerColor,
                FocusScales = new PointF(_shadowFocusScale, _shadowFocusScale),
                SurroundColors = new[] { edgeColor }
            };
            graphics.FillPath(shadowBrush, shadowPath);
        }

        private void DrawSurface(Graphics graphics) {
            Rectangle surfaceBounds = CalculateSurfaceBounds();

            if(surfaceBounds.Width <= 0 || surfaceBounds.Height <= 0) {
                return;
            }

            using GraphicsPath surfacePath = GetFigurePath(surfaceBounds, BorderRadius);
            using SolidBrush surfaceBrush = new SolidBrush(BackColor);
            graphics.FillPath(surfaceBrush, surfacePath);

            if(_borderThickness <= 0 || _borderColor.A == 0) {
                return;
            }

            float thickness = Math.Min(
                _borderThickness,
                Math.Max(1, Math.Min(surfaceBounds.Width, surfaceBounds.Height)));
            using Pen borderPen = new Pen(_borderColor, thickness) {
                Alignment = PenAlignment.Inset
            };
            graphics.DrawPath(borderPen, surfacePath);
        }

        private void InvalidateShadowCache() {
            _shadowBitmap?.Dispose();
            _shadowBitmap = null;
            _shadowCacheValid = false;
            Invalidate();
        }

        private static int SafeAdd(int first, int second) {
            long result = (long)first + second;
            return result >= int.MaxValue ? int.MaxValue : (int)result;
        }

        private static void EnsureRange(int value, int minimum, int maximum, string propertyName) {
            if(value < minimum || value > maximum) {
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    value,
                    $"El valor debe encontrarse entre {minimum} y {maximum}.");
            }
        }

        private static void EnsureContentPadding(Padding value) {
            if(value.Left < 0 || value.Top < 0 || value.Right < 0 || value.Bottom < 0 ||
                value.Left > MaximumContentPadding || value.Top > MaximumContentPadding ||
                value.Right > MaximumContentPadding || value.Bottom > MaximumContentPadding) {
                throw new ArgumentOutOfRangeException(
                    nameof(Padding),
                    value,
                    "Cada lado del espacio interno debe encontrarse entre 0 y 1000000.");
            }
        }
    }
}

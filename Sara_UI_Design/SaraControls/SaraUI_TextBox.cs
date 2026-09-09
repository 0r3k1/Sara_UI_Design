using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Control de entrada de texto avanzado de la suite Sara UI. 
    /// Soporta estilos subrayados o redondeados, placeholders, validación de tipos de entrada, 
    /// iconos integrados de la IconLibrary y efectos visuales de enfoque.
    /// </summary>
    [DefaultEvent("_TextChanged")]
    public partial class SaraUI_TextBox:UserControl {

        /// <summary>
        /// Define el comportamiento del cuadro de texto según el tipo de datos esperado.
        /// </summary>
        public enum InputType {
            /// <summary>Permite introducir texto sin restricciones adicionales.</summary>
            Text,

            /// <summary>Oculta el contenido utilizando el carácter de contraseña del sistema.</summary>
            Password,

            /// <summary>Acepta únicamente caracteres numéricos.</summary>
            Numeric,

            /// <summary>Permite introducir texto distribuido en varias líneas.</summary>
            Multiline
        }

        /// <summary>
        /// Define la ubicación del icono dentro del control.
        /// </summary>
        public enum SaraIconLocation {
            /// <summary>Coloca el icono a la izquierda del contenido.</summary>
            Left,

            /// <summary>Coloca el icono a la derecha del contenido.</summary>
            Right
        }

        // Fields de diseño
        private Color borderColor = Color.MediumSlateBlue;
        private int borderSize = 2;
        private bool underlinedStyle = false;
        private Color borderFocusColor = Color.HotPink;
        private bool isFocused = false;
        private int borderRadius = 0;

        // Fields de Placeholder
        private string placeholderText = "";
        private bool isPlaceholder = false;
        private Color placeholderColor = Color.Gray;
        private Color mainForeColor = Color.Black;
        private InputType inputType = InputType.Text;
        private bool isUpdatingEditor = false;

        // Fields de Icono
        private string iconName = "None";
        private Color iconColor = Color.Gray;
        private int iconSize = 20;
        private SaraIconLocation iconLocation = SaraIconLocation.Left;

        /// <summary>
        /// Ocurre cuando el contenido de texto dentro del control cambia.
        /// Se conserva para mantener compatibilidad; el evento estándar <see cref="Control.TextChanged"/>
        /// también se genera con el mismo cambio lógico.
        /// </summary>
        public event EventHandler? _TextChanged;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_TextBox"/> configurando 
        /// los eventos del control interno, la sincronización de estilos y el espaciado dinámico.
        /// </summary>
        public SaraUI_TextBox() {
            InitializeComponent();
            mainForeColor = ForeColor;
            ConfigureTextBoxEvents();

            ApplyInputType();
            UpdatePadding();
            UpdateEditorPresentation();
            UpdateControlRegion();
        }

        private void ConfigureTextBoxEvents() {
            textBox1.TextChanged += TextBox1_TextChanged;
            textBox1.Enter += TextBox1_Enter;
            textBox1.Leave += TextBox1_Leave;
            textBox1.KeyPress += textBox1_KeyPress;
            textBox1.Click += TextBox1_Click;
        }

        /// <summary>
        /// Obtiene o establece el texto actual del control. Si el placeholder está activo, devuelve un valor vacío.
        /// </summary>
        [Category("Sara UI Design")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text {
            get => base.Text ?? string.Empty;
            set => SetLogicalText(value ?? string.Empty, true);
        }

        /// <summary>
        /// Obtiene o establece el color de fondo del control y sincroniza el cuadro de texto interno.
        /// </summary>
        [Category("Sara UI Design")]
        public override Color BackColor {
            get => base.BackColor;
            set => base.BackColor = value;
        }

        /// <inheritdoc/>
        protected override void OnBackColorChanged(EventArgs e) {
            base.OnBackColorChanged(e);
            if(textBox1 != null)
                textBox1.BackColor = this.BackColor;
            this.Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnForeColorChanged(EventArgs e) {
            base.OnForeColorChanged(e);
            mainForeColor = this.ForeColor;
            if(!isPlaceholder && textBox1 != null)
                textBox1.ForeColor = this.ForeColor;
            this.Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnFontChanged(EventArgs e) {
            base.OnFontChanged(e);
            if(textBox1 == null)
                return;

            textBox1.Font = this.Font;
            UpdateControlHeight();
            this.Invalidate();
        }

        // ==========================================
        // PROPIEDADES SARA UI DESIGN
        // ==========================================

        /// <summary>
        /// Obtiene o establece el nombre del icono a mostrar desde la biblioteca compartida. Use "None" para ocultarlo.
        /// </summary>
        [Category("Sara UI Design")]
        [TypeConverter(typeof(IconNameConverter))]
        public string IconName {
            get => iconName;
            set {
                iconName = string.IsNullOrWhiteSpace(value) ? "None" : value;
                UpdatePadding();
                this.Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el color base del icono gráfico.
        /// </summary>
        [Category("Sara UI Design")]
        public Color IconColor { get => iconColor; set { iconColor = value; this.Invalidate(); } }

        /// <summary>
        /// Obtiene o establece el tamaño en píxeles del icono (ancho y alto proporcional).
        /// </summary>
        [Category("Sara UI Design")]
        public int IconSize {
            get => iconSize;
            set {
                iconSize = Math.Max(1, value);
                UpdatePadding();
                this.Invalidate();
            }
        }

        /// <summary>
        /// Define si el icono se posiciona al inicio (izquierda) o al final (derecha) del texto.
        /// </summary>
        [Category("Sara UI Design")]
        public SaraIconLocation IconLocation { get => iconLocation; set { iconLocation = value; UpdatePadding(); this.Invalidate(); } }

        /// <summary>
        /// Obtiene o establece el grosor del borde perimetral del control.
        /// </summary>
        [Category("Sara UI Design")]
        public int BorderSize {
            get => borderSize;
            set {
                borderSize = Math.Max(0, value);
                UpdatePadding();
                this.Invalidate();
            }
        }

        /// <summary>
        /// Define el tipo de entrada: Texto, Contraseña (máscara), Numérico (solo dígitos) o Multilínea.
        /// </summary>
        [Category("Sara UI Design")]
        public InputType Type {
            get => inputType;
            set {
                if(inputType == value)
                    return;

                inputType = value;
                ApplyInputType();
                this.Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el color del texto indicativo o placeholder.
        /// </summary>
        [Category("Sara UI Design")]
        public Color PlaceholderColor {
            get => placeholderColor;
            set {
                placeholderColor = value;
                if(isPlaceholder)
                    textBox1.ForeColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el texto de ayuda que desaparece cuando el usuario comienza a escribir o enfoca el control.
        /// </summary>
        [Category("Sara UI Design")]
        public string PlaceholderText {
            get => placeholderText;
            set {
                placeholderText = value ?? string.Empty;
                UpdateEditorPresentation();
                this.Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el radio de curvatura para los bordes redondeados del control.
        /// </summary>
        [Category("Sara UI Design")]
        public int BorderRadius {
            get => borderRadius;
            set {
                borderRadius = Math.Max(0, value);
                UpdateControlRegion();
                this.Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el color base del borde cuando el control está en estado pasivo o en reposo.
        /// </summary>
        [Category("Sara UI Design")]
        public Color BorderColor {
            get => borderColor;
            set { borderColor = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el color del borde cuando el control gana el foco de entrada del teclado.
        /// </summary>
        [Category("Sara UI Design")]
        public Color BorderFocusColor {
            get => borderFocusColor;
            set { borderFocusColor = value; this.Invalidate(); }
        }

        /// <summary>
        /// Si es verdadero, el control muestra únicamente una línea inferior en lugar de un borde completo perimetral.
        /// </summary>
        [Category("Sara UI Design")]
        public bool UnderlinedStyle {
            get => underlinedStyle;
            set {
                underlinedStyle = value;
                UpdateControlRegion();
                this.Invalidate();
            }
        }

        // ==========================================
        // LÓGICA DE PROCESAMIENTO Y VALIDACIONES
        // ==========================================

        private void ApplyInputType() {
            switch(inputType) {
                case InputType.Multiline:
                textBox1.Multiline = true;
                break;

                case InputType.Password:
                textBox1.Multiline = false;
                UpdateControlHeight();
                break;

                case InputType.Numeric:
                SetLogicalText(SanitizeNumericText(base.Text ?? string.Empty), true);
                textBox1.Multiline = false;
                UpdateControlHeight();
                break;

                case InputType.Text:
                textBox1.Multiline = false;
                UpdateControlHeight();
                break;
            }

            UpdateEditorPresentation();
        }

        private void TextBox1_TextChanged(object? sender, EventArgs e) {
            if(isUpdatingEditor || isPlaceholder)
                return;

            string editorText = textBox1.Text;
            if(inputType == InputType.Numeric) {
                int caretPosition = CountDigitsBefore(textBox1.Text, textBox1.SelectionStart);
                string sanitizedText = SanitizeNumericText(editorText);

                if(editorText != sanitizedText) {
                    SetEditorText(sanitizedText);
                    textBox1.SelectionStart = Math.Min(caretPosition, sanitizedText.Length);
                }

                editorText = sanitizedText;
            }

            SetLogicalText(editorText, false);
        }

        private void TextBox1_Enter(object? sender, EventArgs e) {
            isFocused = true;
            UpdateEditorPresentation();
            this.Invalidate();
        }

        private void TextBox1_Leave(object? sender, EventArgs e) {
            isFocused = false;
            UpdateEditorPresentation();
            this.Invalidate();
        }

        private void TextBox1_Click(object? sender, EventArgs e) {
            this.OnClick(e);
        }

        private void textBox1_KeyPress(object? sender, KeyPressEventArgs e) {
            if(inputType == InputType.Numeric && !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) {
                e.Handled = true;
            }
            this.OnKeyPress(e);
        }

        private void SetLogicalText(string value, bool updateEditor) {
            string normalizedText = inputType == InputType.Numeric
                ? SanitizeNumericText(value)
                : value;

            bool hasChanged = base.Text != normalizedText;

            if(updateEditor && textBox1 != null)
                UpdateEditorPresentation(normalizedText);

            if(!hasChanged)
                return;

            base.Text = normalizedText;
            _TextChanged?.Invoke(this, EventArgs.Empty);
            this.Invalidate();
        }

        private void UpdateEditorPresentation() {
            UpdateEditorPresentation(base.Text ?? string.Empty);
        }

        private void UpdateEditorPresentation(string currentText) {
            if(textBox1 == null)
                return;

            bool shouldShowPlaceholder = !isFocused
                && currentText.Length == 0
                && !string.IsNullOrEmpty(placeholderText);

            isPlaceholder = shouldShowPlaceholder;
            textBox1.ForeColor = shouldShowPlaceholder ? placeholderColor : mainForeColor;
            textBox1.UseSystemPasswordChar = inputType == InputType.Password && !shouldShowPlaceholder;
            SetEditorText(shouldShowPlaceholder ? placeholderText : currentText);
        }

        private void SetEditorText(string value) {
            if(textBox1.Text == value)
                return;

            isUpdatingEditor = true;
            try {
                textBox1.Text = value;
            } finally {
                isUpdatingEditor = false;
            }
        }

        private static string SanitizeNumericText(string value) {
            if(string.IsNullOrEmpty(value))
                return string.Empty;

            char[] digits = new char[value.Length];
            int digitCount = 0;

            foreach(char character in value) {
                if(char.IsDigit(character)) {
                    digits[digitCount] = character;
                    digitCount++;
                }
            }

            return new string(digits, 0, digitCount);
        }

        private static int CountDigitsBefore(string value, int position) {
            int digitCount = 0;
            int limit = Math.Min(position, value.Length);

            for(int index = 0; index < limit; index++) {
                if(char.IsDigit(value[index]))
                    digitCount++;
            }

            return digitCount;
        }

        /// <summary>
        /// Calcula el margen interno óptimo del control en tiempo real sumando los grosores de bordes y gráficos activos.
        /// </summary>
        private void UpdatePadding() {
            int left = borderSize + 6;
            int right = borderSize + 6;
            int top = borderSize + 5;
            int bottom = borderSize + 5;

            bool hasIcon = !string.IsNullOrEmpty(iconName) && iconName != "None";

            if(hasIcon) {
                if(iconLocation == SaraIconLocation.Left)
                    left += iconSize + 8;
                else
                    right += iconSize + 8;
            }

            this.Padding = new Padding(left, top, right, bottom);
            UpdateControlHeight();
        }

        /// <summary>
        /// Gestiona el dibujo del borde, la región redondeada y la renderización del icono 
        /// con efectos de color reactivos cuando el control tiene el foco.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            if(this.Width <= 2 || this.Height <= 2)
                return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rectBorder = Rectangle.Inflate(this.ClientRectangle, -1, -1);
            Color currentBorder = isFocused ? borderFocusColor : borderColor;

            // 1. Dibujar el Borde (Redondeado o Subrayado)
            if(borderSize > 0) {
                using Pen penBorder = new Pen(currentBorder, borderSize);
                penBorder.Alignment = PenAlignment.Inset;

                if(borderRadius > 1 && !underlinedStyle) {
                    using(GraphicsPath pathBorder = GetFigurePath(rectBorder, borderRadius)) {
                        g.DrawPath(penBorder, pathBorder);
                    }
                } else {
                    if(underlinedStyle)
                        g.DrawLine(penBorder, 0, this.Height - 1, this.Width, this.Height - 1);
                    else
                        g.DrawRectangle(penBorder, rectBorder);
                }
            }

            // 2. Dibujar el Icono desde la IconLibrary de forma dinámica
            if(!string.IsNullOrEmpty(iconName) && iconName != "None") {
                int availableIconSize = Math.Max(1, Math.Min(this.Width - 4, this.Height - 4));
                int renderedIconSize = Math.Min(iconSize, availableIconSize);
                int iconX = iconLocation == SaraIconLocation.Left
                    ? 12
                    : this.Width - renderedIconSize - 12;
                int iconY = (this.Height - renderedIconSize) / 2;
                Rectangle iconRect = new Rectangle(iconX, iconY, renderedIconSize, renderedIconSize);

                Color currentIconColor = isFocused ? borderFocusColor : iconColor;
                SaraUI_IconLibrary.DrawIcon(iconName, g, iconRect, currentIconColor);
            }
        }

        private GraphicsPath GetFigurePath(Rectangle rect, int radius) {
            GraphicsPath path = new GraphicsPath();
            if(rect.Width <= 0 || rect.Height <= 0) {
                return path;
            }

            float curveSize = Math.Min(radius * 2F, Math.Min(rect.Width, rect.Height));
            if(curveSize <= 1F) {
                path.AddRectangle(rect);
                return path;
            }

            path.StartFigure();
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
            path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
            path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void UpdateControlRegion() {
            if(this.Width <= 0 || this.Height <= 0)
                return;

            Region newRegion;
            if(borderRadius > 1 && !underlinedStyle) {
                using GraphicsPath path = GetFigurePath(this.ClientRectangle, borderRadius);
                newRegion = new Region(path);
            } else {
                newRegion = new Region(this.ClientRectangle);
            }

            Region? previousRegion = this.Region;
            this.Region = newRegion;
            previousRegion?.Dispose();
        }

        /// <summary>
        /// Ajusta automáticamente la altura del control basándose en la fuente seleccionada y el padding, 
        /// siempre que no se encuentre en modo multilínea.
        /// </summary>
        private void UpdateControlHeight() {
            if(textBox1.Multiline == false) {
                int txtHeight = TextRenderer.MeasureText("Text", this.Font).Height + 1;
                textBox1.MinimumSize = new Size(0, txtHeight);
                this.Height = textBox1.Height + this.Padding.Top + this.Padding.Bottom;
            }
        }

        /// <inheritdoc/>
        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            UpdateControlRegion();
            if(textBox1 != null && this.DesignMode && inputType != InputType.Multiline) {
                UpdateControlHeight();
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            textBox1.Focus();
        }

        /// <inheritdoc/>
        protected override void OnClick(EventArgs e) {
            if(!textBox1.Focused)
                textBox1.Focus();

            base.OnClick(e);
        }
    }
}

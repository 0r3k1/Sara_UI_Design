using System;
using System.Collections.Generic;
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
        public enum InputType { Text, Password, Numeric, Multiline }

        /// <summary>
        /// Define la ubicación del icono dentro del control.
        /// </summary>
        public enum SaraIconLocation { Left, Right }

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

        // Fields de Icono
        private string iconName = "None";
        private Color iconColor = Color.Gray;
        private int iconSize = 20;
        private SaraIconLocation iconLocation = SaraIconLocation.Left;

        /// <summary>
        /// Ocurre cuando el contenido de texto dentro del control cambia.
        /// </summary>
        public event EventHandler? _TextChanged;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_TextBox"/> configurando 
        /// los eventos del control interno, la sincronización de estilos y el espaciado dinámico.
        /// </summary>
        public SaraUI_TextBox() {
            InitializeComponent();
            this.mainForeColor = this.ForeColor;
            ConfigureTextBoxEvents();

            // Forzar configuraciones e hilos iniciales críticos
            ApplyInputType();
            UpdatePadding();
        }

        private void ConfigureTextBoxEvents() {
            textBox1.TextChanged += (s, e) => _TextChanged?.Invoke(this, e);
            textBox1.Enter += (s, e) => { isFocused = true; RemovePlaceholder(); this.Invalidate(); };
            textBox1.Leave += (s, e) => { isFocused = false; SetPlaceholder(); this.Invalidate(); };
            textBox1.KeyPress += textBox1_KeyPress;
            textBox1.Click += (s, e) => this.OnClick(e);
        }

        /// <summary>
        /// Obtiene o establece el texto actual del control. Si el placeholder está activo, devuelve un valor vacío.
        /// </summary>
        [Category("Sara UI Design")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text {
            get {
                return isPlaceholder ? string.Empty : textBox1.Text;
            }
            set {
                if(string.IsNullOrEmpty(value)) {
                    textBox1.Text = "";
                    SetPlaceholder();
                } else {
                    isPlaceholder = false;
                    textBox1.ForeColor = mainForeColor;
                    textBox1.Text = value;
                    if(inputType == InputType.Password)
                        textBox1.UseSystemPasswordChar = true;
                }
                this.Invalidate();
            }
        }

        /// <summary>
        /// Obtiene o establece el color de fondo del control y sincroniza el cuadro de texto interno.
        /// </summary>
        [Category("Sara UI Design")]
        public override Color BackColor {
            get => base.BackColor;
            set {
                base.BackColor = value;
                if(textBox1 != null)
                    textBox1.BackColor = value;
                this.Invalidate();
            }
        }

        protected override void OnBackColorChanged(EventArgs e) {
            base.OnBackColorChanged(e);
            if(textBox1 != null)
                textBox1.BackColor = this.BackColor;
            this.Invalidate();
        }

        protected override void OnForeColorChanged(EventArgs e) {
            base.OnForeColorChanged(e);
            mainForeColor = this.ForeColor;
            if(!isPlaceholder && textBox1 != null)
                textBox1.ForeColor = this.ForeColor;
        }

        protected override void OnFontChanged(EventArgs e) {
            base.OnFontChanged(e);
            if(textBox1 != null)
                textBox1.Font = this.Font;
            UpdateControlHeight();
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
            set { iconName = value; UpdatePadding(); this.Invalidate(); }
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
        public int IconSize { get => iconSize; set { iconSize = value; UpdatePadding(); this.Invalidate(); } }

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
            set { borderSize = value; UpdatePadding(); this.Invalidate(); }
        }

        /// <summary>
        /// Define el tipo de entrada: Texto, Contraseña (máscara), Numérico (solo dígitos) o Multilínea.
        /// </summary>
        [Category("Sara UI Design")]
        public InputType Type {
            get => inputType;
            set { inputType = value; ApplyInputType(); this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el color del texto indicativo o placeholder.
        /// </summary>
        [Category("Sara UI Design")]
        public Color PlaceholderColor {
            get => placeholderColor;
            set { placeholderColor = value; if(isPlaceholder) textBox1.ForeColor = value; }
        }

        /// <summary>
        /// Obtiene o establece el texto de ayuda que desaparece cuando el usuario comienza a escribir o enfoca el control.
        /// </summary>
        [Category("Sara UI Design")]
        public string PlaceholderText {
            get => placeholderText;
            set { placeholderText = value; SetPlaceholder(); }
        }

        /// <summary>
        /// Obtiene o establece el radio de curvatura para los bordes redondeados del control.
        /// </summary>
        [Category("Sara UI Design")]
        public int BorderRadius {
            get => borderRadius;
            set { borderRadius = (value >= 0) ? value : 0; this.Invalidate(); }
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
            set { borderFocusColor = value; }
        }

        /// <summary>
        /// Si es verdadero, el control muestra únicamente una línea inferior en lugar de un borde completo perimetral.
        /// </summary>
        [Category("Sara UI Design")]
        public bool UnderlinedStyle {
            get => underlinedStyle;
            set { underlinedStyle = value; this.Invalidate(); }
        }

        // ==========================================
        // LÓGICA DE PROCESAMIENTO Y VALIDACIONES
        // ==========================================

        private void ApplyInputType() {
            switch(inputType) {
                case InputType.Multiline:
                textBox1.Multiline = true;
                textBox1.UseSystemPasswordChar = false;
                break;

                case InputType.Password:
                textBox1.Multiline = false;
                textBox1.UseSystemPasswordChar = !isPlaceholder;
                UpdateControlHeight();
                break;

                case InputType.Numeric:
                case InputType.Text:
                textBox1.Multiline = false;
                textBox1.UseSystemPasswordChar = false;
                UpdateControlHeight();
                break;
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) {
            if(inputType == InputType.Numeric && !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) {
                e.Handled = true;
            }
            this.OnKeyPress(e);
        }

        /// <summary>
        /// Activa el modo Placeholder si el control está vacío, aplicando el color y texto informativo.
        /// </summary>
        private void SetPlaceholder() {
            if(string.IsNullOrWhiteSpace(textBox1.Text) && !string.IsNullOrEmpty(placeholderText)) {
                isPlaceholder = true;
                textBox1.Text = placeholderText;
                textBox1.ForeColor = placeholderColor;

                if(inputType == InputType.Password)
                    textBox1.UseSystemPasswordChar = false;
            }
        }

        /// <summary>
        /// Elimina el Placeholder y restaura el formato de texto normal para permitir la escritura.
        /// </summary>
        private void RemovePlaceholder() {
            if(isPlaceholder) {
                isPlaceholder = false;
                textBox1.Text = "";
                textBox1.ForeColor = mainForeColor;

                if(inputType == InputType.Password)
                    textBox1.UseSystemPasswordChar = true;
            }
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
        }

        /// <summary>
        /// Gestiona el dibujo del borde, la región redondeada y la renderización del icono 
        /// con efectos de color reactivos cuando el control tiene el foco.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rectBorder = this.ClientRectangle;
            Color currentBorder = isFocused ? borderFocusColor : borderColor;

            // 1. Dibujar el Borde (Redondeado o Subrayado)
            using(Pen penBorder = new Pen(currentBorder, borderSize)) {
                penBorder.Alignment = PenAlignment.Inset;

                if(borderRadius > 1 && !underlinedStyle) {
                    using(GraphicsPath pathBorder = GetFigurePath(rectBorder, borderRadius)) {
                        this.Region = new Region(pathBorder);
                        g.DrawPath(penBorder, pathBorder);
                    }
                } else {
                    this.Region = new Region(rectBorder);
                    if(underlinedStyle)
                        g.DrawLine(penBorder, 0, this.Height - 1, this.Width, this.Height - 1);
                    else
                        g.DrawRectangle(penBorder, 0, 0, this.Width - 0.5F, this.Height - 0.5F);
                }
            }

            // 2. Dibujar el Icono desde la IconLibrary de forma dinámica
            if(!string.IsNullOrEmpty(iconName) && iconName != "None") {
                int iconX = (iconLocation == SaraIconLocation.Left) ? 12 : this.Width - iconSize - 12;
                int iconY = (this.Height - iconSize) / 2;
                Rectangle iconRect = new Rectangle(iconX, iconY, iconSize, iconSize);

                Color currentIconColor = isFocused ? borderFocusColor : iconColor;
                SaraUI_IconLibrary.DrawIcon(iconName, g, iconRect, currentIconColor);
            }
        }

        private GraphicsPath GetFigurePath(Rectangle rect, int radius) {
            GraphicsPath path = new GraphicsPath();
            float curveSize = radius * 2F;
            path.StartFigure();
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
            path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
            path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
            path.CloseFigure();
            return path;
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

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            if(this.DesignMode && inputType != InputType.Multiline) {
                UpdateControlHeight();
            }
        }
    }
}
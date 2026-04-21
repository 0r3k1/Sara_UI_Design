using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static Sara_UI_Design.SaraControls.SaraUI_TextBox;

namespace Sara_UI_Design.SaraControls {
    [DefaultEvent("_TextChanged")]
    public partial class SaraUI_TextBox:UserControl {
        // Enums para personalización
        public enum InputType { Text, Password, Numeric, Multiline }
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
        private InputType type = InputType.Text;

        // Fields de Icono
        private string iconName = "None";
        private Color iconColor = Color.Gray;
        private int iconSize = 20;
        private SaraIconLocation iconLocation = SaraIconLocation.Left;

        // Eventos
        public event EventHandler _TextChanged;

        public SaraUI_TextBox() {
            InitializeComponent();
            this.mainForeColor = this.ForeColor;
            ConfigureTextBoxEvents();
            this.Padding = new Padding(10, 7, 10, 7); // Padding base
        }

        private void ConfigureTextBoxEvents() {
            textBox1.TextChanged += (s, e) => _TextChanged?.Invoke(this, e);
            textBox1.Enter += (s, e) => { isFocused = true; RemovePlaceholder(); this.Invalidate(); };
            textBox1.Leave += (s, e) => { isFocused = false; SetPlaceholder(); this.Invalidate(); };
            textBox1.KeyPress += textBox1_KeyPress;
            textBox1.Click += (s, e) => this.OnClick(e);
        }


        [Category("Sara UI Design")]
        [TypeConverter(typeof(IconNameConverter))]
        public string IconName {
            get => iconName;
            set { iconName = value; UpdatePadding(); this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        public Color IconColor { get => iconColor; set { iconColor = value; this.Invalidate(); } }

        [Category("Sara UI Design")]
        public int IconSize { get => iconSize; set { iconSize = value; UpdatePadding(); this.Invalidate(); } }

        [Category("Sara UI Design")]
        public SaraIconLocation IconLocation { get => iconLocation; set { iconLocation = value; UpdatePadding(); this.Invalidate(); } }

        // --- Propiedades de Diseño (RESTAURADAS) ---

        [Category("Sara UI Design")]
        public int BorderSize {
            get => borderSize;
            set { borderSize = value; this.Padding = new Padding(borderSize + 5); this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        public InputType Type {
            get { return inputType; }
            set {
                inputType = value;
                ApplyInputType(); // Llamamos al nuevo método unificado
                this.Invalidate();
            }
        }

        [Category("Sara UI Design")]
        public Color PlaceholderColor {
            get => placeholderColor;
            set { placeholderColor = value; if(isPlaceholder) textBox1.ForeColor = value; }
        }

        [Category("Sara UI Design")]
        public string PlaceholderText {
            get => placeholderText;
            set { placeholderText = value; SetPlaceholder(); }
        }

        [Category("Sara UI Design")]
        public int BorderRadius {
            get => borderRadius;
            set { borderRadius = (value >= 0) ? value : 0; this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        public Color BorderColor {
            get => borderColor;
            set { borderColor = value; this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        public Color BorderFocusColor {
            get => borderFocusColor;
            set { borderFocusColor = value; }
        }

        [Category("Sara UI Design")]
        public bool UnderlinedStyle {
            get => underlinedStyle;
            set { underlinedStyle = value; this.Invalidate(); }
        }

        // --- Lógica de Validación y Tipos de Entrada ---

        private void ApplyInputType() {
            // Usamos 'inputType' para ser consistentes con tu declaración de campo
            switch(inputType) {
                case InputType.Multiline:
                textBox1.Multiline = true;
                textBox1.UseSystemPasswordChar = false;
                // No llamamos a UpdateControlHeight para que el usuario pueda estirarlo
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
            // Cambiado 'type' por 'inputType'
            if(inputType == InputType.Numeric && !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) {
                e.Handled = true;
            }
            this.OnKeyPress(e);
        }

        // --- Lógica del Placeholder ---

        private void SetPlaceholder() {
            if(string.IsNullOrWhiteSpace(textBox1.Text) && !string.IsNullOrEmpty(placeholderText)) {
                isPlaceholder = true;
                textBox1.Text = placeholderText;
                textBox1.ForeColor = placeholderColor;

                // Si es contraseña, quitamos los puntos para que se lea el mensaje (ej: "Contraseña")
                if(inputType == InputType.Password)
                    textBox1.UseSystemPasswordChar = false;
            }
        }

        private void RemovePlaceholder() {
            if(isPlaceholder) {
                isPlaceholder = false;
                textBox1.Text = "";
                textBox1.ForeColor = mainForeColor;

                if(inputType == InputType.Password)
                    textBox1.UseSystemPasswordChar = true;
            }
        }

        // --- Dibujo Avanzado (OnPaint mejorado) ---

        // --- Lógica de Padding Dinámico ---

        private void UpdatePadding() {
            // Si hay icono, aumentamos el padding del lado correspondiente para que el texto no lo tape
            int left = 10;
            int right = 10;
            bool hasIcon = !string.IsNullOrEmpty(iconName) && iconName != "None";

            if(hasIcon) {
                if(iconLocation == SaraIconLocation.Left)
                    left = iconSize + 15;
                else
                    right = iconSize + 15;
            }

            this.Padding = new Padding(left, 7, right, 7);
        }

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

            // 2. Dibujar el Icono de la IconLibrary
            if(!string.IsNullOrEmpty(iconName) && iconName != "None") {
                int iconX = (iconLocation == SaraIconLocation.Left) ? 10 : this.Width - iconSize - 10;
                int iconY = (this.Height - iconSize) / 2;
                Rectangle iconRect = new Rectangle(iconX, iconY, iconSize, iconSize);

                // El icono brilla con el color de foco si está seleccionado
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

        private void UpdateControlHeight() {
            // Si el TextBox interno es multilínea, NO debemos forzar la altura del UserControl
            if(textBox1.Multiline == false) {
                int txtHeight = TextRenderer.MeasureText("Text", this.Font).Height + 1;

                // Establecemos una altura mínima para que el control no desaparezca
                textBox1.MinimumSize = new Size(0, txtHeight);

                // La altura del UserControl será la del texto + el padding (superior e inferior)
                this.Height = textBox1.Height + this.Padding.Top + this.Padding.Bottom;
            }
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            // Solo forzamos la altura si NO es multilínea. 
            // Si es multilínea, dejamos que el usuario lo estire libremente.
            if(this.DesignMode && inputType != InputType.Multiline) {
                UpdateControlHeight();
            }
        }
    }
}
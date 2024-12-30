using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Drawing.Drawing2D;

namespace Sara_UI_Design {

    [DefaultEvent("_TextChanged")]
    public partial class SaraUI_TextBox : UserControl {

        public enum InputType {
            Text,
            Password,
            Email,
            Numeric,
            Multiline
        }

        //Fields
        private Color borderColor = Color.MediumSlateBlue;
        private int borderSize = 2;
        private bool underlinedStyle = false;
        private Color borderFocusColor = Color.HotPink;
        private bool isFocused = false;
        private string placeholderText = "";
        private bool isPlaceholder = true;
        private InputType inputType = InputType.Text;
        private Color placeholderColor = Color.Gray;
        private Color textColor = Color.Black;
        private Color errorBorderColor = Color.Red;
        private Label errorLabel;
        private int borderRadius = 0;

        public SaraUI_TextBox() {
            InitializeComponent();
            textBox1.Text = placeholderText;
            textBox1.ForeColor = placeholderColor;
            textBox1.BackColor = BackColor;
            textBox1.Enter += RemovePlaceholder;
            textBox1.Leave += SetPlaceholder;
            textBox1.KeyPress += InputValidation;
            textBox1.Validating += ValidateInput;
            CreateErrorLabel();
        }

        // Methods

        private void CreateErrorLabel() {
            errorLabel = new Label {
                AutoSize = true,
                ForeColor = errorBorderColor,
                Visible = false,
                Font = new Font(this.Font.FontFamily, this.Font.Size - 2),
                Location = new Point(0, this.Height + 2)
            };
            this.Controls.Add(errorLabel);
        }

        private void RemovePlaceholder(object sender, EventArgs e) {
            if(isPlaceholder) {
                textBox1.Text = "";
                textBox1.BackColor = this.BackColor;
                if(inputType == InputType.Password)
                    textBox1.UseSystemPasswordChar = true;
                isPlaceholder = false;
            }
        }

        private void SetPlaceholder(object sender, EventArgs e) {
            if(string.IsNullOrWhiteSpace(textBox1.Text)) {
                isPlaceholder = true;
                textBox1.Text = placeholderText;
                textBox1.BackColor = this.BackColor;
                if(inputType == InputType.Password)
                    textBox1.UseSystemPasswordChar = false;
            }
        }

        private void ConfigureInputType() {
            switch(inputType) {
                case InputType.Text:
                textBox1.UseSystemPasswordChar = false;
                textBox1.MaxLength = 0;
                textBox1.Multiline = false;
                break;
                case InputType.Password:
                textBox1.UseSystemPasswordChar = !isPlaceholder;
                textBox1.Multiline = false;
                break;
                case InputType.Email:
                textBox1.UseSystemPasswordChar = false;
                textBox1.Multiline = false;
                break;
                case InputType.Numeric:
                textBox1.UseSystemPasswordChar = false;
                textBox1.Multiline = false;
                break;
                case InputType.Multiline:
                textBox1.UseSystemPasswordChar = false;
            textBox1.Multiline = true;
                break;
            }
        }

        private void InputValidation(object sender, KeyPressEventArgs e) {
            if(inputType == InputType.Numeric) {
                if(!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) {
                    e.Handled = true;
                }
            }
        }

        private void ValidateInput(object sender, CancelEventArgs e) {
            if(inputType == InputType.Email && !ValidateEmail()) {
                e.Cancel = true;
                ShowError("Invalid email address.");
            } else {
                HideError();
            }
        }

        private void ShowError(string message) {
            errorLabel.Text = message;
            errorLabel.Visible = true;
            this.BackColor = errorBorderColor;
        }

        private void HideError() {
            errorLabel.Visible = false;
            this.BackColor = BackColor;
        }

        public bool ValidateEmail() {
            if(inputType == InputType.Email && !isPlaceholder) {
                string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(textBox1.Text, emailPattern);
            }
            return true;
        }

        private void UpdateControlHeight() {
            if(textBox1.Multiline == false) {
                int txtHeight = TextRenderer.MeasureText("Text", this.Font).Height + 1;
                textBox1.Multiline = true;
                textBox1.MinimumSize = new Size(0, txtHeight);
                textBox1.Multiline = false;
                this.Height = textBox1.Height + this.Padding.Top + this.Padding.Bottom;
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
        private void SetTextBoxRoundedRegion() {
            GraphicsPath pathTxt;
            if(inputType == InputType.Multiline) {
                pathTxt = GetFigurePath(textBox1.ClientRectangle, borderRadius - borderSize);
                textBox1.Region = new Region(pathTxt);
            } else {
                pathTxt = GetFigurePath(textBox1.ClientRectangle, borderSize * 2);
                textBox1.Region = new Region(pathTxt);
            }
            pathTxt.Dispose();
        }

        // Overriden properties to integrate placeholder behavior
        public override string Text {
            get { return isPlaceholder ? "" : textBox1.Text; }
            set {
                textBox1.Text = value;
                RemovePlaceholder(null, null);
            }
        }

        //Properties
        [Category("Sara UI Desing")]
        public Color BorderColor {
            get { return borderColor; }
            set {
                borderColor = value;
                this.Invalidate();
            }
        }
        [Category("Sara UI Desing")]
        public int BorderSize {
            get { return borderSize; }
            set {
                borderSize = value;
                this.Invalidate();
            }
        }

        [Category("Sara UI Desing")]
        public bool UnderlinedStyle {
            get { return underlinedStyle; }
            set {
                underlinedStyle = value;
                this.Invalidate();
            }
        }

        //[Category("Sara UI Desing")]
        //public bool PasswordChar {
        //    get { return textBox1.UseSystemPasswordChar; }
        //    set { textBox1.UseSystemPasswordChar = value; }
        //}

        //[Category("Sara UI Desing")]
        //public bool Multiline {
        //    get { return textBox1.Multiline; }
        //    set { textBox1.Multiline = value; }
        //}

        [Category("Sara UI Desing")]
        public override Color BackColor {
            get { return base.BackColor; }
            set {
                base.BackColor = value;
                textBox1.BackColor = value;
            }
        }


        [Category("Sara UI Desing")]
        public override Color ForeColor {
            get { return base.ForeColor; }
            set {
                base.ForeColor = value;
                textBox1.ForeColor = value;
            }
        }

        [Category("Sara UI Desing")]
        public override Font Font {
            get { return base.Font; }
            set {
                base.Font = value;
                textBox1.Font = value;
                if(this.DesignMode)
                    UpdateControlHeight();
            }
        }

        [Category("Sara UI Desing")]
        public string Texts {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        [Category("Sara UI Desing")]
        public Color BorderFocusColor {
            get { return borderFocusColor; }
            set { borderFocusColor = value; }
        }

        [Category("Sara UI Desing")]
        public string Placeholder {
            get { return placeholderText; }
            set {
                placeholderText = value;
                SetPlaceholder(null, null);
            }
        }

        [Category("Sara UI Desing")]
        public Color PlaceholderColor {
            get { return placeholderColor; }
            set {
                placeholderColor = value;
                if(isPlaceholder)
                    textBox1.ForeColor = value;
            }
        }

        [Category("Sara UI Desing")]
        public InputType Type {
            get { return inputType; }
            set {
                inputType = value;
                ConfigureInputType();
            }
        }

        [Category("Sara UI Desing")]
        public Color ErrorBorderColor {
            get { return errorBorderColor; }
            set { errorBorderColor = value; }
        }

        [Category("Sara UI Desing")]
        public int BorderRadius {
            get { return borderRadius; }
            set {
                if(value >= 0) {
                    borderRadius = value;
                    this.Invalidate();//Redraw control
                }
            }
        }


        //Overridden methods

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            if(this.DesignMode)
                UpdateControlHeight();
        }
        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            UpdateControlHeight();
        }
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            Graphics graph = e.Graphics;
            if(borderRadius > 1)//Rounded TextBox
            {
                //-Fields
                var rectBorderSmooth = this.ClientRectangle;
                var rectBorder = Rectangle.Inflate(rectBorderSmooth, -borderSize, -borderSize);
                int smoothSize = borderSize > 0 ? borderSize : 1;
                using(GraphicsPath pathBorderSmooth = GetFigurePath(rectBorderSmooth, borderRadius))
                using(GraphicsPath pathBorder = GetFigurePath(rectBorder, borderRadius - borderSize))
                using(Pen penBorderSmooth = new Pen(this.Parent.BackColor, smoothSize))
                using(Pen penBorder = new Pen(borderColor, borderSize)) {
                    //-Drawing
                    this.Region = new Region(pathBorderSmooth);//Set the rounded region of UserControl
                    if(borderRadius > 15) SetTextBoxRoundedRegion();//Set the rounded region of TextBox component
                    graph.SmoothingMode = SmoothingMode.AntiAlias;
                    penBorder.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
                    if(isFocused) penBorder.Color = borderFocusColor;
                    if(underlinedStyle) //Line Style
                    {
                        //Draw border smoothing
                        graph.DrawPath(penBorderSmooth, pathBorderSmooth);
                        //Draw border
                        graph.SmoothingMode = SmoothingMode.None;
                        graph.DrawLine(penBorder, 0, this.Height - 1, this.Width, this.Height - 1);
                    } else //Normal Style
                      {
                        //Draw border smoothing
                        graph.DrawPath(penBorderSmooth, pathBorderSmooth);
                        //Draw border
                        graph.DrawPath(penBorder, pathBorder);
                    }
                }
            } else //Square/Normal TextBox
              {
                //Draw border
                using(Pen penBorder = new Pen(borderColor, borderSize)) {
                    this.Region = new Region(this.ClientRectangle);
                    penBorder.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
                    if(isFocused) penBorder.Color = borderFocusColor;
                    if(underlinedStyle) //Line Style
                        graph.DrawLine(penBorder, 0, this.Height - 1, this.Width, this.Height - 1);
                    else //Normal Style
                        graph.DrawRectangle(penBorder, 0, 0, this.Width - 0.5F, this.Height - 0.5F);
                }
            }
        }

        //Change border color in focus mode
        private void textBox1_Enter(object sender, EventArgs e) {
            isFocused = true;
            this.Invalidate();
        }

        private void textBox1_Leave(object sender, EventArgs e) {
            isFocused = false;
            this.Invalidate();
            // Aquí aseguramos que no se sobrescriba el BackColor si no hay cambios
            if(!isPlaceholder) {
                textBox1.BackColor = this.BackColor;
            }
        }

        public event EventHandler _TextChanged;

        //TextBox-> TextChanged event
        private void textBox1_TextChanged(object sender, EventArgs e) {
            if(_TextChanged != null)
                _TextChanged.Invoke(sender, e);
        }


        //TextBox events
        private void textBox1_Click(object sender, EventArgs e) {
            this.OnClick(e);
        }

        private void textBox1_MouseEnter(object sender, EventArgs e) {
            this.OnMouseEnter(e);
        }

        private void textBox1_MouseLeave(object sender, EventArgs e) {
            this.OnMouseLeave(e);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) {
            this.OnKeyPress(e);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e) {
            this.OnKeyDown(e);
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e) {
            this.OnKeyUp(e);
        }

        private void textBox1_Validated(object sender, EventArgs e) {
            this.OnValidated(e);
        }

        private void textBox1_Validating(object sender, CancelEventArgs e) {
            this.OnValidating(e);
        }
    }
}

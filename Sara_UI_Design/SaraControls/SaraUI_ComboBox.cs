using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Drawing.Design;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Control de lista desplegable personalizado de la suite Sara UI. 
    /// Combina un ComboBox estándar con elementos visuales personalizados para un diseño moderno.
    /// </summary>
    [DefaultEvent("OnSelectedIndexChanged")]
    public class SaraUI_ComboBox:UserControl {
        //Fields
        private Color backColor = Color.WhiteSmoke;
        private Color iconColor = Color.MediumSlateBlue;
        private Color listBackColor = Color.FromArgb(230, 228, 245);
        private Color listTextColor = Color.DimGray;
        private Color borderColor = Color.MediumSlateBlue;
        private int borderSize = 1;
        private bool isFocused = false;

        //Items
        private ComboBox cmbList;
        private Label lblText;
        private Button btnIcon;

        
        /// <summary>
        /// Ocurre cuando el índice de la selección ha cambiado. 
        /// Este es el evento principal para capturar la interacción del usuario.
        /// </summary>
        public event EventHandler OnSelectedIndexChanged;//Default event

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_ComboBox"/>, configurando 
        /// los sub-controles internos (botón, etiqueta y lista) con el estilo visual inicial.
        /// </summary>
        public SaraUI_ComboBox() {
            cmbList = new ComboBox();
            lblText = new Label();
            btnIcon = new Button();
            this.SuspendLayout();
            //ComboBox: Dropdown list
            cmbList.BackColor = listBackColor;
            cmbList.Font = new Font(this.Font.Name, 10F);
            cmbList.ForeColor = listTextColor;
            cmbList.SelectedIndexChanged += new EventHandler(ComboBox_SelectedIndexChanged);//Default event
            cmbList.TextChanged += new EventHandler(ComboBox_TextChanged);//Refresh text
                                                                          //Button: Icon
            btnIcon.Dock = DockStyle.Right;
            btnIcon.FlatStyle = FlatStyle.Flat;
            btnIcon.FlatAppearance.BorderSize = 0;
            btnIcon.BackColor = backColor;
            btnIcon.Size = new Size(30, 30);
            btnIcon.Cursor = Cursors.Hand;
            btnIcon.Click += new EventHandler(Icon_Click);//Open dropdown list
            btnIcon.Paint += new PaintEventHandler(Icon_Paint);//Draw icon
                                                               //Label: Text
            lblText.Dock = DockStyle.Fill;
            lblText.AutoSize = false;
            lblText.BackColor = backColor;
            lblText.TextAlign = ContentAlignment.MiddleLeft;
            lblText.Padding = new Padding(8, 0, 0, 0);
            lblText.Font = new Font(this.Font.Name, 10F);
            //->Attach label events to user control event
            lblText.Click += new EventHandler(Surface_Click);//Select combo box
            lblText.MouseEnter += new EventHandler(Surface_MouseEnter);
            lblText.MouseLeave += new EventHandler(Surface_MouseLeave);
            //User Control
            this.Controls.Add(lblText);//2
            this.Controls.Add(btnIcon);//1
            this.Controls.Add(cmbList);//0
            this.MinimumSize = new Size(200, 30);
            this.Size = new Size(200, 30);
            this.ForeColor = Color.DimGray;
            this.Padding = new Padding(borderSize);//Border Size
            this.Font = new Font(this.Font.Name, 10F);
            base.BackColor = borderColor; //Border Color
            this.ResumeLayout();
            AdjustComboBoxDimensions();
        }

        /// <summary>
        /// Ajusta dinámicamente el tamaño y posición del ComboBox interno para que coincida con el diseño del UserControl.
        /// </summary>
        private void AdjustComboBoxDimensions() {
            cmbList.Width = lblText.Width;
            cmbList.Location = new Point() {
                X = this.Width - this.Padding.Right - cmbList.Width,
                Y = lblText.Bottom - cmbList.Height
            };
        }

        //Event methods

        //-> Default event
        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            if(OnSelectedIndexChanged != null)
                OnSelectedIndexChanged.Invoke(sender, e);
            //Refresh text
            lblText.Text = cmbList.Text;
        }
        //-> Items actions
        private void Icon_Click(object sender, EventArgs e) {
            //Open dropdown list
            cmbList.Select();
            cmbList.DroppedDown = true;
        }
        private void Surface_Click(object sender, EventArgs e) {
            //Attach label click to user control click
            this.OnClick(e);
            //Select combo box
            cmbList.Select();
            if(cmbList.DropDownStyle == ComboBoxStyle.DropDownList)
                cmbList.DroppedDown = true;//Open dropdown list
        }
        private void ComboBox_TextChanged(object sender, EventArgs e) {
            //Refresh text
            lblText.Text = cmbList.Text;
        }

        //-> Draw icon
        private void Icon_Paint(object sender, PaintEventArgs e) {
            int iconWidth = 12;
            int iconHeight = 6;
            var rectIcon = new Rectangle((btnIcon.Width - iconWidth) / 2, (btnIcon.Height - iconHeight) / 2, iconWidth, iconHeight);
            Graphics graph = e.Graphics;

            using(GraphicsPath path = new GraphicsPath())
            using(Pen pen = new Pen(iconColor, 2)) {
                graph.SmoothingMode = SmoothingMode.AntiAlias;
                path.AddLine(rectIcon.X, rectIcon.Y, rectIcon.X + (iconWidth / 2), rectIcon.Bottom);
                path.AddLine(rectIcon.X + (iconWidth / 2), rectIcon.Bottom, rectIcon.Right, rectIcon.Y);
                graph.DrawPath(pen, path);
            }
        }

        //Properties
        //-> Appearance
        [Category("Sara UI Design")]
        public override Color BackColor {
            get => base.BackColor;
            set {
                base.BackColor = value; // El borde real
                if(lblText != null)
                    lblText.BackColor = value;
                if(btnIcon != null)
                    btnIcon.BackColor = value;
                backColor = value; // Tu variable interna
            }
        }

        /// <summary>
        /// Obtiene o establece el color de la flecha indicadora del menú desplegable.
        /// </summary>
        [Category("Sara UI Desingn")]
        public Color IconColor {
            get { return iconColor; }
            set {
                iconColor = value;
                btnIcon.Invalidate();//Redraw icon
            }
        }

        /// <summary>
        /// Obtiene o establece el color de fondo de la lista desplegable (el área de los ítems).
        /// </summary>
        [Category("Sara UI Desingn")]
        public Color ListBackColor {
            get { return listBackColor; }
            set {
                listBackColor = value;
                cmbList.BackColor = listBackColor;
            }
        }

        [Category("Sara UI Desingn")]
        public Color ListTextColor {
            get { return listTextColor; }
            set {
                listTextColor = value;
                cmbList.ForeColor = listTextColor;
            }
        }

        [Category("Sara UI Desingn")]
        public Color BorderColor {
            get { return borderColor; }
            set {
                borderColor = value;
                base.BackColor = borderColor; //Border Color
            }
        }

        /// <summary>
        /// Obtiene o establece el grosor del borde exterior del control.
        /// </summary>
        [Category("Sara UI Desingn")]
        public int BorderSize {
            get { return borderSize; }
            set {
                borderSize = value;
                this.Padding = new Padding(borderSize);//Border Size
                AdjustComboBoxDimensions();
            }
        }

        [Category("Sara UI Desingn")]
        public override Color ForeColor {
            get { return base.ForeColor; }
            set {
                base.ForeColor = value;
                lblText.ForeColor = value;
            }
        }

        [Category("Sara UI Desingn")]
        public override Font Font {
            get { return base.Font; }
            set {
                base.Font = value;
                lblText.Font = value;
                cmbList.Font = value;//Optional
            }
        }

        /// <summary>
        /// Obtiene o establece el texto mostrado actualmente en la etiqueta del control.
        /// </summary>
        [Category("Sara UI Desingn")]
        public string Texts {
            get { return lblText.Text; }
            set { lblText.Text = value; }
        }

        /// <summary>
        /// Obtiene o establece un valor que especifica el estilo del cuadro combinado (DropDown o DropDownList).
        /// </summary>
        [Category("Sara UI Desingn")]
        public ComboBoxStyle DropDownStyle {
            get { return cmbList.DropDownStyle; }
            set {
                if(cmbList.DropDownStyle != ComboBoxStyle.Simple)
                    cmbList.DropDownStyle = value;
            }
        }

        /// <summary>
        /// Obtiene la colección de elementos contenidos en este ComboBox.
        /// </summary>
        [Category("Sara UI Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [Localizable(true)]
        [MergableProperty(false)]
        public ComboBox.ObjectCollection Items {
            get { return cmbList.Items; }
        }

        /// <summary>
        /// Obtiene o establece el origen de datos para el control.
        /// </summary>
        [Category("Sara UI Data")]
        [AttributeProvider(typeof(IListSource))]
        [DefaultValue(null)]
        public object DataSource {
            get { return cmbList.DataSource; }
            set { cmbList.DataSource = value; }
        }

        [Category("Sara UI Data")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Localizable(true)]
        public AutoCompleteStringCollection AutoCompleteCustomSource {
            get { return cmbList.AutoCompleteCustomSource; }
            set { cmbList.AutoCompleteCustomSource = value; }
        }

        [Category("Sara UI Data")]
        [Browsable(true)]
        [DefaultValue(AutoCompleteSource.None)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public AutoCompleteSource AutoCompleteSource {
            get { return cmbList.AutoCompleteSource; }
            set { cmbList.AutoCompleteSource = value; }
        }

        [Category("Sara UI Data")]
        [Browsable(true)]
        [DefaultValue(AutoCompleteMode.None)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public AutoCompleteMode AutoCompleteMode {
            get { return cmbList.AutoCompleteMode; }
            set { cmbList.AutoCompleteMode = value; }
        }

        [Category("Sara UI Data")]
        [Bindable(true)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem {
            get { return cmbList.SelectedItem; }
            set { cmbList.SelectedItem = value; }
        }

        /// <summary>
        /// Obtiene o establece el índice que especifica el elemento seleccionado actualmente.
        /// </summary>
        [Category("Sara UI Data")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex {
            get { return cmbList.SelectedIndex; }
            set { cmbList.SelectedIndex = value; }
        }

        /// <summary>
        /// Obtiene o establece la propiedad que se va a mostrar para este control.
        /// </summary>
        [Category("Sara UI Data")]
        [DefaultValue("")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string DisplayMember {
            get { return cmbList.DisplayMember; }
            set { cmbList.DisplayMember = value; }
        }

        [Category("Sara UI Data")]
        [DefaultValue("")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ValueMember {
            get { return cmbList.ValueMember; }
            set { cmbList.ValueMember = value; }
        }

        //->Attach label events to user control event
        private void Surface_MouseLeave(object sender, EventArgs e) {
            this.OnMouseLeave(e);
        }

        private void Surface_MouseEnter(object sender, EventArgs e) {
            this.OnMouseEnter(e);
        }

        private void ComboBox_Enter(object sender, EventArgs e) {
            isFocused = true;
            this.Refresh();
        }

        private void ComboBox_Leave(object sender, EventArgs e) {
            isFocused = false;
            this.Refresh();
        }

        /// <summary>
        /// Dibuja mediante vectores la forma de la flecha (V) en el botón lateral del control.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            Graphics graph = e.Graphics;

            // Dibujar el borde manualmente para que sea profesional
            using(Pen penBorder = new Pen(isFocused ? Color.HotPink : borderColor, borderSize)) {
                penBorder.Alignment = PenAlignment.Inset;
                graph.DrawRectangle(penBorder, 0, 0, this.Width - 0.5F, this.Height - 0.5F);
            }
        }

        //Overridden methods
        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            AdjustComboBoxDimensions();
        }
    }
}

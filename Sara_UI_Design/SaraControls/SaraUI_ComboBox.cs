using System;
using System.Collections.Generic;
using System.Linq;
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
        // Fields de diseño
        private Color iconColor = Color.MediumSlateBlue;
        private Color listBackColor = Color.FromArgb(230, 228, 245);
        private Color listTextColor = Color.DimGray;
        private Color borderColor = Color.MediumSlateBlue;
        private Color borderFocusColor = Color.HotPink;
        private int borderSize = 2; // Ajustado a 2 para consistencia con la suite
        private bool isFocused = false;

        // Sub-controles internos
        private ComboBox cmbList;
        private Label lblText;
        private Button btnIcon;

        /// <summary>
        /// Ocurre cuando el índice de la selección ha cambiado. 
        /// Este es el evento principal para capturar la interacción del usuario.
        /// </summary>
        public event EventHandler OnSelectedIndexChanged;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SaraUI_ComboBox"/>, configurando 
        /// los sub-controles internos con el estilo visual fluido de Sara UI.
        /// </summary>
        public SaraUI_ComboBox() {
            cmbList = new ComboBox();
            lblText = new Label();
            btnIcon = new Button();
            this.SuspendLayout();

            // 1. ComboBox interno (Oculto matemáticamente)
            cmbList.BackColor = listBackColor;
            cmbList.Font = new Font(this.Font.Name, 10F);
            cmbList.ForeColor = listTextColor;
            cmbList.SelectedIndexChanged += new EventHandler(ComboBox_SelectedIndexChanged);
            cmbList.TextChanged += new EventHandler(ComboBox_TextChanged);

            // ¡CONEXIÓN DE EVENTOS CRÍTICOS DE FOCO!
            cmbList.Enter += new EventHandler(ComboBox_Enter);
            cmbList.Leave += new EventHandler(ComboBox_Leave);

            // 2. Botón lateral del Icono
            btnIcon.Dock = DockStyle.Right;
            btnIcon.FlatStyle = FlatStyle.Flat;
            btnIcon.FlatAppearance.BorderSize = 0;
            btnIcon.BackColor = this.BackColor;
            btnIcon.Size = new Size(30, 30);
            btnIcon.Cursor = Cursors.Hand;
            btnIcon.Click += new EventHandler(Icon_Click);
            btnIcon.Paint += new PaintEventHandler(Icon_Paint);

            // 3. Label de superficie (Muestra el texto seleccionado)
            lblText.Dock = DockStyle.Fill;
            lblText.AutoSize = false;
            lblText.BackColor = this.BackColor;
            lblText.TextAlign = ContentAlignment.MiddleLeft;
            lblText.Padding = new Padding(8, 0, 0, 0);
            lblText.Font = new Font(this.Font.Name, 10F);

            // Reenvío de eventos hacia el contenedor principal
            lblText.Click += new EventHandler(Surface_Click);
            lblText.MouseEnter += (s, e) => this.OnMouseEnter(e);
            lblText.MouseLeave += (s, e) => this.OnMouseLeave(e);

            // 4. Configuración del Contenedor de Usuario (UserControl)
            this.Controls.Add(lblText);
            this.Controls.Add(btnIcon);
            this.Controls.Add(cmbList);

            this.MinimumSize = new Size(200, 30);
            this.Size = new Size(200, 30);
            this.ForeColor = Color.DimGray;
            this.Font = new Font(this.Font.Name, 10F);
            this.Padding = new Padding(borderSize);

            this.ResumeLayout(false);
            AdjustComboBoxDimensions();
        }

        private void AdjustComboBoxDimensions() {
            cmbList.Width = lblText.Width;
            cmbList.Location = new Point() {
                X = this.Width - this.Padding.Right - cmbList.Width,
                Y = lblText.Bottom - cmbList.Height
            };
        }

        // ==========================================
        // PROCESAMIENTO DE EVENTOS INTERNOS
        // ==========================================

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            OnSelectedIndexChanged?.Invoke(this, e);
            lblText.Text = cmbList.Text;
        }

        private void Icon_Click(object sender, EventArgs e) {
            cmbList.Select();
            cmbList.DroppedDown = true;
        }

        private void Surface_Click(object sender, EventArgs e) {
            this.OnClick(e);
            cmbList.Select();
            if(cmbList.DropDownStyle == ComboBoxStyle.DropDownList)
                cmbList.DroppedDown = true;
        }

        private void ComboBox_TextChanged(object sender, EventArgs e) {
            lblText.Text = cmbList.Text;
        }

        // INTEGRACIÓN SARAUI_ICONLIBRARY
        private void Icon_Paint(object sender, PaintEventArgs e) {
            int iconSize = 12;
            // Cuadro central de renderizado para la flecha
            var rectIcon = new Rectangle((btnIcon.Width - iconSize) / 2, (btnIcon.Height - iconSize) / 2, iconSize, iconSize);

            // Llamamos dinámicamente al icono de flecha hacia abajo de tu framework
            SaraUI_IconLibrary.DrawIcon("ChevronDown", e.Graphics, rectIcon, iconColor);
        }

        private void ComboBox_Enter(object sender, EventArgs e) {
            isFocused = true;
            this.Invalidate(); // Fuerza el redibujado del borde rosa
        }

        private void ComboBox_Leave(object sender, EventArgs e) {
            isFocused = false;
            this.Invalidate(); // Retorna al borde pasivo
        }

        // ==========================================
        // PROPIEDADES SARA UI DESIGN
        // ==========================================

        [Category("Sara UI Design")]
        public override Color BackColor {
            get => base.BackColor;
            set {
                base.BackColor = value;
                if(lblText != null)
                    lblText.BackColor = value;
                if(btnIcon != null)
                    btnIcon.BackColor = value;
            }
        }

        /// <summary>
        /// Obtiene o establece el color de la flecha indicadora del menú desplegable.
        /// </summary>
        [Category("Sara UI Design")]
        public Color IconColor {
            get => iconColor;
            set { iconColor = value; btnIcon.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el color de fondo de la lista desplegable (el área de los ítems).
        /// </summary>
        [Category("Sara UI Design")]
        public Color ListBackColor {
            get => listBackColor;
            set { listBackColor = value; cmbList.BackColor = value; }
        }

        [Category("Sara UI Design")]
        public Color ListTextColor {
            get => listTextColor;
            set { listTextColor = value; cmbList.ForeColor = value; }
        }

        [Category("Sara UI Design")]
        public Color BorderColor {
            get => borderColor;
            set { borderColor = value; this.Invalidate(); }
        }

        [Category("Sara UI Design")]
        public Color BorderFocusColor {
            get => borderFocusColor;
            set { borderFocusColor = value; this.Invalidate(); }
        }

        /// <summary>
        /// Obtiene o establece el grosor del borde exterior del control.
        /// </summary>
        [Category("Sara UI Design")]
        public int BorderSize {
            get => borderSize;
            set {
                borderSize = value;
                this.Padding = new Padding(borderSize);
                AdjustComboBoxDimensions();
                this.Invalidate();
            }
        }

        [Category("Sara UI Design")]
        public override Color ForeColor {
            get => base.ForeColor;
            set { base.ForeColor = value; lblText.ForeColor = value; }
        }

        [Category("Sara UI Design")]
        public override Font Font {
            get => base.Font;
            set {
                base.Font = value;
                lblText.Font = value;
                cmbList.Font = value;
            }
        }

        /// <summary>
        /// Obtiene o establece el texto mostrado actualmente en la etiqueta del control.
        /// </summary>
        [Category("Sara UI Design")]
        public string Texts {
            get => lblText.Text;
            set => lblText.Text = value;
        }

        /// <summary>
        /// Obtiene o establece un valor que especifica el estilo del cuadro combinado (DropDown o DropDownList).
        /// </summary>
        [Category("Sara UI Design")]
        public ComboBoxStyle DropDownStyle {
            get => cmbList.DropDownStyle;
            set { if(cmbList.DropDownStyle != ComboBoxStyle.Simple) cmbList.DropDownStyle = value; }
        }

        // ==========================================
        // PROPIEDADES DE DATOS BINDING
        // ==========================================

        [Category("Sara UI Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [Localizable(true)]
        [MergableProperty(false)]
        public ComboBox.ObjectCollection Items => cmbList.Items;

        [Category("Sara UI Data")]
        [AttributeProvider(typeof(IListSource))]
        [DefaultValue(null)]
        public object DataSource { get => cmbList.DataSource; set => cmbList.DataSource = value; }

        [Category("Sara UI Data")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Localizable(true)]
        public AutoCompleteStringCollection AutoCompleteCustomSource { get => cmbList.AutoCompleteCustomSource; set => cmbList.AutoCompleteCustomSource = value; }

        [Category("Sara UI Data")]
        [Browsable(true)]
        [DefaultValue(AutoCompleteSource.None)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public AutoCompleteSource AutoCompleteSource { get => cmbList.AutoCompleteSource; set => cmbList.AutoCompleteSource = value; }

        [Category("Sara UI Data")]
        [Browsable(true)]
        [DefaultValue(AutoCompleteMode.None)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public AutoCompleteMode AutoCompleteMode { get => cmbList.AutoCompleteMode; set => cmbList.AutoCompleteMode = value; }

        [Category("Sara UI Data")]
        [Bindable(true)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem { get => cmbList.SelectedItem; set => cmbList.SelectedItem = value; }

        [Category("Sara UI Data")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex { get => cmbList.SelectedIndex; set => cmbList.SelectedIndex = value; }

        [Category("Sara UI Data")]
        [DefaultValue("")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string DisplayMember { get => cmbList.DisplayMember; set => cmbList.DisplayMember = value; }

        [Category("Sara UI Data")]
        [DefaultValue("")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ValueMember { get => cmbList.ValueMember; set => cmbList.ValueMember = value; }

        [Category("Sara UI Data")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedValue {
            get => cmbList.SelectedValue;
            set => cmbList.SelectedValue = value;
        }

        // ==========================================
        // DIBUJO VECTORIAL
        // ==========================================

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            Graphics graph = e.Graphics;
            graph.SmoothingMode = SmoothingMode.AntiAlias;

            // Dibujamos el rectángulo de borde limpio usando GDI+
            using(Pen penBorder = new Pen(isFocused ? borderFocusColor : borderColor, borderSize)) {
                penBorder.Alignment = PenAlignment.Inset;
                graph.DrawRectangle(penBorder, 0, 0, this.Width - 0.5F, this.Height - 0.5F);
            }
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            AdjustComboBoxDimensions();
        }
    }
}
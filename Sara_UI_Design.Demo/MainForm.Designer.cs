namespace Sara_UI_Design.Demo {
    partial class MainForm {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            sarauI_GridPanel1 = new Sara_UI_Design.SaraControls.SaraUI_GridPanel();
            panel2 = new Panel();
            panel1 = new Panel();
            sarauI_ComboBox1 = new Sara_UI_Design.SaraControls.SaraUI_ComboBox();
            sarauI_ComboBox2 = new Sara_UI_Design.SaraControls.SaraUI_ComboBox();
            sarauI_GridPanel1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // sarauI_GridPanel1
            // 
            sarauI_GridPanel1.BorderRadius = 0;
            sarauI_GridPanel1.ColumnGap = 0;
            sarauI_GridPanel1.ColumnsConfig = "1fr, 1fr";
            sarauI_GridPanel1.Controls.Add(panel2);
            sarauI_GridPanel1.Controls.Add(panel1);
            sarauI_GridPanel1.Dock = DockStyle.Fill;
            sarauI_GridPanel1.Location = new Point(0, 0);
            sarauI_GridPanel1.Name = "sarauI_GridPanel1";
            sarauI_GridPanel1.RowGap = 10;
            sarauI_GridPanel1.RowsConfig = "1fr";
            sarauI_GridPanel1.Size = new Size(1190, 725);
            sarauI_GridPanel1.TabIndex = 0;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.InactiveCaption;
            panel2.Location = new Point(595, 10);
            panel2.Name = "panel2";
            panel2.Size = new Size(585, 705);
            panel2.TabIndex = 1;
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ActiveCaption;
            panel1.Controls.Add(sarauI_ComboBox2);
            panel1.Controls.Add(sarauI_ComboBox1);
            panel1.Location = new Point(10, 10);
            panel1.Name = "panel1";
            panel1.Size = new Size(585, 705);
            panel1.TabIndex = 0;
            // 
            // sarauI_ComboBox1
            // 
            sarauI_ComboBox1.BorderColor = Color.MediumSlateBlue;
            sarauI_ComboBox1.BorderFocusColor = Color.HotPink;
            sarauI_ComboBox1.BorderSize = 2;
            sarauI_ComboBox1.DropDownStyle = ComboBoxStyle.DropDown;
            sarauI_ComboBox1.Font = new Font("Segoe UI", 10F);
            sarauI_ComboBox1.ForeColor = Color.DimGray;
            sarauI_ComboBox1.IconColor = Color.MediumSlateBlue;
            sarauI_ComboBox1.ListBackColor = Color.FromArgb(230, 228, 245);
            sarauI_ComboBox1.ListTextColor = Color.DimGray;
            sarauI_ComboBox1.Location = new Point(27, 22);
            sarauI_ComboBox1.MinimumSize = new Size(200, 30);
            sarauI_ComboBox1.Name = "sarauI_ComboBox1";
            sarauI_ComboBox1.Padding = new Padding(2);
            sarauI_ComboBox1.Size = new Size(258, 30);
            sarauI_ComboBox1.TabIndex = 0;
            sarauI_ComboBox1.Texts = "";
            // 
            // sarauI_ComboBox2
            // 
            sarauI_ComboBox2.BorderColor = Color.MediumSlateBlue;
            sarauI_ComboBox2.BorderFocusColor = Color.HotPink;
            sarauI_ComboBox2.BorderSize = 2;
            sarauI_ComboBox2.DropDownStyle = ComboBoxStyle.DropDown;
            sarauI_ComboBox2.Font = new Font("Segoe UI", 10F);
            sarauI_ComboBox2.ForeColor = Color.DimGray;
            sarauI_ComboBox2.IconColor = Color.MediumSlateBlue;
            sarauI_ComboBox2.ListBackColor = Color.FromArgb(230, 228, 245);
            sarauI_ComboBox2.ListTextColor = Color.DimGray;
            sarauI_ComboBox2.Location = new Point(124, 117);
            sarauI_ComboBox2.MinimumSize = new Size(200, 30);
            sarauI_ComboBox2.Name = "sarauI_ComboBox2";
            sarauI_ComboBox2.Padding = new Padding(2);
            sarauI_ComboBox2.Size = new Size(200, 30);
            sarauI_ComboBox2.TabIndex = 1;
            sarauI_ComboBox2.Texts = "";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1190, 725);
            Controls.Add(sarauI_GridPanel1);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Sara UI Design - Demostración";
            sarauI_GridPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sara_UI_Design.SaraControls.SaraUI_GridPanel sarauI_GridPanel1;
        private Panel panel2;
        private Panel panel1;
        private Sara_UI_Design.SaraControls.SaraUI_ComboBox sarauI_ComboBox1;
        private Sara_UI_Design.SaraControls.SaraUI_ComboBox sarauI_ComboBox2;
    }
}

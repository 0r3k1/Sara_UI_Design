namespace Sara_UI_Design.Demo {
    partial class MainForm {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null!;

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
            components = new System.ComponentModel.Container();
            controlTransitions = new Sara_UI_Design.Animations.SaraControlTransitions(components);
            windowTransitions = new Sara_UI_Design.Animations.SaraControlTransitions(components);
            sarauI_GridPanel1 = new Sara_UI_Design.SaraControls.SaraUI_GridPanel();
            panel2 = new Panel();
            panel1 = new Panel();
            sarauI_GridPanel1.SuspendLayout();
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
            panel1.Location = new Point(10, 10);
            panel1.Name = "panel1";
            panel1.Size = new Size(585, 705);
            panel1.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1190, 725);
            Controls.Add(sarauI_GridPanel1);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Sara UI Design - Demostración";
            sarauI_GridPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sara_UI_Design.SaraControls.SaraUI_GridPanel sarauI_GridPanel1;
        private Panel panel2;
        private Panel panel1;
        private Sara_UI_Design.Animations.SaraControlTransitions controlTransitions = null!;
        private Sara_UI_Design.Animations.SaraControlTransitions windowTransitions = null!;
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {

    /// <summary>
    /// Panel de navigation lateral con soporte para animaciones de expansión y colapso.
    /// </summary>
    [ToolboxItem(true)]
    public class SaraUI_SideBar:Panel {
        private int expandedWidth = 250;
        private int collapsedWidth = 60;
        private bool isExpanded = true;
        private int animationSpeed = 15;
        private System.Windows.Forms.Timer animationTimer;

        [Category("Sara UI Design")]
        public int ExpandedWidth { get => expandedWidth; set => expandedWidth = value; }

        [Category("Sara UI Design")]
        public int CollapsedWidth { get => collapsedWidth; set => collapsedWidth = value; }

        [Category("Sara UI Design")]
        public bool IsExpanded {
            get => isExpanded;
            set {
                isExpanded = value;
                if(!this.DesignMode)
                    animationTimer.Start();
                else
                    this.Width = value ? expandedWidth : collapsedWidth;
            }
        }

        public SaraUI_SideBar() {
            this.Width = expandedWidth;
            this.Dock = DockStyle.Left;
            this.BackColor = Color.FromArgb(45, 45, 65);
            this.DoubleBuffered = true;

            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 1;
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e) {
            if(isExpanded) {
                if(this.Width < expandedWidth) {
                    this.Width += animationSpeed;
                } else {
                    this.Width = expandedWidth;
                    animationTimer.Stop();
                    ToggleChildText(true);
                }
            } else {
                if(this.Width > collapsedWidth) {
                    this.Width -= animationSpeed;
                    ToggleChildText(false);
                } else {
                    this.Width = collapsedWidth;
                    animationTimer.Stop();
                }
            }
        }

        private void ToggleChildText(bool show) {
            foreach(Control ctrl in this.Controls) {
                if(ctrl is SaraUI_Button btn) {
                    if(!show && !string.IsNullOrEmpty(btn.Text)) {
                        // Guardamos de forma segura indicando que es el respaldo del texto de la suite
                        btn.Tag = "SaraUI_TextBackup:" + btn.Text;
                        btn.Text = "";
                    } else if(show && btn.Tag != null && btn.Tag.ToString().StartsWith("SaraUI_TextBackup:")) {
                        btn.Text = btn.Tag.ToString().Replace("SaraUI_TextBackup:", "");
                    }
                }
            }
        }
    }
}
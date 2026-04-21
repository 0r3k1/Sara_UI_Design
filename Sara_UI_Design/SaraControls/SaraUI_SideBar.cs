using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {
    public class SaraUI_SideBar:Panel {
        // Campos de configuración
        private int expandedWidth = 250;
        private int collapsedWidth = 60;
        private bool isExpanded = true;
        private int animationSpeed = 15;
        private System.Windows.Forms.Timer animationTimer;

        // Propiedades para el Diseñador
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
            this.BackColor = Color.FromArgb(45, 45, 65); // Color oscuro por defecto
            this.DoubleBuffered = true;

            // Configurar el Timer para la animación
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 1; // Máxima fluidez
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e) {
            if(isExpanded) {
                // Expandiendo
                if(this.Width < expandedWidth) {
                    this.Width += animationSpeed;
                } else {
                    this.Width = expandedWidth;
                    animationTimer.Stop();
                    ToggleChildText(true);
                }
            } else {
                // Colapsando
                if(this.Width > collapsedWidth) {
                    this.Width -= animationSpeed;
                    ToggleChildText(false); // Ocultar texto de inmediato al colapsar
                } else {
                    this.Width = collapsedWidth;
                    animationTimer.Stop();
                }
            }
        }

        // Método para ocultar/mostrar texto de los SaraUI_Buttons internos
        private void ToggleChildText(bool show) {
            foreach(Control ctrl in this.Controls) {
                if(ctrl is SaraUI_Button btn) {
                    // Si el botón está colapsado, guardamos el texto en el Tag para no perderlo
                    if(!show && !string.IsNullOrEmpty(btn.Text)) {
                        btn.Tag = btn.Text;
                        btn.Text = "";
                    } else if(show && btn.Tag != null) {
                        btn.Text = btn.Tag.ToString();
                    }
                }
            }
        }
    }
}
using System;
using System.Drawing;
using System.Windows.Forms;
namespace ProyectoFinalDB2
{
    public class PanelPrincipal : Form
    {
        public PanelPrincipal()
        {
            ConfigurarPanel();
        }

        private void ConfigurarPanel()
        {
            this.Text = "Inversiones Maya - Panel de Control";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(241, 242, 246); // Gris muy claro

            // Panel Superior (Header)
            Panel pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 80;
            pnlHeader.BackColor = Color.FromArgb(44, 62, 80); // Azul marino

            Label lblDashboard = new Label();
            lblDashboard.Text = "PANEL PRINCIPAL";
            lblDashboard.ForeColor = Color.White;
            lblDashboard.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblDashboard.Location = new Point(20, 20);
            lblDashboard.AutoSize = true;
            pnlHeader.Controls.Add(lblDashboard);

            // Crear los 3 botones principales
            this.Controls.Add(CrearBotonMenu("PROYECTOS", 100, Color.FromArgb(52, 152, 219))); // Azul
            this.Controls.Add(CrearBotonMenu("EMPLEADOS", 220, Color.FromArgb(155, 89, 182))); // Morado
            this.Controls.Add(CrearBotonMenu("FINANCIERO", 340, Color.FromArgb(230, 126, 34))); // Naranja

            this.Controls.Add(pnlHeader);
        }

        private Button CrearBotonMenu(string texto, int yPos, Color color)
        {
            Button btn = new Button();
            btn.Text = texto;
            btn.Size = new Size(300, 60);
            btn.Location = new Point(250, yPos);
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;

            btn.Click += (s, e) => {
                if (texto == "EMPLEADOS")
                {
                    SeccionEmpleados seccion = new SeccionEmpleados();
                    seccion.ShowDialog(); // Abrir la ventana de empleados
                }
                else
                {
                    MessageBox.Show("Abriendo sección de " + texto);
                }
            };

            return btn;
        }

        // Para que la app se cierre del todo al cerrar esta ventana
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Application.Exit();
        }
    }
}
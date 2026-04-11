using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;

namespace ProyectoFinalDB2
{
    public class SeccionProyectos : Form
    {
        private DataGridView dgvProyectos;
        private string connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString;

        // Paleta de colores consistente
        private static readonly Color ColorFondo = Color.FromArgb(247, 247, 245);
        private static readonly Color ColorPrimario = Color.FromArgb(26, 26, 46); // Azul Marino
        private static readonly Color ColorAccent = Color.FromArgb(52, 152, 219); // Azul brillante
        private static readonly Color ColorBorde = Color.FromArgb(220, 220, 215);

        public SeccionProyectos()
        {
            ConfigurarVista();
            CargarProyectos();
        }

        private void ConfigurarVista()
        {
            this.Text = "Portafolio de Proyectos — Inversiones Maya";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ColorFondo;
            this.Font = new Font("Segoe UI", 9);

            // --- HEADER ---
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.White
            };

            Label lblTitulo = new Label
            {
                Text = "Proyectos Inmobiliarios",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = ColorPrimario,
                Location = new Point(30, 25),
                AutoSize = true
            };

            Label lblSub = new Label
            {
                Text = "Gestión de lotificaciones y plazos de inversión",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(32, 60),
                AutoSize = true
            };

            Button btnNuevo = new Button
            {
                Text = "+ Nuevo Proyecto",
                Size = new Size(160, 40),
                Location = new Point(680, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorPrimario,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnNuevo.FlatAppearance.BorderSize = 0;
            btnNuevo.Click += (s, e) => AbrirFormularioProyecto();

            pnlHeader.Controls.AddRange(new Control[] { lblTitulo, lblSub, btnNuevo });

            // --- TABLA DE PROYECTOS ---
            dgvProyectos = new DataGridView
            {
                Location = new Point(30, 120),
                Size = new Size(820, 400),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                GridColor = ColorBorde,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowTemplate = { Height = 50 },
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            dgvProyectos.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = ColorPrimario,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Padding = new Padding(10, 0, 0, 0)
            };

            this.Controls.Add(dgvProyectos);
            this.Controls.Add(pnlHeader);
        }

        private void CargarProyectos()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = "SELECT ProyectoID as ID, Nombre, Departamento, Municipio, PlazoMaximo as [Plazo (Meses)] FROM Proyecto";
                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvProyectos.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar proyectos: " + ex.Message);
            }
        }

        private void AbrirFormularioProyecto()
        {
            // Formulario rápido (Pop-up) para insertar
            Form f = new Form
            {
                Text = "Registrar Proyecto",
                Size = new Size(400, 450),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                BackColor = Color.White
            };

            // Controles básicos (Labels y Textbox)
            Label l1 = new Label { Text = "Nombre del Proyecto:", Location = new Point(30, 20), AutoSize = true };
            TextBox txtNom = new TextBox { Location = new Point(30, 45), Width = 320 };

            Label l2 = new Label { Text = "Departamento:", Location = new Point(30, 90), AutoSize = true };
            TextBox txtDep = new TextBox { Location = new Point(30, 115), Width = 320 };

            Label l3 = new Label { Text = "Municipio:", Location = new Point(30, 160), AutoSize = true };
            TextBox txtMun = new TextBox { Location = new Point(30, 185), Width = 320 };

            Label l4 = new Label { Text = "Plazo Máximo (Meses):", Location = new Point(30, 230), AutoSize = true };
            NumericUpDown numPlazo = new NumericUpDown { Location = new Point(30, 255), Width = 320, Minimum = 1, Maximum = 360 };

            Button btnGuardar = new Button
            {
                Text = "GUARDAR PROYECTO",
                Location = new Point(30, 320),
                Size = new Size(320, 45),
                BackColor = ColorPrimario,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            btnGuardar.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtNom.Text)) { MessageBox.Show("El nombre es obligatorio"); return; }

                GuardarEnBD(txtNom.Text, txtDep.Text, txtMun.Text, (int)numPlazo.Value);
                f.DialogResult = DialogResult.OK;
            };

            f.Controls.AddRange(new Control[] { l1, txtNom, l2, txtDep, l3, txtMun, l4, numPlazo, btnGuardar });

            if (f.ShowDialog() == DialogResult.OK) CargarProyectos();
        }

        private void GuardarEnBD(string nom, string dep, string mun, int plazo)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = "INSERT INTO Proyecto (Nombre, Departamento, Municipio, PlazoMaximo) VALUES (@n, @d, @m, @p)";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@n", nom);
                    cmd.Parameters.AddWithValue("@d", dep);
                    cmd.Parameters.AddWithValue("@m", mun);
                    cmd.Parameters.AddWithValue("@p", plazo);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }
    }
}
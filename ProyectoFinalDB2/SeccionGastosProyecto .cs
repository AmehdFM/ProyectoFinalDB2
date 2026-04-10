using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;

namespace ProyectoFinalDB2
{
    public class SeccionGastosProyecto : Form
    {
        private DataGridView dgvGastos;
        private TextBox txtProyectoID;
        private string connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString;

        private static readonly Color ColorFondo = Color.FromArgb(247, 247, 245);
        private static readonly Color ColorPrimario = Color.FromArgb(26, 26, 46);
        private static readonly Color ColorAccent = Color.FromArgb(52, 152, 219);
        private static readonly Color ColorBorde = Color.FromArgb(220, 220, 215);

        public SeccionGastosProyecto()
        {
            ConfigurarVista();
        }

        private void ConfigurarVista()
        {
            this.Text = "Consulta de Gastos por Proyecto";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ColorFondo;
            this.Font = new Font("Segoe UI", 9);

            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.White
            };

            Label lblTitulo = new Label
            {
                Text = "Gastos por Proyecto",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = ColorPrimario,
                Location = new Point(30, 25),
                AutoSize = true
            };

            Label lblSub = new Label
            {
                Text = "Consulta de gastos registrados en la base de datos",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(32, 60),
                AutoSize = true
            };

            txtProyectoID = new TextBox
            {
                Location = new Point(32, 80),
                Width = 200
            };

            Button btnConsultar = new Button
            {
                Text = "CONSULTAR",
                Size = new Size(140, 35),
                Location = new Point(250, 75),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorPrimario,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnConsultar.FlatAppearance.BorderSize = 0;
            btnConsultar.Click += (s, e) => CargarGastos();

            pnlHeader.Controls.AddRange(new Control[] { lblTitulo, lblSub, txtProyectoID, btnConsultar });

            dgvGastos = new DataGridView
            {
                Location = new Point(30, 140),
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
                RowTemplate = { Height = 45 },
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            dgvGastos.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = ColorPrimario,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Padding = new Padding(10, 0, 0, 0)
            };

            this.Controls.Add(dgvGastos);
            this.Controls.Add(pnlHeader);
        }

        private void CargarGastos()
        {
            if (!int.TryParse(txtProyectoID.Text, out int proyectoId))
            {
                MessageBox.Show("Ingrese un ID válido");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = "SELECT * FROM dbo.fnGastosProyecto(@ProyectoID)";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ProyectoID", proyectoId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();

                    da.Fill(dt);
                    dgvGastos.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar gastos: " + ex.Message);
            }
        }
    }
}
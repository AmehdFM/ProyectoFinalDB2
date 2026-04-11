using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;

namespace ProyectoFinalDB2
{
    public class SeccionResumenVentas : Form
    {
        private DataGridView dgvResumen;
        private string connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString;

        private static readonly Color ColorFondo = Color.FromArgb(247, 247, 245);
        private static readonly Color ColorPrimario = Color.FromArgb(26, 26, 46);
        private static readonly Color ColorAccent = Color.FromArgb(52, 152, 219);
        private static readonly Color ColorBorde = Color.FromArgb(220, 220, 215);

        public SeccionResumenVentas()
        {
            ConfigurarVista();
            CargarResumen();
        }

        private void ConfigurarVista()
        {
            this.Text = "Resumen de Ventas — Empleados";
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
                Text = "Resumen de Ventas por Empleado",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = ColorPrimario,
                Location = new Point(30, 25),
                AutoSize = true
            };

            Label lblSub = new Label
            {
                Text = "Total de ventas realizadas y valor generado",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(32, 60),
                AutoSize = true
            };

            Button btnActualizar = new Button
            {
                Text = "ACTUALIZAR",
                Size = new Size(160, 40),
                Location = new Point(680, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorPrimario,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.Click += (s, e) => CargarResumen();

            pnlHeader.Controls.AddRange(new Control[] { lblTitulo, lblSub, btnActualizar });

            // --- TABLA ---
            dgvResumen = new DataGridView
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

            dgvResumen.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = ColorPrimario,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Padding = new Padding(10, 0, 0, 0)
            };

            this.Controls.Add(dgvResumen);
            this.Controls.Add(pnlHeader);
        }

        private void CargarResumen()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = "SELECT * FROM dbo.fnResumenVentasPorEmpleado()";

                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();

                    da.Fill(dt);
                    dgvResumen.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar resumen: " + ex.Message);
            }
        }
    }
}
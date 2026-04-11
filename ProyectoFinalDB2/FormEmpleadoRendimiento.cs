using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;

namespace ProyectoFinalDB2
{
    public class FormEmpleadoRendimiento : Form
    {
        private int empleadoId;
        private string empleadoNombre;
        private DataGridView dgvResumen;

        // Colores para el diseño
        private static readonly Color ColorFondo = Color.FromArgb(245, 246, 250);
        private static readonly Color ColorPrimario = Color.FromArgb(26, 26, 46);
        private static readonly Color ColorAccent = Color.FromArgb(52, 152, 219);

        public FormEmpleadoRendimiento(int id, string nombre)
        {
            this.empleadoId = id;
            this.empleadoNombre = nombre;
            ConfigurarVentana();
            ConfigurarVista();
            CargarDatosDesdeVista();
        }

        private void ConfigurarVentana()
        {
            this.Text = "Análisis de Rendimiento — " + empleadoNombre;
            this.Size = new Size(850, 450);
            this.BackColor = ColorFondo;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9);
        }

        private void ConfigurarVista()
        {
            // --- HEADER ---
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            Label lblTitulo = new Label
            {
                Text = "ESTADÍSTICAS DE VENTA",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = ColorPrimario,
                AutoSize = true,
                Location = new Point(25, 15)
            };

            Label lblSub = new Label
            {
                Text = $"Reporte detallado para: {empleadoNombre} (ID: {empleadoId})",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(27, 45)
            };
            pnlHeader.Controls.AddRange(new Control[] { lblTitulo, lblSub });

            // --- GRID DE RENDIMIENTO ---
            dgvResumen = new DataGridView
            {
                Location = new Point(25, 100),
                Size = new Size(785, 250),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 45 }
            };

            dgvResumen.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ColorPrimario,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            this.Controls.Add(dgvResumen);
            this.Controls.Add(pnlHeader);
        }

        private void CargarDatosDesdeVista()
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    // Consumimos la vista filtrando por el ID del empleado
                    string query = "sp_GetVentasPorEmpleado";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@EmpleadoID", empleadoId);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        dgvResumen.DataSource = dt;
                        FormatearColumnas();
                    }
                    else
                    {
                        MessageBox.Show("No se encontraron registros de ventas para este empleado.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la vista: " + ex.Message);
            }
        }

        private void FormatearColumnas()
        {
            if (dgvResumen.Columns.Contains("TotalVentas")) dgvResumen.Columns["TotalVentas"].HeaderText = "TOTAL VENTAS";
            if (dgvResumen.Columns.Contains("VentasContado")) dgvResumen.Columns["VentasContado"].HeaderText = "CONTADO";
            if (dgvResumen.Columns.Contains("VentasCredito")) dgvResumen.Columns["VentasCredito"].HeaderText = "CRÉDITO";
            if (dgvResumen.Columns.Contains("PrimeraVenta")) dgvResumen.Columns["PrimeraVenta"].HeaderText = "PRIMERA OP.";
            if (dgvResumen.Columns.Contains("UltimaVenta")) dgvResumen.Columns["UltimaVenta"].HeaderText = "ÚLTIMA OP.";

            // Alinear al centro
            foreach (DataGridViewColumn col in dgvResumen.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }
    }
}
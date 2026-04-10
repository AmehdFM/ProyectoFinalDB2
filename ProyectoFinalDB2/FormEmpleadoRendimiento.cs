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
        private DataGridView dgvVentas;

        public FormEmpleadoRendimiento(int id, string nombre)
        {
            this.empleadoId = id;
            this.empleadoNombre = nombre;
            this.Text = "Rendimiento - " + nombre;
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            ConfigurarVista();
            CargarVentas();
        }

        private void ConfigurarVista()
        {
            Label lbl = new Label
            {
                Text = "Empleado: " + empleadoNombre + " (ID: " + empleadoId + ")",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };

            dgvVentas = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(740, 460),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false
            };

            this.Controls.Add(lbl);
            this.Controls.Add(dgvVentas);
        }

        private void CargarVentas()
        {
            dgvVentas.Columns.Clear();
            dgvVentas.Rows.Clear();

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString;
                if (string.IsNullOrEmpty(connStr))
                {
                    MessageBox.Show("No se encontró la cadena de conexión. Mostrando datos de ejemplo.");
                    CargarVentasEjemplo();
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // Intentar tabla Venta
                    cmd.CommandText = "SELECT VentaID, Fecha, Monto FROM Venta WHERE EmpleadoID = @id";
                    cmd.Parameters.AddWithValue("@id", empleadoId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dgvVentas.Columns.Add("VentaID", "VentaID");
                        dgvVentas.Columns.Add("Fecha", "Fecha");
                        dgvVentas.Columns.Add("Monto", "Monto");

                        while (reader.Read())
                        {
                            var vid = reader["VentaID"]?.ToString() ?? string.Empty;
                            var fecha = reader["Fecha"]?.ToString() ?? string.Empty;
                            var monto = reader["Monto"]?.ToString() ?? string.Empty;
                            dgvVentas.Rows.Add(vid, fecha, monto);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando ventas: " + ex.Message + "\nMostrando datos de ejemplo.");
                CargarVentasEjemplo();
            }
        }

        private void CargarVentasEjemplo()
        {
            dgvVentas.Columns.Add("VentaID", "VentaID");
            dgvVentas.Columns.Add("Fecha", "Fecha");
            dgvVentas.Columns.Add("Monto", "Monto");

            dgvVentas.Rows.Add("1001", DateTime.Now.ToShortDateString(), "1500.00");
            dgvVentas.Rows.Add("1002", DateTime.Now.AddDays(-7).ToShortDateString(), "820.50");
        }
    }
}

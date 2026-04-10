using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ProyectoFinalDB2
{
    public class SeccionEmpleados : Form
    {
        private DataGridView dgvEmpleados;

        public SeccionEmpleados()
        {
            ConfigurarVista();
            CargarDatosDesdeBD();
        }

        private void ConfigurarVista()
        {
            this.Text = "Gestión de Empleados - Inversiones Maya";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Título
            Label lblTitulo = new Label
            {
                Text = "NÓMINA DE EMPLEADOS",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(20, 20),
                AutoSize = true
            };

            // Botón Agregar
            Button btnAgregar = new Button
            {
                Text = "+ Agregar Empleado",
                Location = new Point(20, 70),
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(0, 184, 148),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAgregar.Click += (s, e) => {
                FormEmpleado frm = new FormEmpleado("Nuevo Empleado");
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    // Insertar en DB
                    InsertEmpleado(frm.EmpleadoNombre, frm.EmpleadoCargo);
                    CargarDatosDesdeBD();
                }
            };

            // Botón Ver
            Button btnVer = new Button
            {
                Text = "👁 Ver Detalle",
                Location = new Point(190, 70),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnVer.Click += (s, e) => {
                if (dgvEmpleados.SelectedRows.Count > 0)
                {
                    var row = dgvEmpleados.SelectedRows[0];
                    int.TryParse(row.Cells[0].Value?.ToString(), out int id);
                    string nombre = row.Cells[1].Value?.ToString();
                    string cargo = row.Cells[2].Value?.ToString();

                    FormEmpleado frm = new FormEmpleado(id, nombre, cargo);
                    frm.Text = "Ficha de Empleado (Lectura)";
                    frm.SetReadOnly(true);
                    frm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Por favor, selecciona un empleado de la tabla.");
                }
            };

            // Botón Editar
            Button btnEditar = new Button
            {
                Text = "✏️ Editar",
                Location = new Point(330, 70),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEditar.Click += (s, e) => {
                if (dgvEmpleados.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Selecciona un empleado para editar.");
                    return;
                }

                var row = dgvEmpleados.SelectedRows[0];
                if (!int.TryParse(row.Cells[0].Value?.ToString(), out int id))
                {
                    MessageBox.Show("ID de empleado inválido.");
                    return;
                }

                string nombre = row.Cells[1].Value?.ToString();
                string cargo = row.Cells[2].Value?.ToString();

                FormEmpleado frm = new FormEmpleado(id, nombre, cargo);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    UpdateEmpleado(id, frm.EmpleadoNombre, frm.EmpleadoCargo);
                    CargarDatosDesdeBD();
                }
            };

            // Botón Rendimiento
            Button btnRendimiento = new Button
            {
                Text = "📈 Rendimiento",
                Location = new Point(460, 70),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRendimiento.Click += (s, e) => {
                if (dgvEmpleados.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Selecciona un empleado para ver su rendimiento.");
                    return;
                }

                var row = dgvEmpleados.SelectedRows[0];
                if (!int.TryParse(row.Cells[0].Value?.ToString(), out int id))
                {
                    MessageBox.Show("ID de empleado inválido.");
                    return;
                }
                string nombre = row.Cells[1].Value?.ToString();

                FormEmpleadoRendimiento frm = new FormEmpleadoRendimiento(id, nombre);
                frm.ShowDialog();
            };

            // DataGridView (La Tabla)
            dgvEmpleados = new DataGridView
            {
                Location = new Point(20, 130),
                Size = new Size(840, 400),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true
            };

            this.Controls.Add(lblTitulo);
            this.Controls.Add(btnAgregar);
            this.Controls.Add(btnVer);
            this.Controls.Add(btnEditar);
            this.Controls.Add(btnRendimiento);
            this.Controls.Add(dgvEmpleados);
        }

        private void InsertEmpleado(string nombre, string cargo)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString;
                if (string.IsNullOrEmpty(connStr)) throw new Exception("Cadena de conexión no encontrada.");

                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Empleado (Nombre, Cargo) VALUES (@nombre, @cargo)";
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@cargo", cargo);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error insertando empleado: " + ex.Message);
            }
        }

        private void UpdateEmpleado(int id, string nombre, string cargo)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString;
                if (string.IsNullOrEmpty(connStr)) throw new Exception("Cadena de conexión no encontrada.");

                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Empleado SET Nombre = @nombre, Cargo = @cargo WHERE EmpleadoID = @id";
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@cargo", cargo);
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error actualizando empleado: " + ex.Message);
            }
        }

        private void CargarDatosEjemplo()
        {
            dgvEmpleados.Columns.Add("ID", "ID");
            dgvEmpleados.Columns.Add("Nombre", "Nombre Completo");
            dgvEmpleados.Columns.Add("Puesto", "Puesto/Cargo");

            // Datos de prueba para que no se vea vacío
            dgvEmpleados.Rows.Add("1", "Amehd Mendez", "Desarrollador Senior");
            dgvEmpleados.Rows.Add("2", "Maria Lopez", "Gerente de Ventas");
        }

        private void CargarDatosDesdeBD()
        {
            // Limpiar definición previa
            dgvEmpleados.Columns.Clear();
            dgvEmpleados.Rows.Clear();

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString;
                if (string.IsNullOrEmpty(connStr))
                {
                    MessageBox.Show("No se encontró la cadena de conexión 'ConexionInversiones' en App.config. Usando datos de ejemplo.");
                    CargarDatosEjemplo();
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT EmpleadoID, Nombre, Cargo FROM Empleado";
                    cmd.CommandType = CommandType.Text;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Crear columnas según columnas esperadas
                        dgvEmpleados.Columns.Add("ID", "ID");
                        dgvEmpleados.Columns.Add("Nombre", "Nombre Completo");
                        dgvEmpleados.Columns.Add("Puesto", "Puesto/Cargo");

                        while (reader.Read())
                        {
                            var id = reader["EmpleadoID"]?.ToString() ?? string.Empty;
                            var nombre = reader["Nombre"]?.ToString() ?? string.Empty;
                            var cargo = reader["Cargo"]?.ToString() ?? string.Empty;

                            dgvEmpleados.Rows.Add(id, nombre, cargo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Si falla la conexión, mostrar error y usar datos de ejemplo
                MessageBox.Show("Error cargando empleados desde la base de datos: " + ex.Message + "\nSe cargarán datos de ejemplo.");
                CargarDatosEjemplo();
            }
        }
    }
}
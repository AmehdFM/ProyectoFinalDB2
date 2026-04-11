using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ProyectoFinalDB2
{
    public class SeccionEmpleados : Form
    {
        private static readonly Color CPrimario   = Color.FromArgb(15, 23, 42);
        private static readonly Color CAccent     = Color.FromArgb(67, 97, 238);
        private static readonly Color CFondo      = Color.FromArgb(241, 245, 249);
        private static readonly Color CCard       = Color.White;
        private static readonly Color CTexto      = Color.FromArgb(15, 23, 42);
        private static readonly Color CSub        = Color.FromArgb(100, 116, 139);
        private static readonly Color CBorder     = Color.FromArgb(226, 232, 240);
        private static readonly Color CHeader     = Color.FromArgb(248, 250, 252);
        private static readonly Color CSeleccion  = Color.FromArgb(235, 241, 255);

        private DataGridView dgvEmpleados;
        private Label lblConteo;

        public SeccionEmpleados()
        {
            this.Text = "Gestión de Recursos Humanos";
            this.Size = new Size(1000, 700);
            this.BackColor = CFondo;
            this.Font = new Font("Segoe UI", 9);

            BuildUI();
            CargarDatosDesdeBD();
        }

        private void BuildUI()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = CCard };
            pnlTop.Paint += (s, e) => e.Graphics.DrawLine(new Pen(CBorder), 0, 79, pnlTop.Width, 79);

            var lblBread = new Label { Text = "Sistema  ›  Recursos Humanos", Font = new Font("Segoe UI", 7.5f), ForeColor = CSub, Location = new Point(28, 16), AutoSize = true };
            var lblTitulo = new Label { Text = "Nómina de Personal", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = CTexto, Location = new Point(26, 34), AutoSize = true };

            var btnAgregar = new Button { 
                Text = "+ Nuevo Empleado", 
                Size = new Size(160, 36), 
                FlatStyle = FlatStyle.Flat,
                BackColor = CAccent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnAgregar.Location = new Point(pnlTop.Width - btnAgregar.Width - 28, 22);

            btnAgregar.FlatAppearance.BorderSize = 0;
            btnAgregar.Click += (s, e) => {
                var frm = new FormEmpleado("Registro de Nuevo Empleado");
                if (frm.ShowDialog() == DialogResult.OK) {
                    InsertEmpleado(frm.EmpleadoNombre, frm.EmpleadoCargo);
                    CargarDatosDesdeBD();
                }
            };

            pnlTop.Controls.AddRange(new Control[] { lblBread, lblTitulo, btnAgregar });

            dgvEmpleados = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = CCard,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                GridColor = CBorder,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                RowTemplate = { Height = 50 }
            };

            dgvEmpleados.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = CHeader, ForeColor = CSub, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), Padding = new Padding(14, 0, 0, 0), SelectionBackColor = CHeader, SelectionForeColor = CSub };
            dgvEmpleados.DefaultCellStyle = new DataGridViewCellStyle { BackColor = CCard, ForeColor = CTexto, SelectionBackColor = CSeleccion, SelectionForeColor = CTexto, Padding = new Padding(14, 0, 0, 0) };
            
            dgvEmpleados.CellContentClick += DgvEmpleados_CellContentClick;

            var pnlContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 20, 24, 24) };
            var wrapper = new Panel { Dock = DockStyle.Fill, BackColor = CCard, BorderStyle = BorderStyle.FixedSingle };
            wrapper.Controls.Add(dgvEmpleados);
            pnlContainer.Controls.Add(wrapper);

            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = CHeader };
            pnlFooter.Paint += (s, e) => e.Graphics.DrawLine(new Pen(CBorder), 0, 0, pnlFooter.Width, 0);
            lblConteo = new Label { Text = "Cargando...", Font = new Font("Segoe UI", 8), ForeColor = CSub, Location = new Point(20, 12), AutoSize = true };
            pnlFooter.Controls.Add(lblConteo);

            // IMPORTANTE: El orden de Add afecta al DockStyle.Fill
            this.Controls.Add(pnlContainer); // El Fill se agrega PRIMERO o DESPUÉS de los anclados
            this.Controls.Add(pnlTop);
            this.Controls.Add(pnlFooter);
            
            // Forzar layout para que el botón se mueva a su ancla real
            pnlTop.Resize += (s, e) => {
                btnAgregar.Location = new Point(pnlTop.Width - btnAgregar.Width - 28, 22);
            };
        }

        private void AgregarColumnas()
        {
            dgvEmpleados.Columns.Clear();
            dgvEmpleados.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nombre", HeaderText = "EMPLEADO", FillWeight = 130 });
            dgvEmpleados.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cargo", HeaderText = "CARGO / DEPARTAMENTO", FillWeight = 100 });

            var colEditar = new DataGridViewButtonColumn { Name = "BtnEditar", HeaderText = "", Text = "Editar", UseColumnTextForButtonValue = true, Width = 80, FlatStyle = FlatStyle.Flat };
            colEditar.DefaultCellStyle = new DataGridViewCellStyle { BackColor = CHeader, ForeColor = CPrimario, SelectionBackColor = CHeader, SelectionForeColor = CPrimario, Font = new Font("Segoe UI", 8, FontStyle.Bold), Alignment = DataGridViewContentAlignment.MiddleCenter };
            dgvEmpleados.Columns.Add(colEditar);

            var colRend = new DataGridViewButtonColumn { Name = "BtnRendimiento", HeaderText = "", Text = "📊 Rendimiento", UseColumnTextForButtonValue = true, Width = 130, FlatStyle = FlatStyle.Flat };
            colRend.DefaultCellStyle = new DataGridViewCellStyle { BackColor = CSeleccion, ForeColor = CAccent, SelectionBackColor = CSeleccion, SelectionForeColor = CAccent, Font = new Font("Segoe UI", 8, FontStyle.Bold), Alignment = DataGridViewContentAlignment.MiddleCenter };
            dgvEmpleados.Columns.Add(colRend);
        }

        private void DgvEmpleados_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvEmpleados.Rows[e.RowIndex];
            if (!(row.Tag is int id)) return;
            string nombre = row.Cells["Nombre"].Value?.ToString();

            if (dgvEmpleados.Columns[e.ColumnIndex].Name == "BtnEditar") {
                string cargo = row.Cells["Cargo"].Value?.ToString();
                var frm = new FormEmpleado(id, nombre, cargo);
                if (frm.ShowDialog() == DialogResult.OK) {
                    UpdateEmpleado(id, frm.EmpleadoNombre, frm.EmpleadoCargo);
                    CargarDatosDesdeBD();
                }
            }
            else if (dgvEmpleados.Columns[e.ColumnIndex].Name == "BtnRendimiento") {
                new FormEmpleadoRendimiento(id, nombre).ShowDialog();
            }
        }

        private void CargarDatosDesdeBD()
        {
            AgregarColumnas();
            try {
                string connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString;
                if (string.IsNullOrEmpty(connStr)) { CargarDatosEjemplo(); return; }

                int count = 0;
                using (SqlConnection conn = new SqlConnection(connStr)) {
                    SqlCommand cmd = new SqlCommand("SELECT EmpleadoID, Nombre, Cargo FROM Empleado ORDER BY Nombre", conn);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            int idx = dgvEmpleados.Rows.Add(reader["Nombre"], reader["Cargo"]);
                            dgvEmpleados.Rows[idx].Tag = Convert.ToInt32(reader["EmpleadoID"]);
                            count++;
                        }
                    }
                }
                lblConteo.Text = $"{count} empleado{(count != 1 ? "s" : "")} registrados en la nómina activa.";
            } catch { CargarDatosEjemplo(); }
        }

        private void CargarDatosEjemplo()
        {
            int i1 = dgvEmpleados.Rows.Add("AMEHD MÉNDEZ", "DESARROLLADOR SENIOR");
            dgvEmpleados.Rows[i1].Tag = 1;
            int i2 = dgvEmpleados.Rows.Add("MARÍA F. LÓPEZ", "GERENTE DE VENTAS");
            dgvEmpleados.Rows[i2].Tag = 2;
            lblConteo.Text = "Modo demostración (base de datos no disponible)";
        }

        private void InsertEmpleado(string nombre, string cargo) {
            EjecutarComando("sp_InsertarEmpleado", cmd => {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nombre", nombre); 
                cmd.Parameters.AddWithValue("@Cargo", cargo);
            });
        }

        private void UpdateEmpleado(int id, string nombre, string cargo) {
            EjecutarComando("sp_ActualizarEmpleado", cmd => {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmpleadoID", id);
                cmd.Parameters.AddWithValue("@Nombre", nombre); 
                cmd.Parameters.AddWithValue("@Cargo", cargo);
            });
        }

        private void EjecutarComando(string sql, Action<SqlCommand> p) {
            try {
                string cs = ConfigurationManager.ConnectionStrings["ConexionInversiones"].ConnectionString;
                using (SqlConnection c = new SqlConnection(cs)) {
                    SqlCommand cmd = new SqlCommand(sql, c); p(cmd); c.Open(); cmd.ExecuteNonQuery();
                }
            } catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
    }
}
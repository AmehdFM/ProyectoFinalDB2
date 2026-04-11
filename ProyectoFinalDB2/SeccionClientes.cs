using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    public class SeccionClientes : Form
    {
        private static readonly Color CHeader     = Color.FromArgb(248, 250, 252);
        private static readonly Color CSeleccion  = Color.FromArgb(235, 241, 255);
        private static readonly Color CAccent     = Color.FromArgb(67, 97, 238);
        private static readonly Color CFondo      = Color.FromArgb(241, 245, 249);
        private static readonly Color CCard       = Color.White;
        private static readonly Color CTexto      = Color.FromArgb(15, 23, 42);
        private static readonly Color CSub        = Color.FromArgb(100, 116, 139);
        private static readonly Color CBorder     = Color.FromArgb(226, 232, 240);

        private readonly string connStr;
        private DataGridView dgvClientes;
        private Label lblConteo;

        public SeccionClientes() : this(ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString ?? "") { }

        public SeccionClientes(string connStr)
        {
            this.connStr = connStr;
            BackColor = CFondo;
            Font = new Font("Segoe UI", 9);
            Build();
            CargarClientes();
        }

        private void Build()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = CCard };
            pnlTop.Paint += (s, e) => e.Graphics.DrawLine(new Pen(CBorder), 0, 67, pnlTop.Width, 67);

            var lblBread = new Label { Text = "Sistema  ›  Clientes", Font = new Font("Segoe UI", 7.5f), ForeColor = CSub, Location = new Point(28, 14), AutoSize = true };
            var lblTitulo = new Label { Text = "Directorio de Clientes", Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = CTexto, Location = new Point(26, 30), AutoSize = true };

            pnlTop.Controls.Add(lblBread);
            pnlTop.Controls.Add(lblTitulo);

            dgvClientes = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = CCard, BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false, ReadOnly = true, RowHeadersVisible = false,
                GridColor = CBorder, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None, Font = new Font("Segoe UI", 9),
                RowTemplate = { Height = 46 }
            };

            dgvClientes.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = CHeader, ForeColor = CSub, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), Padding = new Padding(14, 0, 0, 0), SelectionBackColor = CHeader, SelectionForeColor = CSub };
            dgvClientes.DefaultCellStyle = new DataGridViewCellStyle { BackColor = CCard, ForeColor = CTexto, SelectionBackColor = CSeleccion, SelectionForeColor = CTexto, Padding = new Padding(14, 0, 0, 0) };
            
            dgvClientes.CellContentClick += OnCellClick;

            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 36, BackColor = CHeader };
            pnlFooter.Paint += (s, e) => e.Graphics.DrawLine(new Pen(CBorder), 0, 0, pnlFooter.Width, 0);
            lblConteo = new Label { Text = "", Font = new Font("Segoe UI", 8), ForeColor = CSub, Location = new Point(20, 10), AutoSize = true };
            pnlFooter.Controls.Add(lblConteo);

            Controls.Add(dgvClientes);
            Controls.Add(pnlFooter);
            Controls.Add(pnlTop);
        }

        private void CargarClientes()
        {
            dgvClientes.Columns.Clear();
            dgvClientes.Columns.Add("Nombre", "NOMBRE DEL CLIENTE");
            dgvClientes.Columns.Add("DNI", "IDENTIDAD (DNI)");
            dgvClientes.Columns.Add("Telefono", "TELÉFONO");

            var btn = new DataGridViewButtonColumn { Name = "BtnVer", HeaderText = "", Text = "Ver Historial →", UseColumnTextForButtonValue = true, Width = 110, FlatStyle = FlatStyle.Flat };
            btn.DefaultCellStyle = new DataGridViewCellStyle { BackColor = CSeleccion, ForeColor = CAccent, SelectionBackColor = CSeleccion, SelectionForeColor = CAccent, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Alignment = DataGridViewContentAlignment.MiddleCenter };
            dgvClientes.Columns.Add(btn);

            dgvClientes.Columns["Nombre"].FillWeight = 40;
            dgvClientes.Columns["DNI"].FillWeight = 30;
            dgvClientes.Columns["Telefono"].FillWeight = 30;

            try
            {
                int count = 0;
                using (var conn = new SqlConnection(connStr))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT ClienteID, Nombre, DNI, Telefono FROM Cliente ORDER BY Nombre";
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int i = dgvClientes.Rows.Add(r["Nombre"], r["DNI"], r["Telefono"]);
                            dgvClientes.Rows[i].Tag = Convert.ToInt32(r["ClienteID"]);
                            count++;
                        }
                    }
                }
                lblConteo.Text = $"{count} clientes registrados.";
            } catch (Exception ex) { lblConteo.Text = "Error: " + ex.Message; }
        }

        private void OnCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvClientes.Columns[e.ColumnIndex].Name != "BtnVer") return;
            var r = dgvClientes.Rows[e.RowIndex];
            int clienteId = (int)r.Tag;
            string nombre = r.Cells["Nombre"].Value?.ToString();

            new FormDetalleCliente(clienteId, nombre, connStr).ShowDialog();
        }
    }
}

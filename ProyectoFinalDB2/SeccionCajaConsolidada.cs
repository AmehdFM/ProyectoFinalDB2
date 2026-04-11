using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ProyectoFinalDB2
{
    public class SeccionCajaConsolidada : Form
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

        private DataGridView dgvCaja;
        private Label lblConteo;

        public SeccionCajaConsolidada()
        {
            this.Text = "Caja Consolidada";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = CFondo;
            this.Font = new Font("Segoe UI", 9);

            BuildUI();
            CargarDatosDesdeBD();
        }

        private void BuildUI()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = CCard };
            pnlTop.Paint += (s, e) => e.Graphics.DrawLine(new Pen(CBorder), 0, 79, pnlTop.Width, 79);

            var lblBread = new Label { Text = "Financiero  ›  Caja Diaria", Font = new Font("Segoe UI", 7.5f), ForeColor = CSub, Location = new Point(28, 16), AutoSize = true };
            var lblTitulo = new Label { Text = "Consolidación de Pagos Diarios", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = CTexto, Location = new Point(26, 34), AutoSize = true };

            var btnActualizar = new Button { 
                Text = "🔄 Actualizar", 
                Size = new Size(140, 36), 
                Location = new Point(this.ClientSize.Width - 168, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = CAccent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.Click += (s, e) => CargarDatosDesdeBD();

            pnlTop.Controls.AddRange(new Control[] { lblBread, lblTitulo, btnActualizar });

            dgvCaja = new DataGridView
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

            dgvCaja.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = CHeader, ForeColor = CSub, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), Padding = new Padding(14, 0, 0, 0), SelectionBackColor = CHeader, SelectionForeColor = CSub };
            dgvCaja.DefaultCellStyle = new DataGridViewCellStyle { BackColor = CCard, ForeColor = CTexto, SelectionBackColor = CSeleccion, SelectionForeColor = CTexto, Padding = new Padding(14, 0, 0, 0) };
            
            dgvCaja.CellFormatting += (s, e) => {
                if (e.ColumnIndex == 2 && e.Value != null) {
                    if (double.TryParse(e.Value.ToString(), out double val)) {
                        e.Value = "L. " + val.ToString("N2");
                        e.CellStyle.Font = new Font(dgvCaja.Font, FontStyle.Bold);
                        e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                        e.FormattingApplied = true;
                    }
                }
            };

            var pnlContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 20, 24, 24) };
            var wrapper = new Panel { Dock = DockStyle.Fill, BackColor = CCard, BorderStyle = BorderStyle.FixedSingle };
            wrapper.Controls.Add(dgvCaja);
            pnlContainer.Controls.Add(wrapper);

            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = CHeader };
            pnlFooter.Paint += (s, e) => e.Graphics.DrawLine(new Pen(CBorder), 0, 0, pnlFooter.Width, 0);
            lblConteo = new Label { Text = "Cargando...", Font = new Font("Segoe UI", 8), ForeColor = CSub, Location = new Point(20, 12), AutoSize = true };
            pnlFooter.Controls.Add(lblConteo);

            this.Controls.Add(pnlContainer);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlTop);
        }

        private void CargarDatosDesdeBD()
        {
            dgvCaja.Columns.Clear();
            dgvCaja.Columns.Add("Banco", "BANCO");
            dgvCaja.Columns.Add("NumeroCuenta", "NÚMERO DE CUENTA");
            dgvCaja.Columns.Add("MontoTotal", "TOTAL RECAUDADO HOY");

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString;
                if (string.IsNullOrEmpty(connStr)) { CargarEjemplo(); return; }

                int count = 0;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    SqlCommand cmd = new SqlCommand("sp_ConsolidarPagosDiariosCaja", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dgvCaja.Rows.Add(reader["Banco"], reader["NumeroCuenta"], reader["MontoTotal"]);
                            count++;
                        }
                    }
                }
                lblConteo.Text = $"{count} cuentas bancarias consolidadas satisfactoriamente.";
            } catch { CargarEjemplo(); }
        }

        private void CargarEjemplo()
        {
            dgvCaja.Rows.Add("BANCO ATLÁNTIDA", "1214-4567-1123", 45600.00);
            dgvCaja.Rows.Add("BAC CREDOMATIC", "7890-1111-2345", 12500.50);
            dgvCaja.Rows.Add("BANPAÍS", "5567-9900-2211", 8940.00);
            lblConteo.Text = "Modo demostración (sin conexión a base de datos)";
        }
    }
}
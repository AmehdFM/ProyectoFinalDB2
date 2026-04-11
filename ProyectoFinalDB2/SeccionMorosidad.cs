using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;

namespace ProyectoFinalDB2
{
    public class SeccionMorosidad : Form
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
        private static readonly Color CNegativo   = Color.FromArgb(239, 68, 68);
        private static readonly Color CPositivo   = Color.FromArgb(16, 185, 129);

        private DataGridView dgvMorosos;
        private Label lblConteo;

        public SeccionMorosidad()
        {
            this.Text = "Reporte de Morosidad";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = CFondo;
            this.Font = new Font("Segoe UI", 9);

            BuildUI();
            CargarDatosDesdeBD();
        }

        private void BuildUI()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = CCard };
            pnlTop.Paint += (s, e) => e.Graphics.DrawLine(new Pen(CBorder), 0, 89, pnlTop.Width, 89);

            var lblBread = new Label { Text = "Financiero  ›  Clientes en Mora", Font = new Font("Segoe UI", 7.5f), ForeColor = CSub, Location = new Point(28, 18), AutoSize = true };
            var lblTitulo = new Label { Text = "Reporte de Morosidad Global", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = CTexto, Location = new Point(26, 38), AutoSize = true };

            var btnProcesar = new Button { 
                Text = "⚙️ Recalcular Mora", 
                Size = new Size(160, 38), 
                Location = new Point(this.ClientSize.Width - 360, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = CHeader,
                ForeColor = CTexto,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnProcesar.FlatAppearance.BorderColor = CBorder;
            btnProcesar.Click += (s, e) => EjecutarSP();

            var btnActualizar = new Button { 
                Text = "🔄 Actualizar Vista", 
                Size = new Size(160, 38), 
                Location = new Point(this.ClientSize.Width - 188, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = CAccent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.Click += (s, e) => CargarDatosDesdeBD();

            pnlTop.Controls.AddRange(new Control[] { lblBread, lblTitulo, btnProcesar, btnActualizar });

            dgvMorosos = new DataGridView
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
                RowTemplate = { Height = 52 }
            };

            dgvMorosos.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = CHeader, ForeColor = CSub, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), Padding = new Padding(14, 0, 0, 0), SelectionBackColor = CHeader, SelectionForeColor = CSub };
            dgvMorosos.DefaultCellStyle = new DataGridViewCellStyle { BackColor = CCard, ForeColor = CTexto, SelectionBackColor = CSeleccion, SelectionForeColor = CTexto, Padding = new Padding(14, 0, 0, 0) };
            
            dgvMorosos.CellFormatting += (s, e) => {
                if (e.RowIndex < 0 || e.Value == null) return;
                string col = dgvMorosos.Columns[e.ColumnIndex].Name;
                
                if (col == "Saldo") {
                    if (double.TryParse(e.Value.ToString(), out double val)) {
                        e.Value = "L. " + val.ToString("N2");
                        e.FormattingApplied = true;
                        if (val > 0) e.CellStyle.ForeColor = CNegativo;
                    }
                }
                else if (col == "Estado") {
                    string est = e.Value.ToString();
                    e.CellStyle.Font = new Font(dgvMorosos.Font, FontStyle.Bold);
                    e.CellStyle.ForeColor = (est == "MOROSO") ? CNegativo : CPositivo;
                }
            };

            var pnlContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 20, 24, 24) };
            var wrapper = new Panel { Dock = DockStyle.Fill, BackColor = CCard, BorderStyle = BorderStyle.FixedSingle };
            wrapper.Controls.Add(dgvMorosos);
            pnlContainer.Controls.Add(wrapper);

            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = CHeader };
            pnlFooter.Paint += (s, e) => e.Graphics.DrawLine(new Pen(CBorder), 0, 0, pnlFooter.Width, 0);
            lblConteo = new Label { Text = "Listo", Font = new Font("Segoe UI", 8), ForeColor = CSub, Location = new Point(20, 12), AutoSize = true };
            pnlFooter.Controls.Add(lblConteo);

            this.Controls.Add(pnlContainer);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlTop);
        }

        private void CargarDatosDesdeBD()
        {
            dgvMorosos.Columns.Clear();
            dgvMorosos.Columns.Add("Nombre", "CLIENTE / PROPIETARIO");
            dgvMorosos.Columns.Add("Saldo", "SALDO TOTAL VENCIDO");
            dgvMorosos.Columns.Add("Estado", "ESTADO");

            dgvMorosos.Columns["Nombre"].FillWeight = 55;
            dgvMorosos.Columns["Saldo"].FillWeight = 25;
            dgvMorosos.Columns["Estado"].FillWeight = 20;

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString;
                if (string.IsNullOrEmpty(connStr)) { CargarEjemplo(); return; }

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    SqlCommand cmd = new SqlCommand(@"
                        SELECT c.ClienteID, c.Nombre,
                               dbo.fnSaldoPendiente(c.ClienteID) AS Saldo,
                               CASE WHEN dbo.fnEsClienteMoroso(c.ClienteID) = 1 THEN 'MOROSO' ELSE 'AL DÍA' END AS Estado
                        FROM Cliente c ORDER BY Saldo DESC", conn);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    int count = 0;
                    while (reader.Read())
                    {
                        dgvMorosos.Rows.Add(reader["Nombre"], reader["Saldo"], reader["Estado"]);
                        count++;
                    }
                    lblConteo.Text = $"{count} clientes analizados en tiempo real.";
                }
            } catch { CargarEjemplo(); }
        }

        private void EjecutarSP()
        {
            try {
                string connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr)) {
                    SqlCommand cmd = new SqlCommand("sp_GenerarReporteMorosidadMasiva", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Motor de mora actualizado exitosamente.", "Sincronización", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarDatosDesdeBD();
                }
            } catch (Exception ex) { MessageBox.Show("Error al sincronizar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void CargarEjemplo()
        {
            dgvMorosos.Rows.Add("CARLOS MARTÍNEZ RIVERA", 24500.00, "MOROSO");
            dgvMorosos.Rows.Add("MARÍA FERNANDA LÓPEZ", 0.00, "AL DÍA");
            dgvMorosos.Rows.Add("ROBERTO SOSA", 8940.50, "MOROSO");
            lblConteo.Text = "Modo demostración (sin conexión a base de datos)";
        }
    }
}
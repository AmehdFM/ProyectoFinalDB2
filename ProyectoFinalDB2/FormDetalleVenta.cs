using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    public class FormDetalleVenta : Form
    {
        private static readonly Color CPrimario   = Color.FromArgb(15, 23, 42);
        private static readonly Color CAccent     = Color.FromArgb(67, 97, 238);
        private static readonly Color CNegativo   = Color.FromArgb(239, 68, 68);
        private static readonly Color CFondo      = Color.FromArgb(241, 245, 249);
        private static readonly Color CCard       = Color.White;
        private static readonly Color CTexto      = Color.FromArgb(15, 23, 42);
        private static readonly Color CSub        = Color.FromArgb(100, 116, 139);
        private static readonly Color CBorder     = Color.FromArgb(226, 232, 240);
        private static readonly Color CHeader     = Color.FromArgb(248, 250, 252);
        private static readonly Color CSeleccion  = Color.FromArgb(235, 241, 255);

        private readonly int ventaId;
        private readonly string connStr;
        
        private Panel pnlSidebar;
        private DataGridView dgvPlan;

        public FormDetalleVenta(int vID, string cStr)
        {
            ventaId = vID;
            connStr = cStr;
            Text = $"Expediente de Venta #{vID}";
            WindowState = FormWindowState.Maximized;
            BackColor = CFondo;
            Font = new Font("Segoe UI", 9);
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            // SIDEBAR (Información de Personas)
            pnlSidebar = new Panel { Dock = DockStyle.Left, Width = 280, BackColor = CPrimario, AutoScroll = true };
            var brand = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Color.FromArgb(10, 16, 30) };
            brand.Controls.Add(new Label { Text = "📁 EXPEDIENTE VENTA", ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 8, FontStyle.Bold), Dock = DockStyle.Top, TextAlign = ContentAlignment.BottomCenter, Height = 36 });
            brand.Controls.Add(new Label { Text = "# " + ventaId.ToString("D5"), ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Bottom, TextAlign = ContentAlignment.TopCenter, Height = 36 });
            pnlSidebar.Controls.Add(brand);

            // MAIN AREA
            var mainArea = new Panel { Dock = DockStyle.Fill };
            
            var header = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = CCard };
            header.Paint += (s, e) => e.Graphics.DrawLine(new Pen(CBorder), 0, 67, header.Width, 67);

            var lblTitulo = new Label { Text = "Plan de Pagos y Estructura de Financiación", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = CTexto, Location = new Point(26, 20), AutoSize = true };
            header.Controls.Add(lblTitulo);



            var pnlContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 20, 24, 24) };
            
            dgvPlan = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = CCard, BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false,
                ReadOnly = true, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                GridColor = CBorder, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None, Font = new Font("Segoe UI", 9),
                RowTemplate = { Height = 46 }
            };
            dgvPlan.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = CHeader, ForeColor = CSub, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), Padding = new Padding(14, 0, 0, 0), SelectionBackColor = CHeader, SelectionForeColor = CSub };
            dgvPlan.DefaultCellStyle = new DataGridViewCellStyle { BackColor = CCard, ForeColor = CTexto, SelectionBackColor = CSeleccion, SelectionForeColor = CTexto, Padding = new Padding(14, 0, 0, 0) };

            dgvPlan.CellFormatting += (s, e) => {
                if (e.Value == null) return;
                string col = dgvPlan.Columns[e.ColumnIndex].Name;
                if (col == "Estado") { e.CellStyle.ForeColor = e.Value.ToString() == "Pagado" ? Color.FromArgb(16, 185, 129) : Color.FromArgb(245, 158, 11); e.CellStyle.Font = new Font(dgvPlan.Font, FontStyle.Bold); }
                else if (col.Contains("Monto") || col.Contains("Capital") || col.Contains("Interes")) {
                    if (double.TryParse(e.Value.ToString(), out double val)) { e.Value = "L. " + val.ToString("N2"); e.FormattingApplied = true; }
                }
            };

            pnlContainer.Controls.Add(dgvPlan);
            mainArea.Controls.Add(pnlContainer);
            mainArea.Controls.Add(header);

            Controls.Add(mainArea);
            Controls.Add(pnlSidebar);
        }

        private void CargarDatos()
        {
            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_GetDetalleVentaCompleto";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VentaID", ventaId);
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            int y = 90;
                            y = RenderSidebarGroup("Inmueble Adquirido", $"Lote {r["LoteNumero"]}\nBloque {r["NumeroBloque"]}\nEtapa: {r["Etapa"]}\nProyecto: {r["Proyecto"]}\nÁrea: {r["LoteArea"]} v2", y, Color.FromArgb(248, 250, 252));
                            y = RenderSidebarGroup("Datos Financieros", $"Tipo: {r["Tipo"]}\nFecha: {Convert.ToDateTime(r["Fecha"]).ToString("dd/MM/yyyy")}\nPlazo: {r["Plazo"]} años\nTasa Interés: {r["Interes"]}%\nPrima: L. {Convert.ToDouble(r["Prima"]).ToString("N2")}", y, Color.FromArgb(248, 250, 252));
                            y = RenderSidebarGroup("Cliente / Comprador", $"{r["ClienteNombre"]}\nDNI: {r["ClienteDNI"]}\nTel: {r["ClienteTelefono"]}", y, Color.FromArgb(16, 185, 129));
                            if (r["AvalNombre"] != DBNull.Value) y = RenderSidebarGroup("Aval Recomendado", r["AvalNombre"].ToString(), y, Color.FromArgb(245, 158, 11));
                            if (r["BeneficiarioNombre"] != DBNull.Value) y = RenderSidebarGroup("Beneficiario Legal", r["BeneficiarioNombre"].ToString(), y, Color.FromArgb(59, 130, 246));
                            y = RenderSidebarGroup("Asesor a Cargo", r["AsesorNombre"].ToString(), y, CSub);
                        }
                    }

                    cmd.CommandText = "sp_GetPlanPagoDetalle";
                    cmd.CommandType = CommandType.StoredProcedure;
                    var dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    dgvPlan.DataSource = dt;
                }
            } catch (Exception ex) { MessageBox.Show("Error al cargar detalles de venta: " + ex.Message); }
        }

        private int RenderSidebarGroup(string titulo, string valor, int y, Color subtituloColor)
        {
            var lblT = new Label { Text = titulo.ToUpper(), ForeColor = subtituloColor, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), Location = new Point(20, y), AutoSize = true };
            var lblV = new Label { Text = valor, ForeColor = Color.White, Font = new Font("Segoe UI", 9.5f), Location = new Point(20, y + 20), AutoSize = true, MaximumSize = new Size(240, 0) };
            pnlSidebar.Controls.Add(lblT);
            pnlSidebar.Controls.Add(lblV);
            return y + lblV.Height + 40;
        }


    }
}

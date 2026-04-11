using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    public class FormDetalleFinanciero : Form
    {
        private static readonly Color CSidebar    = Color.FromArgb(15, 23, 42);
        private static readonly Color CSidebarHov = Color.FromArgb(30, 41, 59);
        private static readonly Color CSidebarSel = Color.FromArgb(67, 97, 238);
        private static readonly Color CAccent     = Color.FromArgb(67, 97, 238);
        private static readonly Color CFondo      = Color.FromArgb(241, 245, 249);
        private static readonly Color CCard       = Color.White;
        private static readonly Color CTexto      = Color.FromArgb(15, 23, 42);
        private static readonly Color CSub        = Color.FromArgb(100, 116, 139);
        private static readonly Color CBorder     = Color.FromArgb(226, 232, 240);
        private static readonly Color CHeader     = Color.FromArgb(248, 250, 252);
        private static readonly Color CSeleccion  = Color.FromArgb(235, 241, 255);

        private readonly int proyectoId;
        private readonly string proyectoNombre;
        private readonly string connStr;

        private Panel pnlContenedorPrincipal;
        private Label lblSeccionActiva;
        private Label lblTotalIngresos, lblTotalGastos, lblTotalBeneficio;

        public FormDetalleFinanciero(int pId, string pNombre, string cStr)
        {
            proyectoId = pId;
            proyectoNombre = pNombre;
            connStr = cStr;
            
            Text = $"Expediente Financiero: {proyectoNombre}";
            WindowState = FormWindowState.Maximized;
            BackColor = CFondo;
            Font = new Font("Segoe UI", 9);

            BuildUI();
        }

        private void BuildUI()
        {
            // SIDEBAR
            var sidebar = new Panel { Dock = DockStyle.Left, Width = 240, BackColor = CSidebar };
            var brand = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Color.FromArgb(10, 16, 30) };
            brand.Controls.Add(new Label { Text = "💰 FINANZAS", ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), Dock = DockStyle.Top, TextAlign = ContentAlignment.BottomCenter, Height = 28 });
            brand.Controls.Add(new Label { Text = proyectoNombre.ToUpper(), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Dock = DockStyle.Bottom, TextAlign = ContentAlignment.TopCenter, Height = 44 });
            sidebar.Controls.Add(brand);
            sidebar.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(30, 41, 59) });

            var btnIngresos = NavBtn("📈  Ingresos", "Ingresos");
            var btnGastos   = NavBtn("📉  Gastos", "Gastos");
            var btnVentas   = NavBtn("🤝  Historial Ventas", "Ventas");

            btnIngresos.Click += (s, e) => { ActivarNav(btnIngresos); MostrarGrid("Ingresos"); };
            btnGastos.Click   += (s, e) => { ActivarNav(btnGastos);   MostrarGrid("Gastos"); };
            btnVentas.Click   += (s, e) => { ActivarNav(btnVentas);   MostrarGrid("Ventas"); };

            sidebar.Controls.Add(new Label { Text = "  REPORTES", ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 7, FontStyle.Bold), Dock = DockStyle.Top, Height = 36, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(16, 0, 0, 0) });
            sidebar.Controls.Add(btnIngresos);
            sidebar.Controls.Add(btnGastos);
            sidebar.Controls.Add(btnVentas);

            // MAIN AREA
            var mainArea = new Panel { Dock = DockStyle.Fill };
            
            var header = new Panel { Dock = DockStyle.Top, Height = 130, BackColor = CCard };
            header.Paint += (s, e) => e.Graphics.DrawLine(new Pen(CBorder), 0, header.Height-1, header.Width, header.Height-1);

            lblSeccionActiva = new Label { Text = "INGRESOS", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = CTexto, Location = new Point(26, 15), AutoSize = true };
            header.Controls.Add(lblSeccionActiva);

            // TARJETAS GLOBALES
            int x = 26;
            lblTotalIngresos  = CrearTarjetaGeneral(header, "INGRESOS TOTALES", x, 55, Color.FromArgb(16, 185, 129)); x += 220;
            lblTotalGastos    = CrearTarjetaGeneral(header, "GASTOS TOTALES", x, 55, Color.FromArgb(239, 68, 68)); x += 220;
            lblTotalBeneficio = CrearTarjetaGeneral(header, "BENEFICIO NETO", x, 55, CSidebarSel);

            CargarTotalesGlobales();

            pnlContenedorPrincipal = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 20, 24, 24) };

            mainArea.Controls.Add(pnlContenedorPrincipal);
            mainArea.Controls.Add(header);

            Controls.Add(mainArea);
            Controls.Add(sidebar);

            ActivarNav(btnIngresos);
            MostrarGrid("Ingresos");
        }

        private Label CrearTarjetaGeneral(Panel parent, string titulo, int x, int y, Color cNum)
        {
            var p = new Panel { Location = new Point(x, y), Size = new Size(200, 60), BackColor = CFondo };
            var lT = new Label { Text = titulo, ForeColor = CSub, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(12, 10), AutoSize = true };
            var lN = new Label { Text = "L. 0.00", ForeColor = cNum, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 28), AutoSize = true };
            p.Controls.Add(lT); p.Controls.Add(lN);
            parent.Controls.Add(p);
            return lN;
        }

        private Button NavBtn(string text, string tag)
        {
            var b = new Button { Text = "   " + text, Tag = tag, Height = 50, Dock = DockStyle.Top, FlatStyle = FlatStyle.Flat, ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 9.5f), TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand, BackColor = Color.Transparent };
            b.FlatAppearance.BorderSize = 0; b.FlatAppearance.MouseOverBackColor = CSidebarHov;
            return b;
        }

        private void ActivarNav(Button btn)
        {
            foreach (Control c in btn.Parent.Controls) if (c is Button b) { b.BackColor = Color.Transparent; b.ForeColor = Color.FromArgb(148, 163, 184); b.Font = new Font("Segoe UI", 9.5f); }
            btn.BackColor = CSidebarSel; btn.ForeColor = Color.White; btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblSeccionActiva.Text = btn.Tag.ToString().ToUpper() + " DEL PROYECTO";
        }

        private void CargarTotalesGlobales()
        {
            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_GetResumenFinancieroProyecto";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProyectoID", proyectoId);
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            lblTotalIngresos.Text  = "L. " + Convert.ToDouble(r["IngresoTotal"]).ToString("N2");
                            lblTotalGastos.Text    = "L. " + Convert.ToDouble(r["GastoTotal"]).ToString("N2");
                            lblTotalBeneficio.Text = "L. " + Convert.ToDouble(r["BeneficioTotal"]).ToString("N2");
                        }
                    }
                }
            } catch { } // Se ignora si falla
        }

        private void MostrarGrid(string tipo)
        {
            pnlContenedorPrincipal.Controls.Clear();
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = CCard, BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false,
                ReadOnly = true, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                GridColor = CBorder, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None, Font = new Font("Segoe UI", 9),
                RowTemplate = { Height = 46 }
            };
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = CHeader, ForeColor = CSub, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), Padding = new Padding(14, 0, 0, 0), SelectionBackColor = CHeader, SelectionForeColor = CSub };
            dgv.DefaultCellStyle = new DataGridViewCellStyle { BackColor = CCard, ForeColor = CTexto, SelectionBackColor = CSeleccion, SelectionForeColor = CTexto, Padding = new Padding(14, 0, 0, 0) };

            var wrapper = new Panel { Dock = DockStyle.Fill, BackColor = CCard, BorderStyle = BorderStyle.FixedSingle };
            wrapper.Controls.Add(dgv);
            pnlContenedorPrincipal.Controls.Add(wrapper);

            string procName = null;
            if (tipo == "Ingresos") procName = "sp_GetIngresosPorEtapa";
            else if (tipo == "Gastos") procName = "sp_GetGastosPorProyecto";
            else if (tipo == "Ventas") procName = "sp_GetHistorialVentasProyecto";

            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd = conn.CreateCommand())
                {
                    if (string.IsNullOrEmpty(procName)) throw new InvalidOperationException("Procedimiento no definido para el tipo solicitado.");
                    cmd.CommandText = procName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    // cmd logic handled in the if(tipo) block above
                    cmd.Parameters.AddWithValue("@ProyectoID", proyectoId);
                    conn.Open();
                    var dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    dgv.DataSource = dt;

                    // Formatear montos
                    dgv.CellFormatting += (s, e) => {
                        if (e.Value != null && (dgv.Columns[e.ColumnIndex].Name.Contains("Ingreso") || dgv.Columns[e.ColumnIndex].Name.Contains("Valor") || dgv.Columns[e.ColumnIndex].Name.Contains("Prima") || dgv.Columns[e.ColumnIndex].Name.Contains("Capital") || dgv.Columns[e.ColumnIndex].Name.Contains("Interes") || dgv.Columns[e.ColumnIndex].Name.Contains("Monto")))
                        {
                            if (double.TryParse(e.Value.ToString(), out double val)) { e.Value = "L. " + val.ToString("N2"); e.FormattingApplied = true; }
                        }
                    };

                    if (tipo == "Ventas")
                    {
                        var btnCols = new DataGridViewButtonColumn { Name = "VerDetalle", HeaderText = "", Text = "Ver Detalles →", UseColumnTextForButtonValue = true, FlatStyle = FlatStyle.Flat, Width = 110 };
                        btnCols.DefaultCellStyle = new DataGridViewCellStyle { BackColor = CSeleccion, ForeColor = CAccent, SelectionBackColor = CSeleccion, SelectionForeColor = CAccent, Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8f, FontStyle.Bold) };
                        dgv.Columns.Add(btnCols);
                        dgv.CellContentClick += (sender, ev) => {
                            if (ev.RowIndex >= 0 && dgv.Columns[ev.ColumnIndex].Name == "VerDetalle") {
                                int vID = Convert.ToInt32(dgv.Rows[ev.RowIndex].Cells["VentaID"].Value);
                                new FormDetalleVenta(vID, connStr).ShowDialog();
                                MostrarGrid("Ventas");
                            }
                        };
                        dgv.Columns["VentaID"].Visible = false;
                    }
                }
            } catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
    }
}

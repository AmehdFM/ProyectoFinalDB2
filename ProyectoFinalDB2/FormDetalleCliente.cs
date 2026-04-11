using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    public class FormDetalleCliente : Form
    {
        private static readonly Color CSidebar    = Color.FromArgb(15, 23, 42);
        private static readonly Color CSidebarSel = Color.FromArgb(67, 97, 238);
        private static readonly Color CFondo      = Color.FromArgb(241, 245, 249);
        private static readonly Color CCard       = Color.White;
        private static readonly Color CBorder     = Color.FromArgb(226, 232, 240);
        private static readonly Color CSub        = Color.FromArgb(100, 116, 139);

        private readonly int clienteId;
        private readonly string clienteNombre;
        private readonly string connStr;

        private Panel pnlContenedorPrincipal;
        private Label lblSeccionActiva;

        public FormDetalleCliente(int id, string nombre, string cStr)
        {
            clienteId = id; clienteNombre = nombre; connStr = cStr;
            Text = $"Ficha del Cliente: {clienteNombre}";
            WindowState = FormWindowState.Maximized; BackColor = CFondo; Font = new Font("Segoe UI", 9);
            BuildUI();
        }

        private void BuildUI()
        {
            // SIDEBAR
            var sidebar = new Panel { Dock = DockStyle.Left, Width = 240, BackColor = CSidebar };
            var brand = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Color.FromArgb(10, 16, 30) };
            brand.Controls.Add(new Label { Text = "👥 CLIENTE", ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), Dock = DockStyle.Top, TextAlign = ContentAlignment.BottomCenter, Height = 28 });
            brand.Controls.Add(new Label { Text = clienteNombre.Split(' ')[0].ToUpper(), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Dock = DockStyle.Bottom, TextAlign = ContentAlignment.TopCenter, Height = 44 });
            sidebar.Controls.Add(brand);

            var btnDatos = NavBtn("👤  Datos del Cliente");
            var btnPlan = NavBtn("📅  Plan de Pagos");
            var btnHist = NavBtn("📊  Historial de Recibos");
            var btnFacturas = NavBtn("🧾  Facturas Emitidas");
            var btnPago = NavBtn("💳  Registrar Pago");

            btnDatos.Click += (s, e) => { ActivarNav(btnDatos, "Perfil del Cliente"); MostrarDatosCliente(); };
            btnPlan.Click += (s, e) => { ActivarNav(btnPlan, "Plan de Pagos"); MostrarGrid("Plan"); };
            btnHist.Click += (s, e) => { ActivarNav(btnHist, "Historial y Recibos Emitidos"); MostrarGrid("Historial"); };
            btnFacturas.Click += (s, e) => { ActivarNav(btnFacturas, "Facturas Emitidas"); MostrarGrid("Facturas"); };
            btnPago.Click += (s, e) => { ActivarNav(btnPago, "Registrar Nuevo Pago"); MostrarPanelPago(); };

            sidebar.Controls.Add(new Label { Text = "  INFORMACIÓN", ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 7, FontStyle.Bold), Dock = DockStyle.Top, Height = 36, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(16, 0, 0, 0) });
            sidebar.Controls.Add(btnDatos);
            sidebar.Controls.Add(btnPlan);
            sidebar.Controls.Add(btnHist);
            sidebar.Controls.Add(btnFacturas);
            sidebar.Controls.Add(btnPago);

            // MAIN AREA
            var mainArea = new Panel { Dock = DockStyle.Fill };
            var header = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = CCard };
            header.Paint += (s, e) => e.Graphics.DrawLine(new Pen(CBorder), 0, 67, header.Width, 67);
            
            lblSeccionActiva = new Label { Text = "", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(26, 20), AutoSize = true };
            header.Controls.Add(lblSeccionActiva);

            pnlContenedorPrincipal = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 20, 24, 24) };
            mainArea.Controls.Add(pnlContenedorPrincipal);
            mainArea.Controls.Add(header);

            Controls.Add(mainArea);
            Controls.Add(sidebar);

            ActivarNav(btnPlan, "Plan de Pagos Actual");
            MostrarGrid("Plan");
        }

        private Button NavBtn(string text)
        {
            var b = new Button { Text = "   " + text, Height = 50, Dock = DockStyle.Top, FlatStyle = FlatStyle.Flat, ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 9.5f), TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand, BackColor = Color.Transparent };
            b.FlatAppearance.BorderSize = 0; b.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
            return b;
        }

        private void ActivarNav(Button btn, string titulo)
        {
            foreach (Control c in btn.Parent.Controls) if (c is Button b) { b.BackColor = Color.Transparent; b.ForeColor = Color.FromArgb(148, 163, 184); b.Font = new Font("Segoe UI", 9.5f); }
            btn.BackColor = CSidebarSel; btn.ForeColor = Color.White; btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblSeccionActiva.Text = titulo.ToUpper();
        }

        private void MostrarGrid(string tipo)
        {
            pnlContenedorPrincipal.Controls.Clear();
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = CCard, BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, ReadOnly = true,
                RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, GridColor = CBorder,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                Font = new Font("Segoe UI", 9), RowTemplate = { Height = 46 }
            };
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 250, 252), ForeColor = CSub, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), Padding = new Padding(14, 0, 0, 0) };
            dgv.DefaultCellStyle = new DataGridViewCellStyle { BackColor = CCard, ForeColor = Color.FromArgb(15, 23, 42), SelectionBackColor = Color.FromArgb(235, 241, 255), SelectionForeColor = Color.FromArgb(15, 23, 42), Padding = new Padding(14, 0, 0, 0) };

            var wrapper = new Panel { Dock = DockStyle.Fill, BackColor = CCard, BorderStyle = BorderStyle.FixedSingle };
            wrapper.Controls.Add(dgv); pnlContenedorPrincipal.Controls.Add(wrapper);

            string procName = null;
            if (tipo == "Plan") procName = "sp_GetPlanPagoClienteHome";
            else if (tipo == "Historial") procName = "sp_GetHistorialPagosClienteHome";
            else if (tipo == "Facturas") procName = "sp_GetFacturasCliente";

            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd = conn.CreateCommand())
                {
                    if (string.IsNullOrEmpty(procName)) throw new InvalidOperationException("Procedimiento no definido para el tipo solicitado.");
                    cmd.CommandText = procName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ClienteID", clienteId);

                    conn.Open();
                    var dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    dgv.DataSource = dt;

                    // Colorear montos y estados
                    dgv.CellFormatting += (s, e) => {
                        if (e.Value == null) return;
                        string colName = dgv.Columns[e.ColumnIndex].Name;

                        if (colName.Contains("Monto") || colName.Contains("Capital") || colName.Contains("Interes") || colName.Contains("Mora"))
                        {
                            if (double.TryParse(e.Value.ToString(), out double val)) { e.Value = "L. " + val.ToString("N2"); e.FormattingApplied = true; }
                        }
                        if (colName == "Estado")
                        {
                            if (e.Value.ToString() == "Pagado") { e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129); e.CellStyle.Font = new Font(dgv.Font, FontStyle.Bold); }
                            else { e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11); }
                        }
                    };
                }
            } catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void MostrarPanelPago()
        {
            pnlContenedorPrincipal.Controls.Clear();

            var panelOpciones = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(0, 0, 0, 20) };
            
            var cboCajero = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200, Location = new Point(0, 25) };
            var lblCajero = new Label { Text = "Cajero que procesa:", Location = new Point(0, 5), AutoSize = true, ForeColor = CSub, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            
            var cboCuenta = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200, Location = new Point(220, 25) };
            var lblCuenta = new Label { Text = "Cuenta de Destino:", Location = new Point(220, 5), AutoSize = true, ForeColor = CSub, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            
            var chkDeposito = new CheckBox { Text = "Es pago por Depósito/Transferencia", Location = new Point(440, 25), AutoSize = true, ForeColor = Color.FromArgb(15, 23, 42), Cursor = Cursors.Hand };

            var btnConfirmar = new Button 
            { 
                Text = "CONFIRMAR PAGO", Width = 180, Height = 40,
                BackColor = CSidebarSel, ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnConfirmar.FlatAppearance.BorderSize = 0;

            panelOpciones.Controls.Add(lblCajero); panelOpciones.Controls.Add(cboCajero);
            panelOpciones.Controls.Add(lblCuenta); panelOpciones.Controls.Add(cboCuenta);
            panelOpciones.Controls.Add(chkDeposito);

            var dgvCuotas = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = CCard, BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, ReadOnly = true,
                RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, GridColor = CBorder,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                Font = new Font("Segoe UI", 9), RowTemplate = { Height = 46 }
            };
            dgvCuotas.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 250, 252), ForeColor = CSub, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), Padding = new Padding(14, 0, 0, 0) };
            dgvCuotas.DefaultCellStyle = new DataGridViewCellStyle { BackColor = CCard, ForeColor = Color.FromArgb(15, 23, 42), SelectionBackColor = Color.FromArgb(235, 241, 255), SelectionForeColor = Color.FromArgb(15, 23, 42), Padding = new Padding(14, 0, 0, 0) };

            var wrapper = new Panel { Dock = DockStyle.Fill, BackColor = CCard, BorderStyle = BorderStyle.FixedSingle };
            wrapper.Controls.Add(dgvCuotas);

            var panelBoton = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(0, 10, 0, 0) };
            btnConfirmar.Dock = DockStyle.Right;
            panelBoton.Controls.Add(btnConfirmar);
            wrapper.Controls.Add(panelBoton);

            pnlContenedorPrincipal.Controls.Add(wrapper);
            pnlContenedorPrincipal.Controls.Add(panelOpciones);

            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("sp_GetListaEmpleados", conn)) {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (var dr = cmd.ExecuteReader()) {
                            var dtEmp = new DataTable(); dtEmp.Load(dr);
                            cboCajero.DataSource = dtEmp; cboCajero.DisplayMember = "Nombre"; cboCajero.ValueMember = "EmpleadoID";
                        }
                    }
                    using (var cmd = new SqlCommand("sp_GetCuentasCombo", conn)) {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (var dr = cmd.ExecuteReader()) {
                            var dtCta = new DataTable(); dtCta.Load(dr);
                            cboCuenta.DataSource = dtCta; cboCuenta.DisplayMember = "Detalle"; cboCuenta.ValueMember = "CuentaID";
                        }
                    }
                    using (var cmd = new SqlCommand("sp_GetCuotasPendientesCliente", conn)) {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ClienteID", clienteId);
                        var dtCuotas = new DataTable(); dtCuotas.Load(cmd.ExecuteReader());
                        dgvCuotas.DataSource = dtCuotas;
                        dgvCuotas.Columns["PlanPagoID"].Visible = false;
                        dgvCuotas.Columns["VentaID"].Visible = false;
                    }

                    dgvCuotas.CellFormatting += (s, e) => {
                        if (e.Value != null && dgvCuotas.Columns[e.ColumnIndex].Name == "Monto") {
                            if (double.TryParse(e.Value.ToString(), out double val)) { e.Value = "L. " + val.ToString("N2"); e.FormattingApplied = true; }
                        }
                    };
                }
            }
            catch (Exception ex) { MessageBox.Show("Error cargando opciones: " + ex.Message); }

            btnConfirmar.Click += (s, e) => {
                if (dgvCuotas.SelectedRows.Count == 0) { MessageBox.Show("Seleccione una cuota a pagar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (cboCajero.SelectedValue == null || cboCuenta.SelectedValue == null) { MessageBox.Show("Verifique que exista cajero y cuenta disponibles.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                
                var r = dgvCuotas.SelectedRows[0];
                int planId = Convert.ToInt32(r.Cells["PlanPagoID"].Value);
                int ventaId = Convert.ToInt32(r.Cells["VentaID"].Value);
                double monto = Convert.ToDouble(r.Cells["Monto"].Value);
                int empId = Convert.ToInt32(cboCajero.SelectedValue);
                int ctaId = Convert.ToInt32(cboCuenta.SelectedValue);
                string tipo = chkDeposito.Checked ? "Deposito" : "Efectivo";

                if (MessageBox.Show($"¿Desea registrar el pago por L. {monto:N2}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try {
                        using (var conn = new SqlConnection(connStr))
                        using (var cmd = new SqlCommand("spRegistrarPago", conn)) {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@VentaID", ventaId);
                            cmd.Parameters.AddWithValue("@PlanPagoID", planId);
                            cmd.Parameters.AddWithValue("@EmpleadoID", empId);
                            cmd.Parameters.AddWithValue("@CuentaID", ctaId);
                            cmd.Parameters.AddWithValue("@MontoPagado", monto);
                            cmd.Parameters.AddWithValue("@TipoPago", tipo);
                            
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Pago registrado correctamente e Historial actualizado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            foreach(Control c in this.Controls) {
                                if(c is Panel mainArea && c.Dock == DockStyle.Fill) continue; // skip main
                                if(c is Panel sidebarPanel) {
                                    foreach(Control btn in sidebarPanel.Controls) {
                                        if(btn is Button sideBtn && sideBtn.Text.Contains("Historial")) {
                                            sideBtn.PerformClick();
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    } catch(Exception ex) { MessageBox.Show("Ocurrió un error al registrar el pago: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            };
        }

        private void MostrarDatosCliente()
        {
            pnlContenedorPrincipal.Controls.Clear();

            var card = new Panel 
            { 
                Width = 600, Height = 450, 
                BackColor = CCard, 
                Location = new Point(24, 24),
                Padding = new Padding(30)
            };
            card.Paint += (s, e) => {
                using (var pen = new Pen(CBorder, 1)) {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            var pic = new Label { Text = "👤", Font = new Font("Segoe UI", 48), Size = new Size(100, 100), TextAlign = ContentAlignment.MiddleCenter, ForeColor = CSidebarSel };
            pic.Location = new Point(card.Width / 2 - 50, 30);
            card.Controls.Add(pic);

            var lblNombre = new Label { 
                Text = clienteNombre.ToUpper(), 
                Font = new Font("Segoe UI", 16, FontStyle.Bold), 
                ForeColor = Color.FromArgb(15, 23, 42), 
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(card.Width, 40),
                Location = new Point(0, 140)
            };
            card.Controls.Add(lblNombre);

            var line = new Panel { Height = 1, BackColor = CBorder, Width = 500, Location = new Point(50, 190) };
            card.Controls.Add(line);

            // Container for Info items
            var infoContainer = new Panel { Location = new Point(50, 210), Size = new Size(500, 200) };
            card.Controls.Add(infoContainer);

            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("sp_GetDetalleCliente", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ClienteID", clienteId);
                    conn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            AddDataItem(infoContainer, "IDENTIDAD (DNI)", dr["DNI"].ToString(), 0);
                            AddDataItem(infoContainer, "TELÉFONO", dr["Telefono"].ToString(), 50);
                            AddDataItem(infoContainer, "LUGAR DE TRABAJO", dr["Trabajo"].ToString(), 100);
                            
                            double sueldo = Convert.ToDouble(dr["Sueldo"]);
                            AddDataItem(infoContainer, "SUELDO MENSUAL", "L. " + sueldo.ToString("N2"), 150);
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar datos: " + ex.Message); }

            pnlContenedorPrincipal.Controls.Add(card);
        }

        private void AddDataItem(Panel parent, string label, string value, int y)
        {
            var lblL = new Label { Text = label, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = CSub, AutoSize = true, Location = new Point(0, y) };
            var lblV = new Label { Text = value, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(30, 41, 59), AutoSize = true, Location = new Point(0, y + 18) };
            parent.Controls.Add(lblL);
            parent.Controls.Add(lblV);
        }
    }
}

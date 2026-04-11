using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    /// <summary>
    /// Formulario maximizado para registrar una venta.
    /// Layout de 3 columnas: [Datos Venta] | [Cliente] | [Aval + Beneficiario]
    /// Permite crear Cliente, Aval y Beneficiario abriendo modales.
    /// </summary>
    public class FormCrearVenta : Form
    {
        // ── Colores ──────────────────────────────────────────────────
        private static readonly Color CPrimario  = Color.FromArgb(26, 26, 46);
        private static readonly Color CAccent    = Color.FromArgb(67, 97, 238);
        private static readonly Color CFondo     = Color.FromArgb(240, 242, 248);
        private static readonly Color CCard      = Color.White;
        private static readonly Color CBorder    = Color.FromArgb(210, 218, 235);
        private static readonly Color CSub       = Color.FromArgb(115, 125, 150);

        // ── Datos ────────────────────────────────────────────────────
        private readonly int    loteId;
        private readonly string connStr;

        // ── Col 1: Venta ─────────────────────────────────────────────
        private Label         lblInfoLote, lblValorLote;
        private ComboBox      cmbTipo, cmbEmpleado;
        private NumericUpDown nudPrima, nudPlazo;
        private double        interesConfigurado = 0;
        private Panel         pnlCamposCredito;

        // ── Col 2: Cliente ───────────────────────────────────────────
        private ComboBox cmbCliente;
        private Button   btnNuevoCliente;

        // ── Col 3: Aval ──────────────────────────────────────────────
        private ComboBox cmbAval;
        private Button   btnNuevoAval;

        // ── Col 3: Beneficiario ──────────────────────────────────────
        private ComboBox cmbBenef;
        private Button   btnNuevoBenef;

        // ─────────────────────────────────────────────────────────────
        public FormCrearVenta(int loteId, string connStr)
        {
            this.loteId  = loteId;
            this.connStr = connStr;

            Text           = "Registrar Nueva Venta";
            WindowState    = FormWindowState.Maximized;
            StartPosition  = FormStartPosition.CenterScreen;
            MinimumSize    = new Size(1150, 700);
            BackColor      = CFondo;
            Font           = new Font("Segoe UI", 9);

            BuildUI();
            CargarCombos();
            MostrarInfoLote();
        }

        // ══════════════════════════════════════════════════════════════
        //  CONSTRUCCIÓN DEL UI
        // ══════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            // ── Header ────────────────────────────────────────────────
            var hdr = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = CPrimario };
            lblInfoLote = new Label { Text = "Cargando lote…", ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = true, Location = new Point(24, 12) };
            lblValorLote = new Label { Text = "", ForeColor = Color.FromArgb(170, 185, 215),
                Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(26, 40) };
            hdr.Controls.Add(lblInfoLote);
            hdr.Controls.Add(lblValorLote);
            Controls.Add(hdr);

            // ── Footer ────────────────────────────────────────────────
            var footer = new Panel { Dock = DockStyle.Bottom, Height = 68, BackColor = CCard,
                Padding = new Padding(16, 12, 16, 12) };
            footer.Paint += (s, e) => {
                e.Graphics.DrawLine(new Pen(CBorder), 0, 0, footer.Width, 0);
            };

            var btnOk = new Button { Text = "✔  REGISTRAR VENTA", Dock = DockStyle.Right, Width = 220,
                BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += BtnRegistrar_Click;

            var btnCx = new Button { Text = "Cancelar", Dock = DockStyle.Left, Width = 120,
                BackColor = CCard, ForeColor = Color.FromArgb(80, 90, 110), FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand, DialogResult = DialogResult.Cancel };
            btnCx.FlatAppearance.BorderColor = CBorder;
            btnCx.FlatAppearance.BorderSize  = 1;
            
            footer.Controls.Add(btnOk);
            footer.Controls.Add(btnCx);
            Controls.Add(footer);
            CancelButton = btnCx;

            // ── TableLayoutPanel 3 columnas ───────────────────────────
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount    = 1,
                Padding     = new Padding(14, 12, 14, 6),
                BackColor   = CFondo
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(tbl);

            tbl.Controls.Add(BuildColVenta(),       0, 0);
            tbl.Controls.Add(BuildColCliente(),     1, 0);
            tbl.Controls.Add(BuildColAvalBenef(),   2, 0);
        }

        // ══════════════════════════════════════════════════════════════
        //  COLUMNA 1 — DATOS DE LA VENTA
        // ══════════════════════════════════════════════════════════════
        private Panel BuildColVenta()
        {
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            var card   = Card("DATOS DE LA VENTA", new Padding(10));
            int x = 16, y = 52;

            // Tipo
            VLbl("Tipo de Venta *", card, x, y);
            cmbTipo = new ComboBox { Location = new Point(x, y + 22), DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            cmbTipo.Items.AddRange(new object[] { "Contado", "Credito" });
            cmbTipo.SelectedIndexChanged += (s, _) => ToggleCredito();
            card.Controls.Add(cmbTipo); y += 58;

            // Prima
            VLbl("Prima inicial (L.)", card, x, y);
            nudPrima = NUD(card, x, y + 22, 2, 0, 9999999m);
            nudPrima.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top; y += 58;

            // Panel campos crédito
            pnlCamposCredito = new Panel { Location = new Point(x, y), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top, Height = 70, Visible = false };
            VLbl("Plazo (años) *", pnlCamposCredito, 0, 0);
            nudPlazo = NUD(pnlCamposCredito, 0, 20, 0, 1, 30, 1);
            nudPlazo.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            card.Controls.Add(pnlCamposCredito); y += 75;

            // Empleado
            VLbl("Empleado que realiza la venta *", card, x, y);
            cmbEmpleado = new ComboBox { Location = new Point(x, y + 22), DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            card.Controls.Add(cmbEmpleado);

            card.Resize += (s, _) => {
                int dw = card.ClientSize.Width - x * 2;
                if (dw < 60) return;
                cmbTipo.Width = dw; nudPrima.Width = dw;
                pnlCamposCredito.Width = dw;
                nudPlazo.Width = dw;
                cmbEmpleado.Width = dw;
            };

            card.Dock = DockStyle.Fill;
            scroll.Controls.Add(card);
            return scroll;
        }

        // ══════════════════════════════════════════════════════════════
        //  COLUMNA 2 — CLIENTE
        // ══════════════════════════════════════════════════════════════
        private Panel BuildColCliente()
        {
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            var card   = Card("DATOS DEL CLIENTE", new Padding(10));
            int x = 16, y = 52;

            card.Controls.Add(SeccionTitulo("Seleccionar cliente existente", x, y)); y += 24;
            cmbCliente = new ComboBox { Location = new Point(x, y), DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            card.Controls.Add(cmbCliente); y += 44;

            btnNuevoCliente = ToggleBtn("＋  Registrar Nuevo Cliente", x, y);
            btnNuevoCliente.Click += (s, _) => AbrirModalPersona(TipoPersona.Cliente, cmbCliente);
            card.Controls.Add(btnNuevoCliente);

            card.Resize += (s, _) => {
                int dw = card.ClientSize.Width - x * 2;
                if (dw < 60) return;
                cmbCliente.Width = dw; btnNuevoCliente.Width = dw;
            };

            card.Dock = DockStyle.Fill;
            scroll.Controls.Add(card);
            return scroll;
        }

        // ══════════════════════════════════════════════════════════════
        //  COLUMNA 3 — AVAL + BENEFICIARIO
        // ══════════════════════════════════════════════════════════════
        private Panel BuildColAvalBenef()
        {
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            var card   = Card("AVAL Y BENEFICIARIO", new Padding(10));
            int x = 16, y = 52;

            // ── Aval ──────────────────────────────────────────────────
            card.Controls.Add(SeccionTitulo("Aval  (opcional)", x, y)); y += 24;

            cmbAval = new ComboBox { Location = new Point(x, y), DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            card.Controls.Add(cmbAval); y += 44;

            btnNuevoAval = ToggleBtn("＋  Registrar Nuevo Aval", x, y);
            btnNuevoAval.Click += (s, _) => AbrirModalPersona(TipoPersona.Aval, cmbAval);
            card.Controls.Add(btnNuevoAval); y += 60;

            // ── Separador ─────────────────────────────────────────────
            var sep = new Panel { Location = new Point(x, y), Height = 1,
                BackColor = CBorder, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            card.Controls.Add(sep); y += 20;

            // ── Beneficiario ──────────────────────────────────────────
            card.Controls.Add(SeccionTitulo("Beneficiario  (opcional)", x, y)); y += 24;

            cmbBenef = new ComboBox { Location = new Point(x, y), DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            card.Controls.Add(cmbBenef); y += 44;

            btnNuevoBenef = ToggleBtn("＋  Registrar Nuevo Beneficiario", x, y);
            btnNuevoBenef.Click += (s, _) => AbrirModalPersona(TipoPersona.Beneficiario, cmbBenef);
            card.Controls.Add(btnNuevoBenef);

            // resize
            card.Resize += (s, _) => {
                int dw = card.ClientSize.Width - x * 2;
                if (dw < 60) return;
                cmbAval.Width = dw; btnNuevoAval.Width = dw;
                sep.Width = dw;
                cmbBenef.Width = dw; btnNuevoBenef.Width = dw;
            };

            card.Dock = DockStyle.Fill;
            scroll.Controls.Add(card);
            return scroll;
        }

        // ══════════════════════════════════════════════════════════════
        //  HELPERS DE UI
        // ══════════════════════════════════════════════════════════════
        private Panel Card(string titulo, Padding padding)
        {
            var p = new Panel { BackColor = CCard, Padding = padding,
                Margin = new Padding(6), BorderStyle = BorderStyle.FixedSingle };
            p.Controls.Add(new Label { Text = titulo, Location = new Point(16, 14), AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = CPrimario });
            return p;
        }

        private Label SeccionTitulo(string t, int x, int y) =>
            new Label { Text = t, Location = new Point(x, y), AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = CSub };

        private void VLbl(string t, Control parent, int x, int y) =>
            parent.Controls.Add(new Label { Text = t, Location = new Point(x, y), AutoSize = true,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 80, 105) });

        private NumericUpDown NUD(Control parent, int x, int y, int dec, decimal min, decimal max, decimal val = 0)
        {
            if (val < min) val = min;
            if (val > max) val = max;
            var n = new NumericUpDown { Location = new Point(x, y), Width = 200,
                DecimalPlaces = dec, Minimum = min, Maximum = max, Value = val,
                Font = new Font("Segoe UI", 9) };
            parent.Controls.Add(n); return n;
        }

        private Button ToggleBtn(string text, int x, int y)
        {
            var b = new Button { Text = text, Location = new Point(x, y), Width = 200, Height = 34,
                BackColor = Color.FromArgb(235, 240, 255), ForeColor = CAccent, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(6, 0, 0, 0) };
            b.FlatAppearance.BorderColor = Color.FromArgb(200, 210, 245);
            b.FlatAppearance.BorderSize  = 1;
            return b;
        }

        // ══════════════════════════════════════════════════════════════
        //  MODALES Y CARGA DE DATOS
        // ══════════════════════════════════════════════════════════════
        private void AbrirModalPersona(TipoPersona tipo, ComboBox cmdTarget)
        {
            using (var frm = new FormCrearPersonaModal(connStr, tipo))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    cmdTarget.Items.Add(new CbItem { Text = frm.NuevoNombre, Value = frm.NuevoID });
                    cmdTarget.SelectedIndex = cmdTarget.Items.Count - 1;
                }
            }
        }

        private void MostrarInfoLote()
        {
            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd  = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT L.Numero, B.NumeroBloque, E.Nombre AS Etapa, P.Nombre AS Proyecto,
                               dbo.fnValorLote(L.LoteID) AS Valor, E.Interes
                        FROM Lote L
                        JOIN Bloque B ON L.BloqueID = B.BloqueID
                        JOIN Etapa E  ON B.EtapaID  = E.EtapaID
                        JOIN Proyecto P ON E.ProyectoID = P.ProyectoID
                        WHERE L.LoteID = @id";
                    cmd.Parameters.AddWithValue("@id", loteId);
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            lblInfoLote.Text  = $"REGISTRAR VENTA  —  Lote #{r["Numero"]}  |  Bloque {r["NumeroBloque"]}  |  Etapa: {r["Etapa"]}  |  Proyecto: {r["Proyecto"]}";
                            interesConfigurado = Convert.ToDouble(r["Interes"]);
                            lblValorLote.Text = $"Valor calculado: L. {Convert.ToDouble(r["Valor"]):N2}  |  Interés Etapa: {interesConfigurado}%";
                        }
                    }
                }
            }
            catch { lblInfoLote.Text = "REGISTRAR VENTA"; }
        }

        private void CargarCombos()
        {
            Combo(cmbCliente,  "SELECT ClienteID, Nombre FROM Cliente ORDER BY Nombre",         "ClienteID", "Nombre", true);
            Combo(cmbEmpleado, "SELECT EmpleadoID, Nombre FROM Empleado ORDER BY Nombre",       "EmpleadoID","Nombre");
            Combo(cmbAval,     "SELECT AvalID, Nombre FROM Aval ORDER BY Nombre",               "AvalID",    "Nombre", true);
            Combo(cmbBenef,    "SELECT BeneficiarioID, Nombre FROM Beneficiario ORDER BY Nombre","BeneficiarioID","Nombre", true);
            if (cmbTipo.Items.Count > 0) cmbTipo.SelectedIndex = 0;
        }

        private void Combo(ComboBox cmb, string sql, string colId, string colNombre, bool nullable = false)
        {
            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd  = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (nullable) cmb.Items.Add(new CbItem { Text = "(Ninguno)", Value = -1 });
                        while (r.Read())
                            cmb.Items.Add(new CbItem { Text = r[colNombre]?.ToString(), Value = Convert.ToInt32(r[colId]) });
                    }
                }
                if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
            }
            catch (Exception ex) { MessageBox.Show("Error cargando combo: " + ex.Message); }
        }

        private void ToggleCredito()
        {
            bool esCredito = cmbTipo.SelectedItem?.ToString() == "Credito";
            pnlCamposCredito.Visible = esCredito;
            if (!esCredito) { nudPlazo.Value = 1; }
        }

        // ══════════════════════════════════════════════════════════════
        //  REGISTRAR VENTA
        // ══════════════════════════════════════════════════════════════
        private void BtnRegistrar_Click(object sender, EventArgs e)
        {
            if (cmbCliente.SelectedItem == null || ((CbItem)cmbCliente.SelectedItem).Value == -1)
            { MessageBox.Show("Seleccione o registre un cliente.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            
            if (cmbEmpleado.SelectedItem == null)
            { MessageBox.Show("Seleccione un empleado.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            
            if (cmbTipo.SelectedItem == null)
            { MessageBox.Show("Seleccione el tipo de venta.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            string tipo      = cmbTipo.SelectedItem.ToString();
            bool   esCredito = tipo == "Credito";

            var avalItem  = cmbAval.SelectedItem  as CbItem;
            var benefItem = cmbBenef.SelectedItem as CbItem;
            int? avalId   = (avalItem  == null || avalItem.Value  == -1) ? (int?)null : avalItem.Value;
            int? benefId  = (benefItem == null || benefItem.Value == -1) ? (int?)null : benefItem.Value;

            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd  = new SqlCommand("sp_CrearVenta", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@LoteID",        SqlDbType.Int)         { Value = loteId });
                    cmd.Parameters.Add(new SqlParameter("@ClienteID",     SqlDbType.Int)         { Value = ((CbItem)cmbCliente.SelectedItem).Value });
                    cmd.Parameters.Add(new SqlParameter("@EmpleadoID",    SqlDbType.Int)         { Value = ((CbItem)cmbEmpleado.SelectedItem).Value });
                    cmd.Parameters.Add(new SqlParameter("@AvalID",        SqlDbType.Int)         { Value = avalId.HasValue  ? (object)avalId.Value  : DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@BeneficiarioID",SqlDbType.Int)         { Value = benefId.HasValue ? (object)benefId.Value : DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@Tipo",          SqlDbType.VarChar, 10) { Value = tipo });
                    cmd.Parameters.Add(new SqlParameter("@Prima",         SqlDbType.Float)       { Value = (double)nudPrima.Value });
                    cmd.Parameters.Add(new SqlParameter("@Plazo",         SqlDbType.Int)         { Value = esCredito ? (int)nudPlazo.Value : 1 });
                    cmd.Parameters.Add(new SqlParameter("@Interes",       SqlDbType.Float)       { Value = esCredito ? interesConfigurado : 0.0 });
                    conn.Open(); cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Venta registrada. El plan de pago fue generado automáticamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private class CbItem { public string Text { get; set; } public int Value { get; set; } public override string ToString() => Text; }
    }
}

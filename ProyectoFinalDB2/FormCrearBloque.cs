using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    public class FormCrearBloque : Form
    {
        private readonly int    proyectoId;
        private readonly string connStr;

        private ComboBox      cmbEtapas;
        private NumericUpDown nudNumero;
        private Label         lblSugerencia;

        private static readonly Color CPrimario = Color.FromArgb(26, 26, 46);
        private static readonly Color CAccent   = Color.FromArgb(67, 97, 238);
        private static readonly Color CFondo    = Color.FromArgb(247, 248, 252);
        private static readonly Color CBorder   = Color.FromArgb(210, 215, 230);

        public FormCrearBloque(int proyectoId, string connStr)
        {
            this.proyectoId = proyectoId;
            this.connStr    = connStr;
            Text = "Nuevo Bloque";
            Size = new Size(440, 340);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = CFondo;
            Font = new Font("Segoe UI", 9);
            Build();
            LoadEtapas();
        }

        private void Build()
        {
            // ── Header ──
            var hdr = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = CPrimario };
            hdr.Controls.Add(new Label { Text = "CREAR BLOQUE", ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true, Location = new Point(22, 12) });
            hdr.Controls.Add(new Label { Text = "Asigne un bloque a la etapa del proyecto",
                ForeColor = Color.FromArgb(170, 185, 215), Font = new Font("Segoe UI", 8),
                AutoSize = true, Location = new Point(24, 40) });
            Controls.Add(hdr);

            int x = 28, y = 86, w = 356;

            Lbl("Etapa *", x, y);
            cmbEtapas = new ComboBox { Location = new Point(x, y + 22), Width = w,
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cmbEtapas.SelectedIndexChanged += (s, _) => SugerirNumero();
            Controls.Add(cmbEtapas); y += 68;

            Lbl("Número de Bloque *", x, y);
            nudNumero = new NumericUpDown { Location = new Point(x, y + 22), Width = w,
                Minimum = 1, Maximum = 100000, Font = new Font("Segoe UI", 9) };
            Controls.Add(nudNumero);

            lblSugerencia = new Label { Text = "", Location = new Point(x, y + 48),
                AutoSize = true, Font = new Font("Segoe UI", 7.5f), ForeColor = Color.ForestGreen };
            Controls.Add(lblSugerencia); y += 90;

            var btnOk = Btn("CREAR BLOQUE", x, y, w, CAccent, Color.White);
            btnOk.Click += Save;
            var btnCx = Btn("CANCELAR", x, y + 48, w, CFondo, Color.FromArgb(60, 70, 90));
            btnCx.FlatAppearance.BorderColor = CBorder;
            btnCx.FlatAppearance.BorderSize  = 1;
            btnCx.DialogResult = DialogResult.Cancel;

            AcceptButton = btnOk; CancelButton = btnCx;
            Controls.Add(btnOk); Controls.Add(btnCx);
        }

        private void Lbl(string t, int x, int y) =>
            Controls.Add(new Label { Text = t, Location = new Point(x, y), AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(55, 65, 90) });

        private Button Btn(string text, int x, int y, int w, Color back, Color fore)
        {
            var b = new Button { Text = text, Location = new Point(x, y), Size = new Size(w, 40),
                BackColor = back, ForeColor = fore, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0; return b;
        }

        private void LoadEtapas()
        {
            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd  = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT EtapaID, Nombre FROM Etapa WHERE ProyectoID = @pid ORDER BY Nombre";
                    cmd.Parameters.AddWithValue("@pid", proyectoId);
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            cmbEtapas.Items.Add(new CbItem { Text = r["Nombre"].ToString(), Value = Convert.ToInt32(r["EtapaID"]) });
                }
                if (cmbEtapas.Items.Count > 0) cmbEtapas.SelectedIndex = 0;
            }
            catch (Exception ex) { MessageBox.Show("Error cargando etapas: " + ex.Message); }
        }

        private void SugerirNumero()
        {
            if (cmbEtapas.SelectedItem == null) return;
            int eid = ((CbItem)cmbEtapas.SelectedItem).Value;
            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd  = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT ISNULL(MAX(NumeroBloque), 0) + 1 FROM Bloque WHERE EtapaID = @eid";
                    cmd.Parameters.AddWithValue("@eid", eid);
                    conn.Open();
                    int sug = Convert.ToInt32(cmd.ExecuteScalar());
                    nudNumero.Value = sug;
                    lblSugerencia.Text = $"✓ Siguiente número sugerido: {sug}";
                }
            }
            catch { lblSugerencia.Text = ""; }
        }

        private void Save(object sender, EventArgs e)
        {
            if (cmbEtapas.SelectedItem == null)
            { MessageBox.Show("Seleccione una etapa.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd  = new SqlCommand("sp_CrearBloque", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@EtapaID",     SqlDbType.Int) { Value = ((CbItem)cmbEtapas.SelectedItem).Value });
                    cmd.Parameters.Add(new SqlParameter("@NumeroBloque", SqlDbType.Int) { Value = (int)nudNumero.Value });
                    conn.Open(); cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Bloque creado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private class CbItem { public string Text { get; set; } public int Value { get; set; } public override string ToString() => Text; }
    }
}
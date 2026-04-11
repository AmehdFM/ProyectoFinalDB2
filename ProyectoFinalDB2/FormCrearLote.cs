using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    public class FormCrearLote : Form
    {
        private readonly int    proyectoId;
        private readonly string connStr;

        private ComboBox      cmbBloques;
        private NumericUpDown nudNumero, nudArea;
        private TextBox       txtCatastro, txtMatricula, txtColindancias;
        private CheckBox      chkEsquina, chkParque, chkCalleCerrada;

        private static readonly Color CPrimario = Color.FromArgb(26, 26, 46);
        private static readonly Color CAccent   = Color.FromArgb(67, 97, 238);
        private static readonly Color CFondo    = Color.FromArgb(247, 248, 252);
        private static readonly Color CBorder   = Color.FromArgb(210, 215, 230);

        public FormCrearLote(int proyectoId, string connStr)
        {
            this.proyectoId = proyectoId;
            this.connStr    = connStr;
            Text = "Nuevo Lote";
            Size = new Size(480, 680);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = CFondo;
            Font = new Font("Segoe UI", 9);
            Build();
            LoadBloques();
        }

        private void Build()
        {
            // ── Header ──
            var hdr = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = CPrimario };
            hdr.Controls.Add(new Label { Text = "CREAR LOTE", ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true, Location = new Point(22, 12) });
            hdr.Controls.Add(new Label { Text = "Registre los datos físicos y registrales del lote",
                ForeColor = Color.FromArgb(170, 185, 215), Font = new Font("Segoe UI", 8),
                AutoSize = true, Location = new Point(24, 40) });
            Controls.Add(hdr);

            int x = 28, y = 84, w = 394;

            // Bloque
            Lbl("Bloque *", x, y);
            cmbBloques = new ComboBox { Location = new Point(x, y + 22), Width = w,
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            Controls.Add(cmbBloques); y += 64;

            // Número + Área (lado a lado)
            Lbl("N° de Lote *", x, y);
            nudNumero = new NumericUpDown { Location = new Point(x, y + 22), Width = 176,
                Minimum = 1, Maximum = 100000, Font = new Font("Segoe UI", 9) };
            Controls.Add(nudNumero);

            Lbl("Área (varas²) *", x + 192, y);
            nudArea = new NumericUpDown { Location = new Point(x + 192, y + 22), Width = 202,
                DecimalPlaces = 2, Minimum = 0.01m, Maximum = 9999999m, Font = new Font("Segoe UI", 9) };
            Controls.Add(nudArea); y += 64;

            // Catastro
            Lbl("Catastro *", x, y);
            txtCatastro = Txt(x, y + 22, w); y += 60;

            // Matrícula
            Lbl("Matrícula *", x, y);
            txtMatricula = Txt(x, y + 22, w); y += 60;

            // Colindancias
            Lbl("Colindancias *", x, y);
            txtColindancias = new TextBox { Location = new Point(x, y + 22), Width = w, Height = 65,
                Multiline = true, ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9), BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(txtColindancias); y += 100;

            // Características especiales (GroupBox)
            var grp = new GroupBox { Text = "Características Especiales  (afectan el valor del lote)",
                Location = new Point(x, y), Size = new Size(w, 90),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(55, 65, 90) };
            chkEsquina    = new CheckBox { Text = "Lote en Esquina  (+10%)", Location = new Point(12, 22), AutoSize = true };
            chkParque     = new CheckBox { Text = "Frente a Parque  (+5%)",  Location = new Point(12, 48), AutoSize = true };
            chkCalleCerrada = new CheckBox { Text = "Calle Cerrada  (+5%)", Location = new Point(210, 22), AutoSize = true };
            grp.Controls.AddRange(new Control[] { chkEsquina, chkParque, chkCalleCerrada });
            Controls.Add(grp); y += 106;

            // Botones
            var btnOk = Btn("CREAR LOTE", x, y, w, CAccent, Color.White);
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

        private TextBox Txt(int x, int y, int w)
        {
            var t = new TextBox { Location = new Point(x, y), Width = w, MaxLength = 50,
                Font = new Font("Segoe UI", 9), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            Controls.Add(t); return t;
        }

        private Button Btn(string text, int x, int y, int w, Color back, Color fore)
        {
            var b = new Button { Text = text, Location = new Point(x, y), Size = new Size(w, 40),
                BackColor = back, ForeColor = fore, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0; return b;
        }

        private void LoadBloques()
        {
            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd  = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT B.BloqueID, E.Nombre AS Etapa, B.NumeroBloque
                                        FROM Bloque B JOIN Etapa E ON B.EtapaID = E.EtapaID
                                        WHERE E.ProyectoID = @pid ORDER BY E.Nombre, B.NumeroBloque";
                    cmd.Parameters.AddWithValue("@pid", proyectoId);
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            cmbBloques.Items.Add(new CbItem
                            {
                                Text  = $"Etapa {r["Etapa"]} — Bloque {r["NumeroBloque"]}",
                                Value = Convert.ToInt32(r["BloqueID"])
                            });
                }
                if (cmbBloques.Items.Count > 0) cmbBloques.SelectedIndex = 0;
            }
            catch (Exception ex) { MessageBox.Show("Error cargando bloques: " + ex.Message); }
        }

        private void Save(object sender, EventArgs e)
        {
            if (cmbBloques.SelectedItem == null)
            { Warn("Seleccione un bloque."); return; }
            if (nudArea.Value <= 0)
            { Warn("El área debe ser mayor a 0."); return; }
            if (string.IsNullOrWhiteSpace(txtCatastro.Text))
            { Warn("El catastro es obligatorio."); txtCatastro.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtMatricula.Text))
            { Warn("La matrícula es obligatoria."); txtMatricula.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtColindancias.Text))
            { Warn("Las colindancias son obligatorias."); txtColindancias.Focus(); return; }

            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd  = new SqlCommand("sp_CrearLote", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@BloqueID",     SqlDbType.Int)          { Value = ((CbItem)cmbBloques.SelectedItem).Value });
                    cmd.Parameters.Add(new SqlParameter("@Numero",       SqlDbType.Int)          { Value = (int)nudNumero.Value });
                    cmd.Parameters.Add(new SqlParameter("@Area",         SqlDbType.Float)        { Value = (double)nudArea.Value });
                    cmd.Parameters.Add(new SqlParameter("@Catastro",     SqlDbType.VarChar, 50)  { Value = txtCatastro.Text.Trim() });
                    cmd.Parameters.Add(new SqlParameter("@Matricula",    SqlDbType.VarChar, 50)  { Value = txtMatricula.Text.Trim() });
                    cmd.Parameters.Add(new SqlParameter("@Colindancias", SqlDbType.VarChar, -1)  { Value = txtColindancias.Text.Trim() });
                    cmd.Parameters.Add(new SqlParameter("@Esquina",      SqlDbType.Bit)          { Value = chkEsquina.Checked });
                    // FIX: el parámetro correcto es @Parque (el anterior era @FrenteParque, incorrecto)
                    cmd.Parameters.Add(new SqlParameter("@Parque",       SqlDbType.Bit)          { Value = chkParque.Checked });
                    cmd.Parameters.Add(new SqlParameter("@CalleCerrada", SqlDbType.Bit)          { Value = chkCalleCerrada.Checked });
                    conn.Open(); cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Lote creado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void Warn(string m) => MessageBox.Show(m, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        private class CbItem { public string Text { get; set; } public int Value { get; set; } public override string ToString() => Text; }
    }
}

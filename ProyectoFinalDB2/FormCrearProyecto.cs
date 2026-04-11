using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ProyectoFinalDB2
{
    public class FormCrearProyecto : Form
    {
        private TextBox txtNombre, txtDepto, txtMuni;
        private NumericUpDown numPlazo;

        private static readonly Color CPrimario  = Color.FromArgb(26, 26, 46);
        private static readonly Color CAccent    = Color.FromArgb(67, 97, 238);
        private static readonly Color CFondo     = Color.FromArgb(247, 248, 252);
        private static readonly Color CBorder    = Color.FromArgb(210, 215, 230);
        private static readonly Color CTexto     = Color.FromArgb(40, 50, 70);
        private static readonly Color CSubtexto  = Color.FromArgb(120, 130, 155);

        public FormCrearProyecto()
        {
            Text = "Nuevo Proyecto Inmobiliario";
            Size = new Size(480, 545);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = CFondo;
            Font = new Font("Segoe UI", 9);
            Build();
        }

        private void Build()
        {
            // ── Header ──
            var hdr = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = CPrimario };
            hdr.Controls.Add(new Label { Text = "REGISTRAR PROYECTO", ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true, Location = new Point(24, 12) });
            hdr.Controls.Add(new Label { Text = "Complete los datos del nuevo proyecto inmobiliario",
                ForeColor = Color.FromArgb(175, 185, 215), Font = new Font("Segoe UI", 8),
                AutoSize = true, Location = new Point(26, 42) });
            Controls.Add(hdr);

            // ── Fields ──
            int x = 34, y = 94, w = 380;

            Lbl("Nombre del Proyecto *", x, y);
            txtNombre = Txt(x, y + 22, w); y += 68;

            Lbl("Departamento *", x, y);
            txtDepto = Txt(x, y + 22, w); y += 68;

            Lbl("Municipio *", x, y);
            txtMuni = Txt(x, y + 22, w); y += 68;

            Lbl("Plazo Máximo (meses) *", x, y);
            numPlazo = new NumericUpDown { Location = new Point(x, y + 22), Width = w,
                Minimum = 1, Maximum = 480, Value = 120,
                Font = new Font("Segoe UI", 9), BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(numPlazo);
            Controls.Add(new Label { Text = "Ej: 120 = 10 años · 240 = 20 años · 360 = 30 años",
                Location = new Point(x, y + 48), AutoSize = true,
                Font = new Font("Segoe UI", 7.5f), ForeColor = CSubtexto });
            y += 102;

            // ── Buttons ──
            var btnOk = Btn("REGISTRAR PROYECTO", x, y, w, CAccent, Color.White);
            btnOk.Click += Save;
            var btnCx = Btn("CANCELAR", x, y + 50, w, CFondo, CTexto);
            btnCx.FlatAppearance.BorderColor = CBorder;
            btnCx.FlatAppearance.BorderSize = 1;
            btnCx.DialogResult = DialogResult.Cancel;

            AcceptButton = btnOk;
            CancelButton = btnCx;
            Controls.Add(btnOk);
            Controls.Add(btnCx);
        }

        private void Lbl(string t, int x, int y) =>
            Controls.Add(new Label { Text = t, Location = new Point(x, y), AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(55, 65, 90) });

        private TextBox Txt(int x, int y, int w)
        {
            var t = new TextBox { Location = new Point(x, y), Width = w,
                Font = new Font("Segoe UI", 9), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            Controls.Add(t); return t;
        }

        private Button Btn(string text, int x, int y, int w, Color back, Color fore)
        {
            var b = new Button { Text = text, Location = new Point(x, y), Size = new Size(w, 40),
                BackColor = back, ForeColor = fore, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void Save(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            { Warn("El nombre del proyecto es obligatorio."); txtNombre.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtDepto.Text))
            { Warn("El departamento es obligatorio."); txtDepto.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtMuni.Text))
            { Warn("El municipio es obligatorio."); txtMuni.Focus(); return; }

            try
            {
                string cs = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString;
                if (string.IsNullOrEmpty(cs)) { Err("Cadena de conexión no encontrada."); return; }
                using (var conn = new SqlConnection(cs))
                using (var cmd = new SqlCommand("spCrearProyecto", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Nombre",      SqlDbType.VarChar, 100) { Value = txtNombre.Text.Trim() });
                    cmd.Parameters.Add(new SqlParameter("@Departamento", SqlDbType.VarChar, 50)  { Value = txtDepto.Text.Trim() });
                    cmd.Parameters.Add(new SqlParameter("@Municipio",   SqlDbType.VarChar, 50)  { Value = txtMuni.Text.Trim() });
                    cmd.Parameters.Add(new SqlParameter("@PlazoMaximo", SqlDbType.Int)           { Value = (int)numPlazo.Value });
                    conn.Open(); cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Proyecto registrado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { Err(ex.Message); }
        }

        private void Warn(string m) => MessageBox.Show(m, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        private void Err(string m)  => MessageBox.Show(m, "Error",      MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
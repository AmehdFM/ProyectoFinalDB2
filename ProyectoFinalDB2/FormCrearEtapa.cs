using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    public class FormCrearEtapa : Form
    {
        private readonly int proyectoId;
        private readonly string connStr;

        private TextBox       txtNombre;
        private NumericUpDown nudPrecioVara, nudInteres, nudAreaVerde;
        private Label         lblPorcentaje;

        private static readonly Color CPrimario = Color.FromArgb(26, 26, 46);
        private static readonly Color CAccent   = Color.FromArgb(67, 97, 238);
        private static readonly Color CFondo    = Color.FromArgb(247, 248, 252);
        private static readonly Color CBorder   = Color.FromArgb(210, 215, 230);
        private static readonly Color CSub      = Color.FromArgb(120, 130, 155);

        public FormCrearEtapa(int proyectoId, string connStr)
        {
            this.proyectoId = proyectoId;
            this.connStr    = connStr;
            Text = "Nueva Etapa";
            Size = new Size(460, 490);
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
            var hdr = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = CPrimario };
            hdr.Controls.Add(new Label { Text = "CREAR ETAPA", ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true, Location = new Point(22, 12) });
            hdr.Controls.Add(new Label { Text = "Define los parámetros financieros de la nueva etapa",
                ForeColor = Color.FromArgb(170, 185, 215), Font = new Font("Segoe UI", 8),
                AutoSize = true, Location = new Point(24, 42) });
            Controls.Add(hdr);

            int x = 30, y = 88, w = 368;

            Lbl("Nombre de la Etapa *", x, y);
            txtNombre = Txt(x, y + 22, w); y += 66;

            Lbl("Precio por Vara Cuadrada (L.) *", x, y);
            nudPrecioVara = new NumericUpDown { Location = new Point(x, y + 22), Width = w,
                DecimalPlaces = 2, Minimum = 0.01m, Maximum = 9999999m,
                Font = new Font("Segoe UI", 9) };
            Controls.Add(nudPrecioVara); y += 66;

            Lbl("Tasa de Interés Anual (%)", x, y);
            nudInteres = new NumericUpDown { Location = new Point(x, y + 22), Width = w,
                DecimalPlaces = 2, Minimum = 0, Maximum = 100,
                Font = new Font("Segoe UI", 9) };
            Controls.Add(nudInteres);
            Controls.Add(new Label { Text = "0 % = ventas de contado sin cargo financiero",
                Location = new Point(x, y + 48), AutoSize = true,
                Font = new Font("Segoe UI", 7.5f), ForeColor = CSub }); y += 80;

            Lbl("Área Verde  (proporción 0.00 – 1.00)", x, y);
            nudAreaVerde = new NumericUpDown { Location = new Point(x, y + 22), Width = w - 90,
                DecimalPlaces = 2, Minimum = 0, Maximum = 1,
                Font = new Font("Segoe UI", 9) };
            nudAreaVerde.ValueChanged += (s, _) =>
                lblPorcentaje.Text = $"= {nudAreaVerde.Value * 100:N1} %";
            Controls.Add(nudAreaVerde);

            lblPorcentaje = new Label { Text = "= 0.0 %",
                Location = new Point(x + nudAreaVerde.Width + 10, y + 25),
                AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = CAccent };
            Controls.Add(lblPorcentaje); y += 74;

            // ── Buttons ──
            var btnOk = Btn("CREAR ETAPA", x, y, w, CAccent, Color.White);
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
            var t = new TextBox { Location = new Point(x, y), Width = w,
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

        private void Save(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            { Warn("El nombre de la etapa es obligatorio."); txtNombre.Focus(); return; }
            if (nudPrecioVara.Value <= 0)
            { Warn("El precio por vara debe ser mayor a 0."); return; }

            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("sp_CrearEtapa", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ProyectoID", SqlDbType.Int)          { Value = proyectoId });
                    cmd.Parameters.Add(new SqlParameter("@Nombre",     SqlDbType.VarChar, 100)  { Value = txtNombre.Text.Trim() });
                    cmd.Parameters.Add(new SqlParameter("@PrecioVara", SqlDbType.Float)         { Value = (double)nudPrecioVara.Value });
                    cmd.Parameters.Add(new SqlParameter("@Interes",    SqlDbType.Float)         { Value = (double)nudInteres.Value });
                    cmd.Parameters.Add(new SqlParameter("@AreaVerde",  SqlDbType.Float)         { Value = (double)nudAreaVerde.Value });
                    conn.Open(); cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Etapa creada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void Warn(string m) => MessageBox.Show(m, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
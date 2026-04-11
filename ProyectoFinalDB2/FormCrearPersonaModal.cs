using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    public enum TipoPersona { Cliente, Aval, Beneficiario }

    public class FormCrearPersonaModal : Form
    {
        private static readonly Color CPrimario = Color.FromArgb(26, 26, 46);
        private static readonly Color CAccent   = Color.FromArgb(67, 97, 238);
        private static readonly Color CSuccess  = Color.FromArgb(39, 174, 96);
        private static readonly Color CFondo    = Color.White;

        private readonly string connStr;
        private readonly TipoPersona tipo;

        private TextBox txtNombre, txtDNI, txtTel;
        private TextBox txtExtraStr; // Trabajo (Cliente/Aval) o Parentesco (Beneficiario)
        private NumericUpDown nudSueldo; // Solo para Cliente/Aval
        private Label lblSueldo;

        public int NuevoID { get; private set; }
        public string NuevoNombre { get; private set; }

        public FormCrearPersonaModal(string connStr, TipoPersona tipo)
        {
            this.connStr = connStr;
            this.tipo = tipo;

            Text = "Registrar " + tipo.ToString();
            Size = new Size(400, tipo == TipoPersona.Beneficiario ? 450 : 520);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = CFondo;
            Font = new Font("Segoe UI", 9);

            BuildUI();
        }

        private void BuildUI()
        {
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = CPrimario };
            pnlHeader.Controls.Add(new Label {
                Text = "REGISTRAR NUEVO " + tipo.ToString().ToUpper(),
                ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true, Location = new Point(20, 18)
            });
            Controls.Add(pnlHeader);

            int y = 80;
            int x = 30;

            Lbl("Nombre completo *", x, y);
            txtNombre = Txt(x, y + 20); y += 65;

            Lbl("DNI (Identidad) *", x, y);
            txtDNI = Txt(x, y + 20); txtDNI.MaxLength = 15; y += 65;

            Lbl("Teléfono *", x, y);
            txtTel = Txt(x, y + 20); txtTel.MaxLength = 15; y += 65;

            string labelExtra = tipo == TipoPersona.Beneficiario ? "Parentesco (Ej. Hijo, Conyuge) *" : "Lugar de trabajo *";
            Lbl(labelExtra, x, y);
            txtExtraStr = Txt(x, y + 20); y += 65;

            if (tipo != TipoPersona.Beneficiario)
            {
                lblSueldo = new Label { Text = "Sueldo mensual (L.) *", Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
                Controls.Add(lblSueldo);
                nudSueldo = new NumericUpDown {
                    Location = new Point(x, y + 20), Width = 320, DecimalPlaces = 2,
                    Minimum = 0.01m, Maximum = 9999999m, Value = 0.01m, Font = new Font("Segoe UI", 10)
                };
                Controls.Add(nudSueldo);
                y += 75;
            }

            var btnGuardar = new Button {
                Text = "✔ GUARDAR", Location = new Point(x, y), Size = new Size(150, 40),
                BackColor = CSuccess, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            var btnCancelar = new Button {
                Text = "Cancelar", Location = new Point(x + 170, y), Size = new Size(150, 40),
                BackColor = Color.FromArgb(240, 240, 240), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand, DialogResult = DialogResult.Cancel
            };
            btnCancelar.FlatAppearance.BorderSize = 0;

            Controls.Add(btnGuardar);
            Controls.Add(btnCancelar);
        }

        private void Lbl(string text, int x, int y)
        {
            Controls.Add(new Label { Text = text, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) });
        }

        private TextBox Txt(int x, int y)
        {
            var t = new TextBox { Location = new Point(x, y), Width = 320, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(t);
            return t;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtDNI.Text) ||
                string.IsNullOrWhiteSpace(txtTel.Text) || string.IsNullOrWhiteSpace(txtExtraStr.Text))
            {
                MessageBox.Show("Por favor, complete todos los campos obligatorios.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    string sp = "";
                    if (tipo == TipoPersona.Cliente) sp = "sp_InsertarCliente";
                    else if (tipo == TipoPersona.Aval) sp = "sp_InsertarAval";
                    else if (tipo == TipoPersona.Beneficiario) sp = "sp_InsertarBeneficiario";

                    using (var cmd = new SqlCommand(sp, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.VarChar, 100) { Value = txtNombre.Text.Trim() });
                        cmd.Parameters.Add(new SqlParameter("@DNI", SqlDbType.VarChar, 15) { Value = txtDNI.Text.Trim() });
                        cmd.Parameters.Add(new SqlParameter("@Telefono", SqlDbType.VarChar, 15) { Value = txtTel.Text.Trim() });

                        if (tipo == TipoPersona.Beneficiario)
                        {
                            cmd.Parameters.Add(new SqlParameter("@Parentesco", SqlDbType.VarChar, 50) { Value = txtExtraStr.Text.Trim() });
                        }
                        else
                        {
                            cmd.Parameters.Add(new SqlParameter("@Trabajo", SqlDbType.VarChar, 100) { Value = txtExtraStr.Text.Trim() });
                            cmd.Parameters.Add(new SqlParameter("@Sueldo", SqlDbType.Float) { Value = (double)nudSueldo.Value });
                        }

                        conn.Open();
                        NuevoID = Convert.ToInt32(cmd.ExecuteScalar());
                        NuevoNombre = txtNombre.Text.Trim();
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

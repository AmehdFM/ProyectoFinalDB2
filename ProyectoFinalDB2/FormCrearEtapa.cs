using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    public class FormCrearEtapa : Form
    {
        private int proyectoId;
        private string connStr;

        private TextBox txtNombre;
        private NumericUpDown nudPrecioVara;
        private NumericUpDown nudInteres;
        private NumericUpDown nudAreaVerde;

        public FormCrearEtapa(int proyectoId, string connStr)
        {
            this.proyectoId = proyectoId;
            this.connStr = connStr;

            this.Text = "Crear Etapa";
            this.Size = new Size(420, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Font = new Font("Segoe UI", 9);

            ConfigureControls();
        }

        private void ConfigureControls()
        {
            int x = 24, y = 18, w = 340;

            this.Controls.Add(new Label { Text = "Nombre de la Etapa", Location = new Point(x, y), AutoSize = true });
            txtNombre = new TextBox { Location = new Point(x, y + 20), Width = w };
            this.Controls.Add(txtNombre);

            y += 60;
            this.Controls.Add(new Label { Text = "Precio por Vara (L.)", Location = new Point(x, y), AutoSize = true });
            nudPrecioVara = new NumericUpDown { Location = new Point(x, y + 20), Width = w, DecimalPlaces = 2, Maximum = 9999999, Minimum = 0.01m };
            this.Controls.Add(nudPrecioVara);

            y += 60;
            this.Controls.Add(new Label { Text = "Interés (%)", Location = new Point(x, y), AutoSize = true });
            nudInteres = new NumericUpDown { Location = new Point(x, y + 20), Width = w, DecimalPlaces = 2, Maximum = 100, Minimum = 0 };
            this.Controls.Add(nudInteres);

            y += 60;
            this.Controls.Add(new Label { Text = "Área Verde (0.00 - 1.00)", Location = new Point(x, y), AutoSize = true });
            nudAreaVerde = new NumericUpDown { Location = new Point(x, y + 20), Width = w, DecimalPlaces = 2, Maximum = 1, Minimum = 0 };
            this.Controls.Add(nudAreaVerde);

            y += 70;
            Button btnGuardar = new Button
            {
                Text = "CREAR ETAPA",
                Location = new Point(x, y),
                Size = new Size(w, 38),
                BackColor = Color.FromArgb(26, 26, 46),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);

            Button btnCancelar = new Button
            {
                Text = "CANCELAR",
                Location = new Point(x, y + 46),
                Size = new Size(w, 30),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            btnCancelar.FlatAppearance.BorderSize = 1;
            this.Controls.Add(btnCancelar);

            this.AcceptButton = btnGuardar;
            this.CancelButton = btnCancelar;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre de la etapa es obligatorio.");
                return;
            }

            if (nudPrecioVara.Value <= 0)
            {
                MessageBox.Show("El precio por vara debe ser mayor a 0.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand("sp_CrearEtapa", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ProyectoID", SqlDbType.Int) { Value = proyectoId });
                    cmd.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.VarChar, 100) { Value = txtNombre.Text.Trim() });
                    cmd.Parameters.Add(new SqlParameter("@PrecioVara", SqlDbType.Float) { Value = (double)nudPrecioVara.Value });
                    cmd.Parameters.Add(new SqlParameter("@Interes", SqlDbType.Float) { Value = (double)nudInteres.Value });
                    cmd.Parameters.Add(new SqlParameter("@AreaVerde", SqlDbType.Float) { Value = (double)nudAreaVerde.Value });

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Etapa creada correctamente.");
                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creando etapa: " + ex.Message);
            }
        }
    }
}
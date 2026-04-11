using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    public class FormCrearBloque : Form
    {
        private int proyectoId;
        private string connStr;

        private ComboBox cmbEtapas;
        private NumericUpDown nudNumeroBloque;

        public FormCrearBloque(int proyectoId, string connStr)
        {
            this.proyectoId = proyectoId;
            this.connStr = connStr;

            this.Text = "Crear Bloque";
            this.Size = new Size(420, 260);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Font = new Font("Segoe UI", 9);

            ConfigureControls();
            LoadEtapas();
        }

        private void ConfigureControls()
        {
            int x = 24, y = 18, w = 340;

            this.Controls.Add(new Label { Text = "Etapa", Location = new Point(x, y), AutoSize = true });
            cmbEtapas = new ComboBox { Location = new Point(x, y + 20), Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbEtapas);

            y += 60;
            this.Controls.Add(new Label { Text = "Número de Bloque", Location = new Point(x, y), AutoSize = true });
            nudNumeroBloque = new NumericUpDown { Location = new Point(x, y + 20), Width = w, Minimum = 1, Maximum = 100000 };
            this.Controls.Add(nudNumeroBloque);

            y += 70;
            Button btnGuardar = new Button
            {
                Text = "CREAR BLOQUE",
                Location = new Point(x, y),
                Size = new Size(w, 36),
                BackColor = Color.FromArgb(26, 26, 46),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);

            Button btnCancelar = new Button { Text = "CANCELAR", Location = new Point(x, y + 44), Size = new Size(w, 30), DialogResult = DialogResult.Cancel };
            this.Controls.Add(btnCancelar);

            this.AcceptButton = btnGuardar;
            this.CancelButton = btnCancelar;
        }

        private void LoadEtapas()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "select EtapaID, Nombre from Etapa where ProyectoID = @pid order by Nombre";
                    cmd.Parameters.AddWithValue("@pid", proyectoId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            cmbEtapas.Items.Add(new ComboboxItem
                            {
                                Text = reader["Nombre"]?.ToString(),
                                Value = Convert.ToInt32(reader["EtapaID"])
                            });
                    }
                }
                if (cmbEtapas.Items.Count > 0) cmbEtapas.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando etapas: " + ex.Message);
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (cmbEtapas.SelectedItem == null)
            {
                MessageBox.Show("Seleccione una etapa.");
                return;
            }

            int etapaId = ((ComboboxItem)cmbEtapas.SelectedItem).Value;
            int numero = (int)nudNumeroBloque.Value;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand("sp_CrearBloque", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@EtapaID", SqlDbType.Int) { Value = etapaId });
                    cmd.Parameters.Add(new SqlParameter("@NumeroBloque", SqlDbType.Int) { Value = numero });

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Bloque creado correctamente.");
                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creando bloque: " + ex.Message);
            }
        }

        private class ComboboxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }
            public override string ToString() => Text;
        }
    }
}
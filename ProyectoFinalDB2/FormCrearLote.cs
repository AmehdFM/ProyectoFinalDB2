using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    public class FormCrearLote : Form
    {
        private int proyectoId;
        private string connStr;

        private ComboBox cmbBloques;
        private NumericUpDown nudNumero;
        private NumericUpDown nudArea;
        private TextBox txtCatastro;
        private TextBox txtMatricula;
        private TextBox txtColindancias;
        private CheckBox chkEsquina;
        private CheckBox chkParque;
        private CheckBox chkCalleCerrada;

        public FormCrearLote(int proyectoId, string connStr)
        {
            this.proyectoId = proyectoId;
            this.connStr = connStr;

            this.Text = "Crear Lote";
            this.Size = new Size(420, 620);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Font = new Font("Segoe UI", 9);

            ConfigureControls();
            LoadBloques();
        }

        private void ConfigureControls()
        {
            int x = 24, y = 18, w = 340;

            this.Controls.Add(new Label { Text = "Bloque", Location = new Point(x, y), AutoSize = true });
            cmbBloques = new ComboBox { Location = new Point(x, y + 20), Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbBloques);

            y += 60;
            this.Controls.Add(new Label { Text = "Número de Lote", Location = new Point(x, y), AutoSize = true });
            nudNumero = new NumericUpDown { Location = new Point(x, y + 20), Width = w, Minimum = 1, Maximum = 100000 };
            this.Controls.Add(nudNumero);

            y += 60;
            this.Controls.Add(new Label { Text = "Área (v²)", Location = new Point(x, y), AutoSize = true });
            nudArea = new NumericUpDown { Location = new Point(x, y + 20), Width = w, DecimalPlaces = 2, Minimum = 0.01m, Maximum = 9999999 };
            this.Controls.Add(nudArea);

            y += 60;
            this.Controls.Add(new Label { Text = "Catastro", Location = new Point(x, y), AutoSize = true });
            txtCatastro = new TextBox { Location = new Point(x, y + 20), Width = w, MaxLength = 50 };
            this.Controls.Add(txtCatastro);

            y += 60;
            this.Controls.Add(new Label { Text = "Matrícula", Location = new Point(x, y), AutoSize = true });
            txtMatricula = new TextBox { Location = new Point(x, y + 20), Width = w, MaxLength = 50 };
            this.Controls.Add(txtMatricula);

            y += 60;
            this.Controls.Add(new Label { Text = "Colindancias", Location = new Point(x, y), AutoSize = true });
            txtColindancias = new TextBox
            {
                Location = new Point(x, y + 20),
                Width = w,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtColindancias);

            y += 90;
            chkEsquina = new CheckBox { Text = "Lote en Esquina", Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(chkEsquina);

            y += 28;
            chkParque = new CheckBox { Text = "Frente a Parque", Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(chkParque);

            y += 28;
            chkCalleCerrada = new CheckBox { Text = "Calle Cerrada", Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(chkCalleCerrada);

            y += 46;
            Button btnGuardar = new Button
            {
                Text = "CREAR LOTE",
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

        private void LoadBloques()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "select B.BloqueID, B.NumeroBloque, E.Nombre as Etapa from Bloque B join Etapa E on B.EtapaID = E.EtapaID where E.ProyectoID = @pid order by E.Nombre, B.NumeroBloque";
                    cmd.Parameters.AddWithValue("@pid", proyectoId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            cmbBloques.Items.Add(new ComboboxItem
                            {
                                Text = $"Etapa {reader["Etapa"]} — Bloque {reader["NumeroBloque"]}",
                                Value = Convert.ToInt32(reader["BloqueID"])
                            });
                    }
                }
                if (cmbBloques.Items.Count > 0) cmbBloques.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando bloques: " + ex.Message);
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (cmbBloques.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un bloque.");
                return;
            }
            if (nudArea.Value <= 0)
            {
                MessageBox.Show("El área debe ser mayor a 0.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtCatastro.Text))
            {
                MessageBox.Show("El catastro es obligatorio.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtMatricula.Text))
            {
                MessageBox.Show("La matrícula es obligatoria.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtColindancias.Text))
            {
                MessageBox.Show("Las colindancias son obligatorias.");
                return;
            }

            int bloqueId = ((ComboboxItem)cmbBloques.SelectedItem).Value;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand("sp_CrearLote", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@BloqueID", SqlDbType.Int) { Value = bloqueId });
                    cmd.Parameters.Add(new SqlParameter("@Numero", SqlDbType.Int) { Value = (int)nudNumero.Value });
                    cmd.Parameters.Add(new SqlParameter("@Area", SqlDbType.Float) { Value = (double)nudArea.Value });
                    cmd.Parameters.Add(new SqlParameter("@Catastro", SqlDbType.VarChar, 50) { Value = txtCatastro.Text.Trim() });
                    cmd.Parameters.Add(new SqlParameter("@Matricula", SqlDbType.VarChar, 50) { Value = txtMatricula.Text.Trim() });
                    cmd.Parameters.Add(new SqlParameter("@Colindancias", SqlDbType.VarChar, -1) { Value = txtColindancias.Text.Trim() });
                    cmd.Parameters.Add(new SqlParameter("@Esquina", SqlDbType.Bit) { Value = chkEsquina.Checked });
                    cmd.Parameters.Add(new SqlParameter("@FrenteParque", SqlDbType.Bit) { Value = chkParque.Checked });
                    cmd.Parameters.Add(new SqlParameter("@CalleCerrada", SqlDbType.Bit) { Value = chkCalleCerrada.Checked });

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Lote creado correctamente.");
                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creando lote: " + ex.Message);
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

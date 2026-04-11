using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    public class FormCrearVenta : Form
    {
        private int loteId;
        private string connStr;

        private ComboBox cmbCliente;
        private ComboBox cmbEmpleado;
        private ComboBox cmbAval;
        private ComboBox cmbBeneficiario;
        private ComboBox cmbTipo;
        private NumericUpDown nudPrima;
        private NumericUpDown nudPlazo;
        private NumericUpDown nudInteres;
        private Label lblValorLote;

        private Color ColorPrimario = Color.FromArgb(26, 26, 46);
        private Color ColorAccent = Color.FromArgb(39, 174, 96);

        public FormCrearVenta(int loteId, string connStr)
        {
            this.loteId = loteId;
            this.connStr = connStr;

            this.Text = "Registrar Venta";
            this.Size = new Size(460, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Font = new Font("Segoe UI", 9);

            ConfigureControls();
            CargarCombos();
            MostrarValorLote();
        }

        private void ConfigureControls()
        {
            int x = 24, y = 14, w = 380;

            lblValorLote = new Label
            {
                Text = "Valor del lote: calculando...",
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorAccent
            };
            this.Controls.Add(lblValorLote);

            y += 30;
            this.Controls.Add(new Label { Text = "Cliente", Location = new Point(x, y), AutoSize = true });
            cmbCliente = new ComboBox { Location = new Point(x, y + 18), Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbCliente);

            y += 56;
            this.Controls.Add(new Label { Text = "Empleado (Vendedor)", Location = new Point(x, y), AutoSize = true });
            cmbEmpleado = new ComboBox { Location = new Point(x, y + 18), Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbEmpleado);

            y += 56;
            this.Controls.Add(new Label { Text = "Aval (opcional)", Location = new Point(x, y), AutoSize = true });
            cmbAval = new ComboBox { Location = new Point(x, y + 18), Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbAval);

            y += 56;
            this.Controls.Add(new Label { Text = "Beneficiario (opcional)", Location = new Point(x, y), AutoSize = true });
            cmbBeneficiario = new ComboBox { Location = new Point(x, y + 18), Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbBeneficiario);

            y += 56;
            this.Controls.Add(new Label { Text = "Tipo de Venta", Location = new Point(x, y), AutoSize = true });
            cmbTipo = new ComboBox { Location = new Point(x, y + 18), Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTipo.Items.AddRange(new object[] { "Contado", "Credito" });
            cmbTipo.SelectedIndex = 1;
            cmbTipo.SelectedIndexChanged += (s, e) => ToggleCamposCredito();
            this.Controls.Add(cmbTipo);

            y += 56;
            this.Controls.Add(new Label { Text = "Prima (L.)", Location = new Point(x, y), AutoSize = true });
            nudPrima = new NumericUpDown { Location = new Point(x, y + 18), Width = w, DecimalPlaces = 2, Minimum = 0, Maximum = 9999999 };
            this.Controls.Add(nudPrima);

            y += 56;
            this.Controls.Add(new Label { Text = "Plazo (años)", Location = new Point(x, y), AutoSize = true });
            nudPlazo = new NumericUpDown { Location = new Point(x, y + 18), Width = w, Minimum = 1, Maximum = 30, Value = 1 };
            this.Controls.Add(nudPlazo);

            y += 56;
            this.Controls.Add(new Label { Text = "Interés Anual (%)", Location = new Point(x, y), AutoSize = true });
            nudInteres = new NumericUpDown { Location = new Point(x, y + 18), Width = w, DecimalPlaces = 2, Minimum = 0, Maximum = 100 };
            this.Controls.Add(nudInteres);

            y += 64;
            Button btnGuardar = new Button
            {
                Text = "REGISTRAR VENTA",
                Location = new Point(x, y),
                Size = new Size(w, 38),
                BackColor = ColorAccent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
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

        private void ToggleCamposCredito()
        {
            bool esCredito = cmbTipo.SelectedItem?.ToString() == "Credito";
            nudPlazo.Enabled = esCredito;
            nudInteres.Enabled = esCredito;
            if (!esCredito)
            {
                nudPlazo.Value = 1;
                nudInteres.Value = 0;
                nudPrima.Value = 0;
            }
        }

        private void MostrarValorLote()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "select dbo.fnValorLote(@LoteID)";
                    cmd.Parameters.AddWithValue("@LoteID", loteId);
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    double valor = result != null ? Convert.ToDouble(result) : 0;
                    lblValorLote.Text = $"Valor del lote: L. {valor:N2}";
                }
            }
            catch
            {
                lblValorLote.Text = "Valor del lote: no disponible";
            }
        }

        private void CargarCombos()
        {
            CargarCombo(cmbCliente, "select ClienteID, Nombre from Cliente order by Nombre", "ClienteID", "Nombre");
            CargarCombo(cmbEmpleado, "select EmpleadoID, Nombre from Empleado order by Nombre", "EmpleadoID", "Nombre");
            CargarCombo(cmbAval, "select AvalID, Nombre from Aval order by Nombre", "AvalID", "Nombre", nullable: true);
            CargarCombo(cmbBeneficiario, "select BeneficiarioID, Nombre from Beneficiario order by Nombre", "BeneficiarioID", "Nombre", nullable: true);
        }

        private void CargarCombo(ComboBox cmb, string query, string colId, string colNombre, bool nullable = false)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (nullable)
                            cmb.Items.Add(new ComboboxItem { Text = "(Ninguno)", Value = -1 });

                        while (reader.Read())
                            cmb.Items.Add(new ComboboxItem
                            {
                                Text = reader[colNombre]?.ToString(),
                                Value = Convert.ToInt32(reader[colId])
                            });
                    }
                }
                if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando datos: " + ex.Message);
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (cmbCliente.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un cliente.");
                return;
            }
            if (cmbEmpleado.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un empleado.");
                return;
            }
            if (cmbTipo.SelectedItem == null)
            {
                MessageBox.Show("Seleccione el tipo de venta.");
                return;
            }

            string tipo = cmbTipo.SelectedItem.ToString();
            bool esCredito = tipo == "Credito";

            if (esCredito && nudInteres.Value <= 0)
            {
                MessageBox.Show("Ingrese una tasa de interés mayor a 0 para ventas a crédito.");
                return;
            }

            int clienteId = ((ComboboxItem)cmbCliente.SelectedItem).Value;
            int empleadoId = ((ComboboxItem)cmbEmpleado.SelectedItem).Value;
            var avalItem = (ComboboxItem)cmbAval.SelectedItem;
            var benefItem = (ComboboxItem)cmbBeneficiario.SelectedItem;
            int? avalId = (avalItem == null || avalItem.Value == -1) ? (int?)null : avalItem.Value;
            int? benefId = (benefItem == null || benefItem.Value == -1) ? (int?)null : benefItem.Value;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand("sp_CrearVenta", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@LoteID", SqlDbType.Int) { Value = loteId });
                    cmd.Parameters.Add(new SqlParameter("@ClienteID", SqlDbType.Int) { Value = clienteId });
                    cmd.Parameters.Add(new SqlParameter("@EmpleadoID", SqlDbType.Int) { Value = empleadoId });
                    cmd.Parameters.Add(new SqlParameter("@AvalID", SqlDbType.Int) { Value = avalId.HasValue ? (object)avalId.Value : DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@BeneficiarioID", SqlDbType.Int) { Value = benefId.HasValue ? (object)benefId.Value : DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@Tipo", SqlDbType.VarChar, 10) { Value = tipo });
                    cmd.Parameters.Add(new SqlParameter("@Prima", SqlDbType.Float) { Value = (double)nudPrima.Value });
                    cmd.Parameters.Add(new SqlParameter("@Plazo", SqlDbType.Int) { Value = (int)nudPlazo.Value });
                    cmd.Parameters.Add(new SqlParameter("@Interes", SqlDbType.Float) { Value = (double)nudInteres.Value });

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Venta registrada correctamente. El plan de pago fue generado automáticamente.");
                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error registrando venta: " + ex.Message);
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
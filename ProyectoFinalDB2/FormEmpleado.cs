using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    public class FormEmpleado : Form
    {
        private TextBox txtNombre;
        private TextBox txtCargo;
        private Button btnGuardar;
        private int? empleadoId;

        public int? EmpleadoId => empleadoId;
        public string EmpleadoNombre => txtNombre?.Text;
        public string EmpleadoCargo => txtCargo?.Text;

        public FormEmpleado(string titulo) : this(titulo, null, null, null) { }

        public FormEmpleado(int id, string nombre, string cargo) : this("Editar Empleado", id, nombre, cargo) { }

        private FormEmpleado(string titulo, int? id, string nombre, string cargo)
        {
            this.Text = titulo;
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.empleadoId = id;
            ConfigurarCampos(nombre, cargo);
        }

        private void ConfigurarCampos(string nombreInicial, string cargoInicial)
        {
            int y = 20;

            Label lblNombre = new Label { Text = "Nombre:", Location = new Point(30, y), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtNombre = new TextBox { Location = new Point(30, y + 25), Size = new Size(320, 25), Font = new Font("Segoe UI", 10) };
            y += 65;

            Label lblCargo = new Label { Text = "Cargo:", Location = new Point(30, y), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtCargo = new TextBox { Location = new Point(30, y + 25), Size = new Size(320, 25), Font = new Font("Segoe UI", 10) };
            y += 65;

            if (!string.IsNullOrEmpty(nombreInicial)) txtNombre.Text = nombreInicial;
            if (!string.IsNullOrEmpty(cargoInicial)) txtCargo.Text = cargoInicial;

            btnGuardar = new Button
            {
                Text = "Guardar",
                Location = new Point(110, y),
                Size = new Size(160, 36),
                BackColor = Color.FromArgb(44, 62, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            btnGuardar.Click += (s, e) => {
                // Validaciones simples
                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MessageBox.Show("El nombre es requerido.");
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtCargo.Text))
                {
                    MessageBox.Show("El cargo es requerido.");
                    return;
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            this.Controls.Add(lblNombre);
            this.Controls.Add(txtNombre);
            this.Controls.Add(lblCargo);
            this.Controls.Add(txtCargo);
            this.Controls.Add(btnGuardar);
        }

        public void SetReadOnly(bool readOnly)
        {
            if (txtNombre != null) txtNombre.ReadOnly = readOnly;
            if (txtCargo != null) txtCargo.ReadOnly = readOnly;
            if (btnGuardar != null) btnGuardar.Visible = !readOnly;
        }
    }
}
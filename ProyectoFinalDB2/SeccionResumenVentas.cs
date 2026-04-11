using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;

namespace ProyectoFinalDB2
{
    public class SeccionResumenVentas : Form
    {
        private static readonly Color CPrimario   = Color.FromArgb(15, 23, 42);
        private static readonly Color CAccent     = Color.FromArgb(67, 97, 238);
        private static readonly Color CFondo      = Color.FromArgb(241, 245, 249);
        private static readonly Color CCard       = Color.White;
        private static readonly Color CTexto      = Color.FromArgb(15, 23, 42);
        private static readonly Color CSub        = Color.FromArgb(100, 116, 139);
        private static readonly Color CBorder     = Color.FromArgb(226, 232, 240);
        private static readonly Color CHeader     = Color.FromArgb(248, 250, 252);
        private static readonly Color CSeleccion  = Color.FromArgb(235, 241, 255);

        private DataGridView dgvResumen;
        private string connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString;

        public SeccionResumenVentas()
        {
            this.Text = "Rendimiento de Ventas";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = CFondo;
            this.Font = new Font("Segoe UI", 9);

            BuildUI();
            CargarResumen();
        }

        private void BuildUI()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = CCard };
            pnlTop.Paint += (s, e) => e.Graphics.DrawLine(new Pen(CBorder), 0, 79, pnlTop.Width, 79);

            var lblBread = new Label { Text = "Financiero  ›  Rendimiento Comercial", Font = new Font("Segoe UI", 7.5f), ForeColor = CSub, Location = new Point(28, 16), AutoSize = true };
            var lblTitulo = new Label { Text = "Resumen de Ventas por Empleado", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = CTexto, Location = new Point(26, 34), AutoSize = true };

            var btnActualizar = new Button { 
                Text = "🔄 Sincronizar", 
                Size = new Size(140, 36), 
                Location = new Point(this.ClientSize.Width - 168, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = CAccent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.Click += (s, e) => CargarResumen();

            pnlTop.Controls.AddRange(new Control[] { lblBread, lblTitulo, btnActualizar });

            dgvResumen = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = CCard,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                GridColor = CBorder,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                RowTemplate = { Height = 50 }
            };

            dgvResumen.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = CHeader, ForeColor = CSub, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), Padding = new Padding(14, 0, 0, 0), SelectionBackColor = CHeader, SelectionForeColor = CSub };
            dgvResumen.DefaultCellStyle = new DataGridViewCellStyle { BackColor = CCard, ForeColor = CTexto, SelectionBackColor = CSeleccion, SelectionForeColor = CTexto, Padding = new Padding(14, 0, 0, 0) };
            
            dgvResumen.CellFormatting += (s, e) => {
                if (e.Value != null && dgvResumen.Columns[e.ColumnIndex].HeaderText.Contains("VALOR")) {
                    if (double.TryParse(e.Value.ToString(), out double val)) {
                        e.Value = "L. " + val.ToString("N2");
                        e.FormattingApplied = true;
                    }
                }
            };

            var pnlContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 20, 24, 24) };
            var wrapper = new Panel { Dock = DockStyle.Fill, BackColor = CCard, BorderStyle = BorderStyle.FixedSingle };
            wrapper.Controls.Add(dgvResumen);
            pnlContainer.Controls.Add(wrapper);

            this.Controls.Add(pnlContainer);
            this.Controls.Add(pnlTop);
        }

        private void CargarResumen()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    SqlDataAdapter da = new SqlDataAdapter("sp_GetResumenVentasPorEmpleado", conn);
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvResumen.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar resumen: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
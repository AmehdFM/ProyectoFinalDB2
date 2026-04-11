using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;

namespace ProyectoFinalDB2
{
    public class SeccionGastosProyecto : Form
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

        private DataGridView dgvGastos;
        private TextBox txtProyectoID;
        private string connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString;

        public SeccionGastosProyecto()
        {
            this.Text = "Consulta de Gastos";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = CFondo;
            this.Font = new Font("Segoe UI", 9);

            BuildUI();
        }

        private void BuildUI()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = CCard };
            pnlTop.Paint += (s, e) => e.Graphics.DrawLine(new Pen(CBorder), 0, 99, pnlTop.Width, 99);

            var lblBread = new Label { Text = "Financiero  ›  Buscador de Gastos", Font = new Font("Segoe UI", 7.5f), ForeColor = CSub, Location = new Point(28, 16), AutoSize = true };
            var lblTitulo = new Label { Text = "Consulta de Gastos por Proyecto", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = CTexto, Location = new Point(26, 34), AutoSize = true };

            var lblPrompt = new Label { Text = "ID PROYECTO:", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = CSub, Location = new Point(this.ClientSize.Width - 425, 42), Anchor = AnchorStyles.Top | AnchorStyles.Right, AutoSize = true };
            
            txtProyectoID = new TextBox { 
                Location = new Point(this.ClientSize.Width - 320, 38), 
                Width = 120, 
                Font = new Font("Segoe UI", 11),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle
            };

            var btnConsultar = new Button { 
                Text = "🔍 Buscar", 
                Size = new Size(110, 34), 
                Location = new Point(this.ClientSize.Width - 190, 37),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = CAccent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnConsultar.FlatAppearance.BorderSize = 0;
            btnConsultar.Click += (s, e) => CargarGastos();

            pnlTop.Controls.AddRange(new Control[] { lblBread, lblTitulo, lblPrompt, txtProyectoID, btnConsultar });

            dgvGastos = new DataGridView
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
                RowTemplate = { Height = 48 }
            };

            dgvGastos.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = CHeader, ForeColor = CSub, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), Padding = new Padding(14, 0, 0, 0), SelectionBackColor = CHeader, SelectionForeColor = CSub };
            dgvGastos.DefaultCellStyle = new DataGridViewCellStyle { BackColor = CCard, ForeColor = CTexto, SelectionBackColor = CSeleccion, SelectionForeColor = CTexto, Padding = new Padding(14, 0, 0, 0) };
            
            dgvGastos.CellFormatting += (s, e) => {
                if (e.Value != null && (dgvGastos.Columns[e.ColumnIndex].HeaderText.Contains("MONTO") || dgvGastos.Columns[e.ColumnIndex].HeaderText.Contains("TOTAL"))) {
                    if (double.TryParse(e.Value.ToString(), out double val)) {
                        e.Value = "L. " + val.ToString("N2");
                        e.FormattingApplied = true;
                    }
                }
            };

            var pnlContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 20, 24, 24) };
            var wrapper = new Panel { Dock = DockStyle.Fill, BackColor = CCard, BorderStyle = BorderStyle.FixedSingle };
            wrapper.Controls.Add(dgvGastos);
            pnlContainer.Controls.Add(wrapper);

            this.Controls.Add(pnlContainer);
            this.Controls.Add(pnlTop);
        }

        private void CargarGastos()
        {
            if (!int.TryParse(txtProyectoID.Text, out int proyectoId))
            {
                MessageBox.Show("Por favor, ingrese un ID de proyecto válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = "SELECT * FROM dbo.fnGastosProyecto(@ProyectoID)";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ProyectoID", proyectoId);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvGastos.DataSource = dt;
                    
                    if (dt.Rows.Count == 0)
                        MessageBox.Show("No se encontraron gastos registrados para el proyecto ID: " + proyectoId, "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar gastos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
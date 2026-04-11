using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    public class SeccionFinanciero : Form
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
        private static readonly Color CPositivo   = Color.FromArgb(16, 185, 129);
        private static readonly Color CNegativo   = Color.FromArgb(239, 68, 68);

        private readonly string connStr;
        private DataGridView dgvFinanzas;
        private Label        lblConteo;

        public SeccionFinanciero() : this(ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString ?? "") { }

        public SeccionFinanciero(string connStr)
        {
            this.connStr = connStr;
            BackColor = CFondo;
            Font = new Font("Segoe UI", 9);
            Build();
            CargarFinanzas();
        }

        private void Build()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = CCard };
            pnlTop.Paint += (s, e) => e.Graphics.DrawLine(new Pen(CBorder), 0, 67, pnlTop.Width, 67);

            var lblBread = new Label { Text = "Sistema  ›  Financiero", Font = new Font("Segoe UI", 7.5f), ForeColor = CSub, Location = new Point(28, 14), AutoSize = true };
            var lblTitulo = new Label { Text = "Resumen Financiero Global", Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = CTexto, Location = new Point(26, 30), AutoSize = true };

            pnlTop.Controls.Add(lblBread);
            pnlTop.Controls.Add(lblTitulo);

            dgvFinanzas = new DataGridView
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
                Font = new Font("Segoe UI", 9),
                RowTemplate = { Height = 46 }
            };

            dgvFinanzas.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = CHeader, ForeColor = CSub, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), Padding = new Padding(14, 0, 0, 0), SelectionBackColor = CHeader, SelectionForeColor = CSub };
            dgvFinanzas.DefaultCellStyle = new DataGridViewCellStyle { BackColor = CCard, ForeColor = CTexto, SelectionBackColor = CSeleccion, SelectionForeColor = CTexto, Padding = new Padding(14, 0, 0, 0) };
            
            // Coloreo custom
            dgvFinanzas.CellFormatting += DgvFinanzas_CellFormatting;
            dgvFinanzas.CellContentClick += OnCellClick;

            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 36, BackColor = CHeader };
            pnlFooter.Paint += (s, e) => e.Graphics.DrawLine(new Pen(CBorder), 0, 0, pnlFooter.Width, 0);
            lblConteo = new Label { Text = "", Font = new Font("Segoe UI", 8), ForeColor = CSub, Location = new Point(20, 10), AutoSize = true };
            pnlFooter.Controls.Add(lblConteo);

            Controls.Add(dgvFinanzas);
            Controls.Add(pnlFooter);
            Controls.Add(pnlTop);
        }

        private void DgvFinanzas_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null) return;
            string colName = dgvFinanzas.Columns[e.ColumnIndex].Name;

            if (colName == "IngresoTotal" || colName == "GastoTotal" || colName == "BeneficioTotal")
            {
                if (double.TryParse(e.Value.ToString(), out double val))
                {
                    e.Value = "L. " + val.ToString("N2");
                    e.FormattingApplied = true;

                    if (colName == "BeneficioTotal")
                    {
                        e.CellStyle.ForeColor = val >= 0 ? CPositivo : CNegativo;
                        e.CellStyle.Font = new Font(dgvFinanzas.Font, FontStyle.Bold);
                    }
                }
            }
        }

        private void AgregarColumnas()
        {
            dgvFinanzas.Columns.Clear();

            dgvFinanzas.Columns.Add(new DataGridViewTextBoxColumn { Name = "Proyecto", HeaderText = "PROYECTO", FillWeight = 40 });
            dgvFinanzas.Columns.Add(new DataGridViewTextBoxColumn { Name = "IngresoTotal", HeaderText = "INGRESOS", FillWeight = 20, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvFinanzas.Columns.Add(new DataGridViewTextBoxColumn { Name = "GastoTotal", HeaderText = "GASTOS", FillWeight = 20, DefaultCellStyle = { ForeColor = CNegativo, Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvFinanzas.Columns.Add(new DataGridViewTextBoxColumn { Name = "BeneficioTotal", HeaderText = "BENEFICIO NETO", FillWeight = 20, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });

            var colVer = new DataGridViewButtonColumn
            {
                Name = "BtnVer",
                HeaderText = "",
                Text = "Ver Detalle →",
                UseColumnTextForButtonValue = true,
                Width = 120,
                FlatStyle = FlatStyle.Flat
            };
            colVer.DefaultCellStyle = new DataGridViewCellStyle { BackColor = CSeleccion, ForeColor = CAccent, SelectionBackColor = CSeleccion, SelectionForeColor = CAccent, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Alignment = DataGridViewContentAlignment.MiddleCenter };
            dgvFinanzas.Columns.Add(colVer);
        }

        private void OnCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvFinanzas.Columns[e.ColumnIndex].Name != "BtnVer") return;

            var row = dgvFinanzas.Rows[e.RowIndex];
            if (!(row.Tag is int proyectoId)) return;
            string nombre = row.Cells["Proyecto"].Value?.ToString() ?? "";

            var det = new FormDetalleFinanciero(proyectoId, nombre, connStr);
            det.ShowDialog();
        }

        private void CargarFinanzas()
        {
            AgregarColumnas();
            try
            {
                int count = 0;
                using (var conn = new SqlConnection(connStr))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT ProyectoID, Proyecto, IngresoTotal, GastoTotal, BeneficioTotal FROM vResumenFinancieroProyecto ORDER BY Proyecto";
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int idx = dgvFinanzas.Rows.Add(r["Proyecto"], r["IngresoTotal"], r["GastoTotal"], r["BeneficioTotal"]);
                            dgvFinanzas.Rows[idx].Tag = Convert.ToInt32(r["ProyectoID"]);
                            count++;
                        }
                    }
                }
                lblConteo.Text = $"{count} proyectos analizados.";
            }
            catch (Exception ex){ lblConteo.Text = "Error: " + ex.Message; }
        }
    }
}

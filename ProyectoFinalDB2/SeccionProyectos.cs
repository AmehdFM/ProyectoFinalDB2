using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    /// <summary>
    /// Vista de proyectos que se incrusta en el PanelPrincipal (TopLevel = false).
    /// Acepta connStr desde el constructor para no depender de ConfigurationManager aquí.
    /// </summary>
    public class SeccionProyectos : Form
    {
        // ── Colores ──────────────────────────────────────────────────
        private static readonly Color CPrimario   = Color.FromArgb(15, 23, 42);
        private static readonly Color CAccent     = Color.FromArgb(67, 97, 238);
        private static readonly Color CFondo      = Color.FromArgb(241, 245, 249);
        private static readonly Color CCard       = Color.White;
        private static readonly Color CTexto      = Color.FromArgb(15, 23, 42);
        private static readonly Color CSub        = Color.FromArgb(100, 116, 139);
        private static readonly Color CBorder     = Color.FromArgb(226, 232, 240);
        private static readonly Color CHeader     = Color.FromArgb(248, 250, 252);
        private static readonly Color CSeleccion  = Color.FromArgb(235, 241, 255);
        private static readonly Color CVerde      = Color.FromArgb(16, 185, 129);
        private static readonly Color CVerdeLight = Color.FromArgb(209, 250, 229);

        private readonly string connStr;
        private DataGridView dgvProyectos;
        private Label        lblConteo;

        // ─────────────────────────────────────────────────────────────
        public SeccionProyectos() : this(
            ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString ?? "") { }

        public SeccionProyectos(string connStr)
        {
            this.connStr = connStr;
            BackColor    = CFondo;
            Font         = new Font("Segoe UI", 9);
            Build();
            CargarProyectos();
        }

        // ══════════════════════════════════════════════════════════════
        private void Build()
        {
            // ── Header de sección ─────────────────────────────────────
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = CCard };
            pnlTop.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(CBorder), 0, 67, pnlTop.Width, 67);

            var lblBread = new Label { Text = "Sistema  ›  Proyectos",
                Font = new Font("Segoe UI", 7.5f), ForeColor = CSub,
                Location = new Point(28, 14), AutoSize = true };

            var lblTitulo = new Label { Text = "Proyectos Inmobiliarios",
                Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = CTexto,
                Location = new Point(26, 30), AutoSize = true };

            var btnNuevo = new Button
            {
                Text      = "＋  Nuevo Proyecto",
                Size      = new Size(182, 36),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = CAccent, ForeColor = Color.White,
                Cursor    = Cursors.Hand
            };
            btnNuevo.FlatAppearance.BorderSize = 0;
            btnNuevo.Click += (s, _) => {
                using (var frm = new FormCrearProyecto())
                    if (frm.ShowDialog() == DialogResult.OK) CargarProyectos();
            };

            pnlTop.Controls.Add(lblBread);
            pnlTop.Controls.Add(lblTitulo);
            pnlTop.Controls.Add(btnNuevo);

            pnlTop.Resize += (s, _) =>
                btnNuevo.Location = new Point(pnlTop.Width - btnNuevo.Width - 24, 16);

            // ── Grid ──────────────────────────────────────────────────
            dgvProyectos = new DataGridView
            {
                Dock                    = DockStyle.Fill,
                BackgroundColor         = CCard,
                BorderStyle             = BorderStyle.None,
                AutoSizeColumnsMode     = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode           = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows      = false,
                ReadOnly                = true,
                RowHeadersVisible       = false,
                GridColor               = CBorder,
                CellBorderStyle         = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                Font                    = new Font("Segoe UI", 9),
                RowTemplate             = { Height = 46 }
            };
            dgvProyectos.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor           = CHeader, ForeColor = CSub,
                Font                = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                Padding             = new Padding(14, 0, 0, 0),
                SelectionBackColor  = CHeader, SelectionForeColor = CSub
            };
            dgvProyectos.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor          = CCard, ForeColor = CTexto,
                SelectionBackColor = CSeleccion, SelectionForeColor = CTexto,
                Padding            = new Padding(14, 0, 0, 0)
            };
            dgvProyectos.CellContentClick += OnCellClick;

            // ── Footer ────────────────────────────────────────────────
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 36, BackColor = CHeader };
            pnlFooter.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(CBorder), 0, 0, pnlFooter.Width, 0);
            lblConteo = new Label { Text = "", Font = new Font("Segoe UI", 8), ForeColor = CSub,
                Location = new Point(20, 10), AutoSize = true };
            pnlFooter.Controls.Add(lblConteo);

            // ── Ensamblado ────────────────────────────────────────────
            Controls.Add(dgvProyectos);
            Controls.Add(pnlFooter);
            Controls.Add(pnlTop);
        }

        // ══════════════════════════════════════════════════════════════
        //  COLUMNAS
        // ══════════════════════════════════════════════════════════════
        private void AgregarColumnas()
        {
            dgvProyectos.Columns.Clear();

            dgvProyectos.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "Nombre", HeaderText = "PROYECTO", FillWeight = 38 });

            dgvProyectos.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "Departamento", HeaderText = "DEPARTAMENTO", FillWeight = 20,
                  DefaultCellStyle = { ForeColor = CSub } });

            dgvProyectos.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "Municipio", HeaderText = "MUNICIPIO", FillWeight = 20,
                  DefaultCellStyle = { ForeColor = CSub } });

            dgvProyectos.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "Plazo", HeaderText = "PLAZO (MESES)", FillWeight = 14,
                  DefaultCellStyle = { ForeColor = CSub, Alignment = DataGridViewContentAlignment.MiddleCenter } });

            // ── Columna Etapas (badge numérico) ──────────────────────
            dgvProyectos.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "Etapas", HeaderText = "ETAPAS", FillWeight = 10,
                  DefaultCellStyle = {
                      ForeColor = CVerde, Font = new Font("Segoe UI", 9, FontStyle.Bold),
                      Alignment = DataGridViewContentAlignment.MiddleCenter } });

            // ── Columna "Ver Proyecto" (botón) ───────────────────────
            var colVer = new DataGridViewButtonColumn
            {
                Name                      = "BtnVer",
                HeaderText                = "",
                Text                      = "Ver Proyecto →",
                UseColumnTextForButtonValue = true,
                Width                     = 130,
                FlatStyle                 = FlatStyle.Flat
            };
            colVer.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor          = CSeleccion,
                ForeColor          = CAccent,
                SelectionBackColor = CSeleccion,
                SelectionForeColor = CAccent,
                Font               = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Alignment          = DataGridViewContentAlignment.MiddleCenter
            };
            dgvProyectos.Columns.Add(colVer);
        }

        // ══════════════════════════════════════════════════════════════
        //  EVENTO BOTÓN GRID
        // ══════════════════════════════════════════════════════════════
        private void OnCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvProyectos.Columns[e.ColumnIndex].Name != "BtnVer") return;

            var row = dgvProyectos.Rows[e.RowIndex];
            if (!(row.Tag is int proyectoId)) return;

            string nombre = row.Cells["Nombre"].Value?.ToString() ?? "";

            var detalle = new FormDetalleProyecto(proyectoId, nombre);
            detalle.ShowDialog();

            // Recargar por si se crearon etapas/bloques/lotes mientras estuvo abierto
            CargarProyectos();
        }

        // ══════════════════════════════════════════════════════════════
        //  CARGA DE DATOS
        // ══════════════════════════════════════════════════════════════
        private void CargarProyectos()
        {
            AgregarColumnas();

            if (string.IsNullOrEmpty(connStr)) { CargarEjemplo(); return; }

            try
            {
                int count = 0;
                using (var conn = new SqlConnection(connStr))
                using (var cmd  = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT P.ProyectoID, P.Nombre, P.Departamento, P.Municipio, P.PlazoMaximo,
                               COUNT(E.EtapaID) AS TotalEtapas
                        FROM Proyecto P
                        LEFT JOIN Etapa E ON P.ProyectoID = E.ProyectoID
                        GROUP BY P.ProyectoID, P.Nombre, P.Departamento, P.Municipio, P.PlazoMaximo
                        ORDER BY P.Nombre";
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int idx = dgvProyectos.Rows.Add(
                                r["Nombre"]?.ToString(),
                                r["Departamento"]?.ToString(),
                                r["Municipio"]?.ToString(),
                                r["PlazoMaximo"]?.ToString(),
                                r["TotalEtapas"]?.ToString()
                            );
                            dgvProyectos.Rows[idx].Tag = Convert.ToInt32(r["ProyectoID"]);
                            count++;
                        }
                    }
                }
                lblConteo.Text = $"{count} proyecto{(count != 1 ? "s" : "")} registrado{(count != 1 ? "s" : "")}";
            }
            catch (Exception ex)
            {
                lblConteo.Text = "Error al cargar: " + ex.Message;
                CargarEjemplo();
            }
        }

        private void CargarEjemplo()
        {
            int i1 = dgvProyectos.Rows.Add("Residencial Las Palmas", "Francisco Morazán", "Tegucigalpa", "120", "3");
            dgvProyectos.Rows[i1].Tag = 1;
            int i2 = dgvProyectos.Rows.Add("Centro Empresarial Maya", "Cortés", "San Pedro Sula", "240", "2");
            dgvProyectos.Rows[i2].Tag = 2;
            lblConteo.Text = "2 proyectos (datos de ejemplo — sin conexión)";
        }
    }
}
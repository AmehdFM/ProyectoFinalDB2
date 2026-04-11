using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    public class FormDetalleProyecto : Form
    {
        // ── Colores del sistema (Minimalista y moderno) ───────────────
        private static readonly Color CSidebar    = Color.FromArgb(15, 23, 42);
        private static readonly Color CSidebarHov = Color.FromArgb(30, 41, 59);
        private static readonly Color CSidebarSel = Color.FromArgb(67, 97, 238);
        private static readonly Color CFondo      = Color.FromArgb(241, 245, 249);
        private static readonly Color CCard       = Color.White;
        private static readonly Color CTexto      = Color.FromArgb(15, 23, 42);
        private static readonly Color CSub        = Color.FromArgb(100, 116, 139);
        private static readonly Color CBorder     = Color.FromArgb(226, 232, 240);
        private static readonly Color CHeader     = Color.FromArgb(248, 250, 252);
        private static readonly Color CSeleccion  = Color.FromArgb(235, 241, 255);

        // ── Layout y contenedores ─────────────────────────────────────
        private readonly int proyectoId;
        private readonly string proyectoNombre;
        private readonly string connStr;

        private Panel  pnlContenedorPrincipal;
        private Label  lblSeccionActiva;
        private Button btnActivo;

        // ─────────────────────────────────────────────────────────────
        public FormDetalleProyecto(int id, string nombre)
        {
            proyectoId = id;
            proyectoNombre = nombre;
            connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString ?? "";

            Text = $"Expediente Técnico: {proyectoNombre}";
            WindowState = FormWindowState.Maximized;
            BackColor = CFondo;
            Font = new Font("Segoe UI", 9);

            BuildUI();
        }

        // ══════════════════════════════════════════════════════════════
        //  CONSTRUCCIÓN PRINCIPAL
        // ══════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            // ── Sidebar ───────────────────────────────────────────────
            var sidebar = new Panel { Dock = DockStyle.Left, Width = 240, BackColor = CSidebar };

            var brand = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Color.FromArgb(10, 16, 30) };
            brand.Controls.Add(new Label
            {
                Text = "🏢  PROYECTO", ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), Dock = DockStyle.Top,
                TextAlign = ContentAlignment.BottomCenter, Height = 28
            });
            brand.Controls.Add(new Label
            {
                Text = proyectoNombre.ToUpper(), ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold), Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.TopCenter, Height = 44
            });
            sidebar.Controls.Add(brand);
            sidebar.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(30, 41, 59) });

            sidebar.Controls.Add(new Label
            {
                Text = "  GESTIÓN", ForeColor = Color.FromArgb(71, 85, 105),
                Font = new Font("Segoe UI", 7, FontStyle.Bold), Dock = DockStyle.Top,
                Height = 36, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(16, 0, 0, 0)
            });

            var btnLotes   = NavBtn("🏠  Lotes", "Lotes");
            var btnBloques = NavBtn("🧊  Bloques", "Bloques");
            var btnEtapas  = NavBtn("📑  Etapas", "Etapas");

            sidebar.Controls.Add(btnLotes);
            sidebar.Controls.Add(btnBloques);
            sidebar.Controls.Add(btnEtapas);

            // ── Header Principal ──────────────────────────────────────
            var header = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = CCard };
            header.Paint += (s, e) => e.Graphics.DrawLine(new Pen(CBorder), 0, 67, header.Width, 67);

            var lblBreadcrumb = new Label { Text = $"Proyectos  ›  {proyectoNombre}",
                Font = new Font("Segoe UI", 8), ForeColor = CSub,
                Location = new Point(28, 14), AutoSize = true };
            
            lblSeccionActiva = new Label { Text = "Sección",
                Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = CTexto,
                Location = new Point(26, 30), AutoSize = true };

            header.Controls.Add(lblBreadcrumb);
            header.Controls.Add(lblSeccionActiva);

            // ── Área principal ────────────────────────────────────────
            pnlContenedorPrincipal = new Panel { Dock = DockStyle.Fill, BackColor = CFondo, Padding = new Padding(0) };

            // ── Ensamblado ────────────────────────────────────────────
            Controls.Add(pnlContenedorPrincipal);
            Controls.Add(header);
            Controls.Add(sidebar);

            // Eventos 
            btnEtapas.Click  += (s, e) => { ActivarNav(btnEtapas);  MostrarSeccion("Etapas"); };
            btnBloques.Click += (s, e) => { ActivarNav(btnBloques); MostrarSeccion("Bloques"); };
            btnLotes.Click   += (s, e) => { ActivarNav(btnLotes);   MostrarSeccion("Lotes"); };

            ActivarNav(btnEtapas);
            MostrarSeccion("Etapas");
        }

        // ══════════════════════════════════════════════════════════════
        //  NAVEGACIÓN INTERNA
        // ══════════════════════════════════════════════════════════════
        private Button NavBtn(string text, string tag)
        {
            var b = new Button
            {
                Text = "   " + text, Tag = tag, Height = 50, Dock = DockStyle.Top,
                FlatStyle = FlatStyle.Flat, ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 9.5f), TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand, BackColor = Color.Transparent
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = CSidebarHov;
            return b;
        }

        private void ActivarNav(Button btn)
        {
            foreach (Control c in btn.Parent.Controls)
            {
                if (c is Button b && b != btn)
                {
                    b.BackColor = Color.Transparent; b.ForeColor = Color.FromArgb(148, 163, 184); b.Font = new Font("Segoe UI", 9.5f);
                }
            }
            btn.BackColor = CSidebarSel;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnActivo = btn;
        }

        // ══════════════════════════════════════════════════════════════
        //  GRID Y CONTENIDO
        // ══════════════════════════════════════════════════════════════
        private void MostrarSeccion(string seccion)
        {
            lblSeccionActiva.Text = seccion.ToUpper();
            pnlContenedorPrincipal.Controls.Clear();

            // Fondo blanco del grid simulando un Card central
            var innerWrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 20, 24, 24) };
            pnlContenedorPrincipal.Controls.Add(innerWrapper);

            // Top action bar
            var actionBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = CFondo };
            
            var btnNuevo = new Button {
                Text = "＋ Nuevo " + seccion.Substring(0, seccion.Length - 1),
                Size = new Size(160, 36), Location = new Point(0, 0),
                BackColor = CSidebarSel, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnNuevo.FlatAppearance.BorderSize = 0;
            btnNuevo.Click += (s, e) => { AbrirFormularioCreacion(seccion); MostrarSeccion(seccion); };
            actionBar.Controls.Add(btnNuevo);

            // DataGridView con diseño minimalista
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = CCard,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                GridColor = CBorder,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                Font = new Font("Segoe UI", 9),
                RowTemplate = { Height = 46 }
            };

            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = CHeader, ForeColor = CSub,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                Padding = new Padding(14, 0, 0, 0),
                SelectionBackColor = CHeader, SelectionForeColor = CSub
            };

            dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = CCard, ForeColor = CTexto,
                SelectionBackColor = CSeleccion, SelectionForeColor = CTexto,
                Padding = new Padding(14, 0, 0, 0)
            };

            if (seccion == "Lotes")
            {
                var btnVender = new Button {
                    Text = "💰 Vender Lote", Size = new Size(160, 36), Location = new Point(175, 0),
                    BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold), Enabled = false, Cursor = Cursors.Hand
                };
                btnVender.FlatAppearance.BorderSize = 0;

                dgv.SelectionChanged += (s, e) => {
                    if (dgv.SelectedRows.Count == 0) { btnVender.Enabled = false; return; }
                    string estado = dgv.SelectedRows[0].Cells["Estado"].Value?.ToString();
                    btnVender.Enabled = (estado == "Disponible");
                };

                btnVender.Click += (s, e) => {
                    if (dgv.SelectedRows.Count == 0) return;
                    int loteId = Convert.ToInt32(dgv.SelectedRows[0].Cells["LoteID"].Value);
                    using (var frmVenta = new FormCrearVenta(loteId, connStr))
                    {
                        if (frmVenta.ShowDialog() == DialogResult.OK) MostrarSeccion("Lotes");
                    }
                };
                actionBar.Controls.Add(btnVender);
            }

            // Un panel contenedor con border rad y sombra (simulado)
            var cardGrid = new Panel { Dock = DockStyle.Fill, BackColor = CCard, Padding = new Padding(0) };
            cardGrid.Controls.Add(dgv);
            
            innerWrapper.Controls.Add(cardGrid);
            innerWrapper.Controls.Add(actionBar);

            CargarDatosTabla(seccion, dgv);
        }

        private void AbrirFormularioCreacion(string seccion)
        {
            switch (seccion)
            {
                case "Etapas":
                    using (var frmEtapa = new FormCrearEtapa(proyectoId, connStr)) frmEtapa.ShowDialog();
                    break;
                case "Bloques":
                    using (var frmBloque = new FormCrearBloque(proyectoId, connStr)) frmBloque.ShowDialog();
                    break;
                case "Lotes":
                    using (var frmLote = new FormCrearLote(proyectoId, connStr)) frmLote.ShowDialog();
                    break;
            }
        }

        private void CargarDatosTabla(string seccion, DataGridView dgv)
        {
            string funcion = "";
            switch (seccion)
            {
                case "Etapas": funcion = "fn_ObtenerEtapasPorProyecto"; break;
                case "Bloques": funcion = "fn_ObtenerBloquesPorProyecto"; break;
                case "Lotes": funcion = "fn_ObtenerLotesPorProyecto"; break;
            }

            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand($"SELECT * FROM {funcion}(@ProyectoID)", conn))
                {
                    cmd.Parameters.AddWithValue("@ProyectoID", proyectoId);
                    var da = new SqlDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgv.DataSource = dt;

                    if (seccion == "Lotes" && dgv.Columns.Contains("LoteID"))
                        dgv.Columns["LoteID"].Visible = false;
                    
                    // Asegurarnos que la última columna llene el espacio disponible
                    if (dgv.Columns.Count > 0) dgv.Columns[dgv.Columns.Count -1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos de " + seccion + ": " + ex.Message);
            }
        }
    }
}
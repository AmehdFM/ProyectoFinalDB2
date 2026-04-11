using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;

namespace ProyectoFinalDB2
{
    public class FormDetalleProyecto : Form
    {
        private int proyectoId;
        private string proyectoNombre;
        private Panel pnlContenedorPrincipal;
        private Label lblSeccionActiva;
        private string connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString;

        private Color ColorSidebar = Color.FromArgb(30, 39, 46);
        private Color ColorFondo = Color.FromArgb(241, 242, 246);
        private Color ColorAccent = Color.FromArgb(52, 152, 219);

        public FormDetalleProyecto(int id, string nombre)
        {
            this.proyectoId = id;
            this.proyectoNombre = nombre;

            this.Text = $"Expediente Técnico: {proyectoNombre}";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = ColorFondo;
            this.Font = new Font("Segoe UI", 10);

            ConfigurarEstructura();
            MostrarSeccion("Etapas");
        }

        private void ConfigurarEstructura()
        {
            Panel pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = ColorSidebar
            };

            Label lblInfo = new Label
            {
                Text = proyectoNombre.ToUpper(),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 80,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlSidebar.Controls.Add(lblInfo);

            pnlSidebar.Controls.Add(CrearBotonMenu("LOTES", "Lotes"));
            pnlSidebar.Controls.Add(CrearBotonMenu("BLOQUES", "Bloques"));
            pnlSidebar.Controls.Add(CrearBotonMenu("ETAPAS", "Etapas"));

            Panel pnlMain = new Panel { Dock = DockStyle.Fill };

            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White
            };

            lblSeccionActiva = new Label
            {
                Text = "Cargando...",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(25, 20),
                AutoSize = true
            };
            pnlTop.Controls.Add(lblSeccionActiva);

            pnlContenedorPrincipal = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30)
            };

            pnlMain.Controls.Add(pnlContenedorPrincipal);
            pnlMain.Controls.Add(pnlTop);

            this.Controls.Add(pnlMain);
            this.Controls.Add(pnlSidebar);
        }

        private Button CrearBotonMenu(string texto, string tag)
        {
            Button btn = new Button
            {
                Text = "    " + texto,
                Height = 50,
                Dock = DockStyle.Top,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(210, 218, 226),
                TextAlign = ContentAlignment.MiddleLeft,
                Tag = tag,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => MostrarSeccion(btn.Tag.ToString());
            return btn;
        }

        private void MostrarSeccion(string seccion)
        {
            lblSeccionActiva.Text = seccion.ToUpper();
            pnlContenedorPrincipal.Controls.Clear();

            Button btnNuevo = new Button
            {
                Text = "+ Nuevo " + seccion.Substring(0, seccion.Length - 1),
                Size = new Size(180, 40),
                Location = new Point(0, 0),
                BackColor = ColorAccent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnNuevo.FlatAppearance.BorderSize = 0;
            btnNuevo.Click += (s, e) => AbrirFormularioCreacion(seccion);

            DataGridView dgv = new DataGridView
            {
                Location = new Point(0, 60),
                Size = new Size(pnlContenedorPrincipal.Width - 60, pnlContenedorPrincipal.Height - 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            pnlContenedorPrincipal.Controls.Add(btnNuevo);

            if (seccion == "Lotes")
            {
                Button btnVender = new Button
                {
                    Text = "$ Vender Lote",
                    Size = new Size(160, 40),
                    Location = new Point(190, 0),
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Enabled = false
                };
                btnVender.FlatAppearance.BorderSize = 0;

                dgv.SelectionChanged += (s, e) =>
                {
                    if (dgv.SelectedRows.Count == 0)
                    {
                        btnVender.Enabled = false;
                        return;
                    }
                    string estado = dgv.SelectedRows[0].Cells["Estado"].Value?.ToString();
                    btnVender.Enabled = estado == "Disponible";
                };

                btnVender.Click += (s, e) =>
                {
                    if (dgv.SelectedRows.Count == 0) return;
                    int loteId = Convert.ToInt32(dgv.SelectedRows[0].Cells["LoteID"].Value);
                    var frmVenta = new FormCrearVenta(loteId, connStr);
                    if (frmVenta.ShowDialog() == DialogResult.OK)
                        MostrarSeccion("Lotes");
                };

                pnlContenedorPrincipal.Controls.Add(btnVender);
            }

            pnlContenedorPrincipal.Controls.Add(dgv);
            CargarDatosTabla(seccion, dgv);
        } 

        private void AbrirFormularioCreacion(string seccion)
        {
            switch (seccion)
            {
                case "Etapas":
                    var frmEtapa = new FormCrearEtapa(proyectoId, connStr);
                    if (frmEtapa.ShowDialog() == DialogResult.OK)
                        MostrarSeccion("Etapas");
                    break;

                case "Bloques":
                    var frmBloque = new FormCrearBloque(proyectoId, connStr);
                    if (frmBloque.ShowDialog() == DialogResult.OK)
                        MostrarSeccion("Bloques");
                    break;

                case "Lotes":
                    var frmLote = new FormCrearLote(proyectoId, connStr);
                    if (frmLote.ShowDialog() == DialogResult.OK)
                        MostrarSeccion("Lotes");
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
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    SqlCommand cmd = new SqlCommand($"select * from {funcion}(@ProyectoID)", conn);
                    cmd.Parameters.AddWithValue("@ProyectoID", proyectoId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgv.DataSource = dt;

                    if (seccion == "Lotes" && dgv.Columns.Contains("LoteID"))
                        dgv.Columns["LoteID"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos: " + ex.Message);
            }
        }
    }
}
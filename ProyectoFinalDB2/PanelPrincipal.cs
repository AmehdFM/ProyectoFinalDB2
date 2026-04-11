using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinalDB2
{
    public class PanelPrincipal : Form
    {
        // â”€â”€ Colores del sistema â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private static readonly Color CSidebar    = Color.FromArgb(15, 23, 42);
        private static readonly Color CSidebarHov = Color.FromArgb(30, 41, 59);
        private static readonly Color CSidebarSel = Color.FromArgb(67, 97, 238);
        private static readonly Color CFondo      = Color.FromArgb(241, 245, 249);
        private static readonly Color CCard       = Color.White;
        private static readonly Color CTexto      = Color.FromArgb(15, 23, 42);
        private static readonly Color CSub        = Color.FromArgb(100, 116, 139);

        // â”€â”€ Layout â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private Panel  pnlContenido;   // Ãrea central donde se cargan las vistas
        private Button btnActivo;      // BotÃ³n de nav actualmente seleccionado

        // â”€â”€ Secciones embebidas â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private readonly string connStr;

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public PanelPrincipal()
        {
            connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString ?? "";

            Text           = "Inversiones Maya â€” Sistema de GestiÃ³n";
            WindowState    = FormWindowState.Maximized;
            StartPosition  = FormStartPosition.CenterScreen;
            MinimumSize    = new Size(1100, 650);
            BackColor      = CFondo;
            Font           = new Font("Segoe UI", 9);

            Build();
        }

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        //  CONSTRUCCIÃ“N PRINCIPAL
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        private void Build()
        {
            // â”€â”€ Sidebar â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            var sidebar = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 240,
                BackColor = CSidebar
            };

            // Logo / brand
            var brand = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Color.FromArgb(10, 16, 30) };
            brand.Controls.Add(new Label
            {
                Text      = "ðŸ   INVERSIONES MAYA",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
            sidebar.Controls.Add(brand);

            // Separador fino bajo el logo
            sidebar.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(30, 41, 59) });

            // Etiqueta de menÃº
            sidebar.Controls.Add(new Label
            {
                Text      = "  NAVEGACIÃ“N",
                ForeColor = Color.FromArgb(71, 85, 105),
                Font      = new Font("Segoe UI", 7, FontStyle.Bold),
                Dock      = DockStyle.Top,
                Height    = 36,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(16, 0, 0, 0)
            });

            // Botones de navegaciÃ³n
            var btnProyectos   = NavBtn("ðŸ“  Proyectos",  "proyectos");
            var btnClientes    = NavBtn("👥  Clientes",   "clientes");
            var btnEmpleados   = NavBtn("ðŸ‘¤  Empleados",  "empleados");
            var btnFinanciero  = NavBtn("ðŸ’°  Financiero", "financiero");

            sidebar.Controls.Add(btnProyectos);
            sidebar.Controls.Add(btnClientes);
            sidebar.Controls.Add(btnEmpleados);
            sidebar.Controls.Add(btnFinanciero);

            // VersiÃ³n al fondo del sidebar
            var lblVer = new Label
            {
                Text      = "v1.0  â€”  2025",
                ForeColor = Color.FromArgb(51, 65, 85),
                Font      = new Font("Segoe UI", 7.5f),
                Dock      = DockStyle.Bottom,
                Height    = 32,
                TextAlign = ContentAlignment.MiddleCenter
            };
            sidebar.Controls.Add(lblVer);

            // â”€â”€ Header â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            var header = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 58,
                BackColor = CCard,
                Padding   = new Padding(0)
            };
            header.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, 57, header.Width, 57);

            var lblBienvenido = new Label
            {
                Text      = "Bienvenido al Sistema de GestiÃ³n Inmobiliaria",
                ForeColor = CTexto,
                Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                Location  = new Point(28, 16),
                AutoSize  = true
            };
            header.Controls.Add(lblBienvenido);

            var lblFecha = new Label
            {
                Text      = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy",
                                new System.Globalization.CultureInfo("es-HN")),
                ForeColor = CSub,
                Font      = new Font("Segoe UI", 8),
                Dock      = DockStyle.Right,
                Width     = 310,
                TextAlign = ContentAlignment.MiddleRight,
                Padding   = new Padding(0, 0, 24, 0)
            };
            header.Controls.Add(lblFecha);

            // â”€â”€ Ãrea de contenido â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            pnlContenido = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = CFondo,
                Padding   = new Padding(0)
            };

            // â”€â”€ Ensamblado â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            Controls.Add(pnlContenido);
            Controls.Add(header);
            Controls.Add(sidebar);

            // Vista inicial: Proyectos
            MostrarSeccion(new SeccionProyectos(connStr));

            // Wire-up botones nav
            btnProyectos.Click  += (s, _) => { ActivarNav(btnProyectos);  MostrarSeccion(new SeccionProyectos(connStr)); };
            btnEmpleados.Click  += (s, _) => { ActivarNav(btnEmpleados);  MostrarSeccion(new SeccionEmpleados()); };
            btnClientes.Click   += (s, _) => { ActivarNav(btnClientes);   MostrarSeccion(new SeccionClientes(connStr)); };
            btnFinanciero.Click += (s, _) => { ActivarNav(btnFinanciero); MostrarSeccion(new SeccionFinanciero(connStr)); };

            // Activar el primer botÃ³n visualmente
            ActivarNav(btnProyectos);
        }

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        //  NAVEGACIÃ“N
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        private Button NavBtn(string text, string tag)
        {
            var b = new Button
            {
                Text      = "   " + text,
                Tag       = tag,
                Height    = 50,
                Dock      = DockStyle.Top,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(148, 163, 184),
                Font      = new Font("Segoe UI", 9.5f),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor    = Cursors.Hand,
                BackColor = Color.Transparent
            };
            b.FlatAppearance.BorderSize          = 0;
            b.FlatAppearance.MouseOverBackColor  = CSidebarHov;

            // Indicador lateral (Panel de 4px a la izquierda) â€” se pinta en ActivarNav
            return b;
        }

        private void ActivarNav(Button btn)
        {
            // Restablecer todos los botones nav
            foreach (Control c in btn.Parent.Controls)
            {
                if (c is Button b && b != btn)
                {
                    b.BackColor = Color.Transparent;
                    b.ForeColor = Color.FromArgb(148, 163, 184);
                    b.Font      = new Font("Segoe UI", 9.5f);
                }
            }
            btn.BackColor = CSidebarSel;
            btn.ForeColor = Color.White;
            btn.Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnActivo     = btn;
        }

        /// Muestra un UserControl / Form-como-panel en el Ã¡rea de contenido
        private void MostrarSeccion(Form seccion)
        {
            pnlContenido.Controls.Clear();

            seccion.TopLevel      = false;
            seccion.FormBorderStyle = FormBorderStyle.None;
            seccion.Dock          = DockStyle.Fill;

            pnlContenido.Controls.Add(seccion);
            seccion.Show();
        }

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        //  DASHBOARD (vista de inicio)
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        private void MostrarDashboard()
        {
            pnlContenido.Controls.Clear();

            var flow = new FlowLayoutPanel
            {
                Dock      = DockStyle.Fill,
                Padding   = new Padding(32, 28, 32, 28),
                AutoScroll = true,
                BackColor = CFondo
            };

            // TÃ­tulo
            flow.Controls.Add(new Label
            {
                Text      = "Panel de Control",
                Font      = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = CTexto,
                AutoSize  = true,
                Margin    = new Padding(0, 0, 0, 6),
                Width     = 600
            });
            flow.Controls.Add(new Label
            {
                Text      = "Resumen general del sistema inmobiliario",
                Font      = new Font("Segoe UI", 10),
                ForeColor = CSub,
                AutoSize  = true,
                Margin    = new Padding(0, 0, 0, 24),
                Width     = 600
            });

            // Tarjetas de estadÃ­sticas
            int[] datos = ObtenerEstadisticas();
            flow.Controls.Add(Tarjeta("Proyectos registrados", datos[0].ToString(), "ðŸ“", Color.FromArgb(67, 97, 238)));
            flow.Controls.Add(Tarjeta("Empleados activos",     datos[1].ToString(), "ðŸ‘¤", Color.FromArgb(16, 185, 129)));
            flow.Controls.Add(Tarjeta("Lotes disponibles",     datos[2].ToString(), "ðŸ ", Color.FromArgb(245, 158, 11)));
            flow.Controls.Add(Tarjeta("Ventas totales",        datos[3].ToString(), "ðŸ’°", Color.FromArgb(239, 68, 68)));

            // Acceso rÃ¡pido
            flow.Controls.Add(new Label
            {
                Text      = "Acceso rÃ¡pido",
                Font      = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = CTexto,
                AutoSize  = true,
                Margin    = new Padding(0, 36, 0, 12),
                Width     = 600
            });

            flow.Controls.Add(AccesoRapido("Ver Proyectos",  "ðŸ“", () => {
                ActivarNav(pnlContenido.Parent?.Controls.Find("", false)[0] as Button);
                MostrarSeccion(new SeccionProyectos(connStr));
            }));
            flow.Controls.Add(AccesoRapido("Ver Empleados", "ðŸ‘¤", () => MostrarSeccion(new SeccionEmpleados())));
            flow.Controls.Add(AccesoRapido("Gastos",        "ðŸ’°", () => MostrarSeccion(new SeccionGastosProyecto())));

            pnlContenido.Controls.Add(flow);
        }

        private int[] ObtenerEstadisticas()
        {
            int[] r = { 0, 0, 0, 0 };
            if (string.IsNullOrEmpty(connStr)) return r;
            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string[] sqls = {
                        "SELECT COUNT(*) FROM Proyecto",
                        "SELECT COUNT(*) FROM Empleado",
                        "SELECT COUNT(*) FROM Lote WHERE Estado = 'Disponible'",
                        "SELECT COUNT(*) FROM Venta"
                    };
                    for (int i = 0; i < sqls.Length; i++)
                        using (var cmd = new SqlCommand(sqls[i], conn))
                            r[i] = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch { }
            return r;
        }

        private Panel Tarjeta(string titulo, string valor, string icono, Color acento)
        {
            var p = new Panel
            {
                Size      = new Size(220, 120),
                BackColor = CCard,
                Margin    = new Padding(0, 0, 16, 16),
                Cursor    = Cursors.Default
            };
            p.Paint += (s, e) => {
                e.Graphics.FillRectangle(new SolidBrush(acento), 0, 0, 5, p.Height);
                using (var pen = new Pen(Color.FromArgb(226, 232, 240)))
                    e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            };

            p.Controls.Add(new Label { Text = icono + "  " + titulo,
                Font = new Font("Segoe UI", 8.5f), ForeColor = CSub,
                Location = new Point(18, 18), AutoSize = true });
            p.Controls.Add(new Label { Text = valor,
                Font = new Font("Segoe UI", 28, FontStyle.Bold), ForeColor = CTexto,
                Location = new Point(18, 44), AutoSize = true });
            return p;
        }

        private Button AccesoRapido(string texto, string icono, Action accion)
        {
            var b = new Button
            {
                Text      = icono + "  " + texto,
                Size      = new Size(190, 52),
                Margin    = new Padding(0, 0, 12, 0),
                BackColor = CCard,
                ForeColor = CTexto,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(10, 0, 0, 0)
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            b.FlatAppearance.BorderSize  = 1;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            b.Click += (s, _) => accion();
            return b;
        }

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Application.Exit();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            ClientSize = new Size(1100, 700);
            Name = "PanelPrincipal";
            Load += PanelPrincipal_Load;
            ResumeLayout(false);
        }
        private void PanelPrincipal_Load(object sender, EventArgs e) { }
    }
}


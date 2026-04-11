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
        private static readonly Color CSidebar    = Color.FromArgb(15, 23, 42);
        private static readonly Color CSidebarHov = Color.FromArgb(30, 41, 59);
        private static readonly Color CSidebarSel = Color.FromArgb(67, 97, 238);
        private static readonly Color CFondo      = Color.FromArgb(241, 245, 249);
        private static readonly Color CCard       = Color.White;
        private static readonly Color CTexto      = Color.FromArgb(15, 23, 42);
        private static readonly Color CSub        = Color.FromArgb(100, 116, 139);

        private Panel  pnlContenido;   
        private Button btnActivo;      

        private readonly string connStr;

        public PanelPrincipal()
        {
            connStr = ConfigurationManager.ConnectionStrings["ConexionInversiones"]?.ConnectionString ?? "";

            Text           = "Inversiones Maya - Sistema de Gestión";
            WindowState    = FormWindowState.Maximized;
            StartPosition  = FormStartPosition.CenterScreen;
            MinimumSize    = new Size(1100, 650);
            BackColor      = CFondo;
            Font           = new Font("Segoe UI", 9);

            Build();
        }

        private void Build()
        {
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
                Text      = "INVERSIONES MAYA",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
            sidebar.Controls.Add(brand);

            // Separador fino bajo el logo
            sidebar.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(30, 41, 59) });

            // Botones de navegaciÃ³n
            var btnProyectos   = NavBtn("Proyectos",  "proyectos");
            var btnClientes    = NavBtn("Clientes",   "clientes");
            var btnEmpleados   = NavBtn("Empleados",  "empleados");
            var btnFinanciero  = NavBtn("Financiero", "financiero");

            sidebar.Controls.Add(btnProyectos);
            sidebar.Controls.Add(btnClientes);
            sidebar.Controls.Add(btnEmpleados);
            sidebar.Controls.Add(btnFinanciero);

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
                Text      = "Bienvenido al Sistema de Gestión Inmobiliaria",
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

            pnlContenido = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = CFondo,
                Padding   = new Padding(0)
            };

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

            ActivarNav(btnProyectos);
        }

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

        private void MostrarSeccion(Form seccion)
        {
            pnlContenido.Controls.Clear();

            seccion.TopLevel      = false;
            seccion.FormBorderStyle = FormBorderStyle.None;
            seccion.Dock          = DockStyle.Fill;

            pnlContenido.Controls.Add(seccion);
            seccion.Show();
        }
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


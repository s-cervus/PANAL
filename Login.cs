using System;
using System.Drawing;
using System.Runtime.InteropServices;



namespace PA
{
    public partial class MainPage : APCPA
    {
        private FontCLo FontCLo;


        public Login()
        {
            InitializeComponent();
            
        }

        private void Login_Load(object sender, EventArgs e)
        {

            //this.optimizer();

            this.Region = System.Drawing.Region.FromHrgn(DreamStyle.CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            this.FormBorderStyle = FormBorderStyle.None;


            pnlUser.Region = Region.FromHrgn(DreamStyle.CreateRoundRectRgn(0, 0, pnlUser.Width, pnlUser.Height, 15, 15));
            pnlPasswd.Region = Region.FromHrgn(DreamStyle.CreateRoundRectRgn(0, 0, pnlPasswd.Width, pnlPasswd.Height, 15, 15));
            btnLogin.Region = Region.FromHrgn(DreamStyle.CreateRoundRectRgn(0, 0, btnLogin.Width, btnLogin.Height, 15, 15));
            btnRegister.Region = Region.FromHrgn(DreamStyle.CreateRoundRectRgn(0, 0, btnRegister.Width, btnRegister.Height, 15, 15));

            this.OpFullUI();
            try
            {
                FontCLo = new FontCLo();


                //FontCLo.Apply4All(this, 9f, FontStyle.Regular);
                lblW.Font = FontCLo.ObtainFont(45f, FontStyle.Regular);
                lblP.Font = FontCLo.ObtainFont(19f, FontStyle.Regular);
                lblU.Font = FontCLo.ObtainFont(19f, FontStyle.Regular);
                btnLogin.Font = FontCLo.ObtainFont(19F, FontStyle.Regular);
                btnRegister.Font = FontCLo.ObtainFont(19F, FontStyle.Regular);
                

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando la interfaz: " + ex.Message);
            }
        }

              

        private bool passwdHide = true;
        private void picEye_Click(object sender, EventArgs e)
        {
            if (passwdHide)
            {
                // Mostrar contraseña
                txtPasswd.UseSystemPasswordChar = false;
                txtPasswd.PasswordChar = '\0';
                picEye.Image = Properties.Resources.eye; // Cambia el icono
                passwdHide = false;
            }
            else
            {
                // Ocultar contraseña
                txtPasswd.UseSystemPasswordChar = true;
                picEye.Image = Properties.Resources.eye_closed; // Cambia el icono
                passwdHide = true;
            }
        }


        // Este evento se encargará del arrastre

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private static extern void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private static extern void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int lparam);

        private void pnlDragZone_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                // Le mandamos el handle del formulario completo (this.Handle), 
                // aunque el clic haya entrado por el panel invisible
                SendMessage(this.Handle, 0x112, 0xf012, 0);
            }
        }

        // ----------------------------------------------------




        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private int tryes = 0;

        public object GraphicExt { get; private set; }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if(tryes >= 3)
            {
                MessageBox.Show("Has excedido el número de intentos. La aplicación se cerrará.", "Acceso Denegado");
                Application.Exit();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPasswd.Text))
            {
                MessageBox.Show("No dejes campos vacíos.", "Error de entrada");
                return;
            }

            if (ControlUnit.ValidarYMutarLogin(txtUser.Text, txtPasswd.Text))
            {
                /*
                MessageBox.Show("Login Screen", "Bienvenido " + txtUser);
                */
                // Abrir el otro form y ocultar este
                Menu menu = new Menu();
                menu.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Credenciales incorrectas o inexistentes.", "Acceso Denegado");
                return;
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {

            Register menu = new Register();
            menu.Show();
            this.Hide();
            
        }
    }
}


//Pixeles y Curiosidades


        /*
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // width of ellipse
            int nHeightEllipse // height of ellipse
        );
        */





        /*
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // 0x02000000 es el flag de Win32 para WS_EX_COMPOSITED
                // Fuerza al sistema operativo a renderizar de abajo hacia arriba de forma síncrona
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
        */



        /*
        // P/Invoke para el efecto de sombra
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private void ActivarModoOscuroBarra()
        {
            int verdadero = 1; // 1 = Activar Modo Oscuro, 0 = Desactivar (Modo Claro)

            // Invocamos la función mágica
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref verdadero, sizeof(int));
        }
        */








/*
// --- CÓDIGO PARA REDONDEAR LOS PANELES DE TEXTO (HITBOXES) ---
// (Requiere GDI+ para recortar los paneles)

[DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

private void FormPrincipal_Load(object sender, EventArgs e)
{
    // Redondear el Panel que contiene el TextBox de 'Usuario'
    // El '15' es el radio de la curvatura
    pnlUsuario.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlUsuario.Width, pnlUsuario.Height, 15, 15));

    // Redondear el Panel de 'Contraseña'
    pnlContrasena.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlContrasena.Width, pnlContrasena.Height, 15, 15));

    // Redondear los botones de acción si no usas imágenes (opcional)
    btnRegistrar.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnRegistrar.Width, btnRegistrar.Height, 15, 15));
    btnAcceder.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnAcceder.Width, btnAcceder.Height, 15, 15));
}
*/

/*
        public Form()
        {
            InitializeComponent();

            // 1. Quitar bordes estándar
            this.FormBorderStyle = FormBorderStyle.None;

            // 2. Activar la sombra de la ventana (DWM) para un look pro
            int attrValue = 2; // DWMWA_WINDOW_CORNER_PREFERENCE
            DwmSetWindowAttribute(this.Handle, 33, ref attrValue, 4); // 33 es la opción de esquina redonda

            // (Si usas un .NET muy antiguo, el DWM de arriba puede no funcionar.
            //  En ese caso, usarías el método de 'Clipping Region' de abajo).
            // this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20)); // Método alternativo
        }
        */

// --- Método Alternativo Clásico de Recorte de Región (Hit Testing) ---
// Descomenta esto y la línea de arriba si el efecto DWM no funciona.
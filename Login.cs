using System;
using System.Drawing;
using System.Runtime.InteropServices;



namespace PANAL
{
    public partial class MainPage : ContentPage
    {

        /*
        public Login()
        {
            InitializeComponent();
            
        }
        */

        private void Login_Load(object sender, EventArgs e)
        {

            //this.optimizer();

            // this.Region = System.Drawing.Region.FromHrgn(DreamStyle.CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            // this.FormBorderStyle = FormBorderStyle.None;

            /*
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
            */
        }

              

        private bool passwdHide = true;
        private void picEye_Click(object sender, EventArgs e)
        {
            if (passwdHide)
            {
                // Mostrar contraseña
                /*
                txtPasswd.UseSystemPasswordChar = false;
                txtPasswd.PasswordChar = '\0';
                picEye.Image = Properties.Resources.eye; // Cambia el icono
                passwdHide = false;
                */
            }
            else
            {
                // Ocultar contraseña
                /*
                txtPasswd.UseSystemPasswordChar = true;
                picEye.Image = Properties.Resources.eye_closed; // Cambia el icono
                passwdHide = true;
                */
            }
        }


        // Este evento se encargará del arrastre

        /*

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
        */

        // ----------------------------------------------------




        private void btnClose_Click(object sender, EventArgs e)
        {
            
            // Application.Exit();
        }

        private int tryes = 0;

        public object GraphicExt { get; private set; }

        /*
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
                
                // MessageBox.Show("Login Screen", "Bienvenido " + txtUser);
                
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
        */    
        
        
    }
}
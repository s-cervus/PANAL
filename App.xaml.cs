using Microsoft.Extensions.DependencyInjection;

namespace APCPA
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            //generar un acceso para mainpage
            MainPage = new NavigationPage(new Registro());
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
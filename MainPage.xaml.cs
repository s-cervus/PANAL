namespace APCPA
{
    public partial class MainPage : ContentPage
    {
        

        public MainPage()
        {
            InitializeComponent();
            // Routing.RegisterRoute(nameof(Registro), typeof(Registro));
        }

        private async void RegisterBTN(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new Registro());
        }
    }
}

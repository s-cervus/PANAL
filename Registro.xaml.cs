namespace APCPA;

public partial class Registro : ContentPage
{
	public Registro()
	{
		InitializeComponent();
	}
	private async void OnDeleteClicked (object sender, EventArgs e)
	{

	}

    private void OnModificarClicked (object sender, EventArgs e)
    {

    }


	private async void Return(object? sender, EventArgs e)
	{
		await Navigation.PushAsync(new MainPage());
    }
}
using Microsoft.VisualBasic;

namespace APCPA;

public partial class Access : ContentPage
{
	public Access()
	{
		InitializeComponent();
	}
	//redireccionamiento a la pagina principal con el boton disponible (ajustar para prototipo final)
	private async void BtnCont_Clicked(object sender, EventArgs e)
	{
        Application.Current.MainPage = new NavigationPage(new MainPage());

    }
}
namespace PotoDocs.View;

public partial class DriversPage : ContentPage
{
	public DriversPage(DriversViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
	}
}
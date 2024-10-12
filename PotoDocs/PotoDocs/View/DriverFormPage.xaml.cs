namespace PotoDocs.View;

public partial class DriverFormPage : ContentPage
{
	public DriverFormPage(DriverFormViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}
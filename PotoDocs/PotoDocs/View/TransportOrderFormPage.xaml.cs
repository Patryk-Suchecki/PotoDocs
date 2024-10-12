namespace PotoDocs.View;

public partial class TransportOrderFormPage : ContentPage
{
	public TransportOrderFormPage(TransportOrderFormViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}
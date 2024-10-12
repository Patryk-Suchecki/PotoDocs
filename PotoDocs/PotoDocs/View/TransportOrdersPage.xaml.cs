namespace PotoDocs.View;

public partial class TransportOrdersPage : ContentPage
{
	public TransportOrdersPage(TransportOrdersViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}
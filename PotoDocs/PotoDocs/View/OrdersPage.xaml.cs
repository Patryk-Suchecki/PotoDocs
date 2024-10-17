namespace PotoDocs.View;

public partial class OrdersPage : ContentPage
{
	public OrdersPage(OrdersViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}
namespace PotoDocs.View;

public partial class OrdersPage : ContentPage
{
	public OrdersPage(OrdersViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
}
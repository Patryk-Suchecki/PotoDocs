
namespace PotoDocs.View;

public partial class OrdersPage : ContentPage
{
    private readonly OrdersViewModel _viewModel;
    public OrdersPage(OrdersViewModel viewModel)
	{
        BindingContext = viewModel;
        _viewModel = viewModel;

        InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.GetAll();
    }
}
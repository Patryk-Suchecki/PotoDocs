namespace PotoDocs.View;

public partial class OrderFormPage : ContentPage
{
    private readonly OrderFormViewModel _viewModel;
    public OrderFormPage(OrderFormViewModel viewModel)
    {
        BindingContext = viewModel;
        _viewModel = viewModel;

        InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.GetAllDrivers();
    }
}
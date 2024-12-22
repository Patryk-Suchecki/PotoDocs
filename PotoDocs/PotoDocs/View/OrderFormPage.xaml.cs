namespace PotoDocs.View;

public partial class OrderFormPage : ContentPage
{
    private readonly OrderFormViewModel _viewModel;
    public OrderFormPage(OrderFormViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.Initialize();
    }
}
namespace PotoDocs.View;

public partial class DriversPage : ContentPage
{
    DriversViewModel _viewModel;
    public DriversPage(DriversViewModel viewModel)
	{
        BindingContext = viewModel;
        _viewModel = viewModel;

        InitializeComponent();
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.GetUsersAsync();
    }
}
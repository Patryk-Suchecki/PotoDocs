
namespace PotoDocs.View;

public partial class DriverFormPage : ContentPage
{
    DriverFormViewModel _viewModel;

    public DriverFormPage(DriverFormViewModel viewModel)
	{
        BindingContext = viewModel;
        _viewModel = viewModel;

        InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.GetRoles();
    }
}

namespace PotoDocs.View;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;
    public SettingsPage(SettingsViewModel viewModel)
	{
        _viewModel = viewModel;
        BindingContext = viewModel;

        InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.GetUser();
    }
}
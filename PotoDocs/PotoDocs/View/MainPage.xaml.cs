using PotoDocs.Services;

namespace PotoDocs.View;

public partial class MainPage : ContentPage
{
    private readonly IAuthService _authService;
    private readonly MainViewModel _viewModel;
    public MainPage(MainViewModel viewModel, IAuthService authService)
    {
        BindingContext = viewModel;
        _authService = authService;
        _viewModel = viewModel;

        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var isAuthenticated = await _authService.IsUserAuthenticated();

        Dispatcher.Dispatch(async () =>
        {
            if (!isAuthenticated)
            {
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }
        });
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.GetAll();
    }
}

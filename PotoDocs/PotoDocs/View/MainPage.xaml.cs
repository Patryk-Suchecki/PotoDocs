using PotoDocs.Services;

namespace PotoDocs.View;

public partial class MainPage : ContentPage
{
    private readonly IAuthService _authService;
    public MainPage(MainViewModel viewModel, IAuthService authService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _authService = authService;
        BindingContext = viewModel;
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
}

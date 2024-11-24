using PotoDocs.Services;

namespace PotoDocs.View;

public partial class MainPage : ContentPage
{
    private readonly IAuthService _authService;
    public MainPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var isAuthenticated = await _authService.IsUserAuthenticated();

        // Wykonanie nawigacji na wątku interfejsu użytkownika
        Dispatcher.Dispatch(async () =>
        {
            if (isAuthenticated)
            {
                await Shell.Current.GoToAsync($"//{nameof(OrdersPage)}");
            }
            else
            {
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }
        });
    }
}

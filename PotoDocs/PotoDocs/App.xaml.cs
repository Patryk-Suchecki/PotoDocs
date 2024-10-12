using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs;

public partial class App : Application
{
    private readonly AuthService _authService;

    public App(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;

        MainPage = new ContentPage
        {
            Content = new ActivityIndicator
            {
                IsRunning = true,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            }
        };

        CheckAuthentication();
    }

    private async void CheckAuthentication()
    {
        if (await _authService.IsUserAuthenticated())
        {
            MainPage = new AppShell(_authService);
        }
        else
        {
            MainPage = new NavigationPage(new LoginPage(_authService));
        }
    }
}

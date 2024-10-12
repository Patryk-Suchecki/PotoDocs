using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs;

public partial class AppShell : Shell
{
    private readonly AuthService _authService;
    public AppShell(AuthService authService)
    {
        InitializeComponent();

        _authService = authService;
        this.Navigating += AppShell_Navigating;

        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(TransportOrdersPage), typeof(TransportOrdersPage));
        Routing.RegisterRoute(nameof(DetailsPage), typeof(DetailsPage));
        Routing.RegisterRoute(nameof(TransportOrderFormPage), typeof(TransportOrderFormPage));
        Routing.RegisterRoute(nameof(DownloadPage), typeof(DownloadPage));
        Routing.RegisterRoute(nameof(DriversPage), typeof(DriversPage));
        Routing.RegisterRoute(nameof(DriverFormPage), typeof(DriverFormPage));
    }
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        _authService.Logout();
        Application.Current.MainPage = new NavigationPage(new LoginPage(_authService)); ;
    }

    private async void AppShell_Navigating(object sender, ShellNavigatingEventArgs e)
    {
        if (!await _authService.IsUserAuthenticated() && e.Target.Location.OriginalString != "//LoginPage")
        {
            e.Cancel();
            Application.Current.MainPage = new NavigationPage(new LoginPage(_authService)); ;
        }
    }
}

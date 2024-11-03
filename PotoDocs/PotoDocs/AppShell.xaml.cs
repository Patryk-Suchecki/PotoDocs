using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs;

public partial class AppShell : Shell
{
    private readonly AuthService _authService;
    public AppShell()
    {
        InitializeComponent();
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

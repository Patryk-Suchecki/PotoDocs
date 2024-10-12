using PotoDocs.Services;

namespace PotoDocs.View;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;
    public LoginPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;


        var error = await _authService.LoginAsync(new LoginDto() { Email = username , Password = password });
        if (string.IsNullOrWhiteSpace(error))
        {
            Application.Current.MainPage = new AppShell(_authService);
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", error, "Ok");
        }
    }
}
using PotoDocs.Services;

namespace PotoDocs.View;

public partial class LoginPage : ContentPage
{
    private readonly IAuthService _authService;

    public LoginPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        _authService.Logout();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;

        var error = await _authService.LoginAsync(new LoginRequestDto(username, password));
        if (string.IsNullOrWhiteSpace(error))
        {
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }
        else
        {
            ResponseLabel.Text = error;
        }
    }
}
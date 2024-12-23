using PotoDocs.Services;

namespace PotoDocs.View;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel loginViewModel)
    {
        BindingContext = loginViewModel;

        InitializeComponent();
    }
}
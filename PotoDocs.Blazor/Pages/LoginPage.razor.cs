using Microsoft.AspNetCore.Components;
using MudBlazor;
using PotoDocs.Blazor.Services;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Pages;

public partial class LoginPage
{
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private LoginDto _loginModel = new();

    private string? _errorMessage;

    private bool _isPasswordVisible;
    private InputType _passwordInputType = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;

    private async Task SubmitAsync()
    {
        _errorMessage = null;

        var result = await AuthService.LoginAsync(_loginModel);

        if (result.IsSuccess)
        {
            Navigation.NavigateTo("/", true);
        }
        else
        {
            _errorMessage = result.ErrorMessage;
        }
    }

    private void TogglePasswordVisibility()
    {
        if (_isPasswordVisible)
        {
            _isPasswordVisible = false;
            _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
            _passwordInputType = InputType.Password;
        }
        else
        {
            _isPasswordVisible = true;
            _passwordInputIcon = Icons.Material.Filled.Visibility;
            _passwordInputType = InputType.Text;
        }
    }
}
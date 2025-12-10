using Microsoft.AspNetCore.Components;
using MudBlazor;
using PotoDocs.Blazor.Services;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Pages;

public partial class LoginPage
{
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private readonly LoginDto LoginDto = new();
    private readonly LoginDtoValidator loginValidator = new();
    private MudForm form = default!;

    private string? errorMessage;
    bool isShow;
    InputType PasswordInput = InputType.Password;
    string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;

    private async Task SubmitAsync()
    {
        errorMessage = null;

        await form.Validate();
        if (!form.IsValid)
        {
            return;
        }

        var result = await AuthService.LoginAsync(LoginDto);

        if (result.IsSuccess)
        {
            Navigation.NavigateTo("/", true);
        }
        else
        {
            errorMessage = result.ErrorMessage;

        }
    }

    void ButtonTestclick()
    {
        if (isShow)
        {
            isShow = false;
            PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
            PasswordInput = InputType.Password;
        }
        else
        {
            isShow = true;
            PasswordInputIcon = Icons.Material.Filled.Visibility;
            PasswordInput = InputType.Text;
        }
    }
}
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PotoDocs.Blazor.Services;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Pages;

public partial class SettingsPage
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private readonly List<BreadcrumbItem> _items = [new("Strona główna", href: "#"), new("Ustawienia", href: null, disabled: true)];
    private UserDto UserDto { get; set; } = new();
    private ChangePasswordDto ChangePasswordDto { get; set; } = new() { Email = "", NewPassword = "", OldPassword = "" };
    private bool IsLoading = true;

    private MudForm form = default!;
    private readonly ChangePasswordDtoValidator passwordValidator = new();
    private bool _isPasswordVisible;
    private InputType _passwordInputType = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    private string _confirmPassword = "";
    protected override async Task OnInitializedAsync()
    {
        await LoadUser();
    }

    private async Task LoadUser()
    {
        try
        {
            IsLoading = true;
            UserDto = await UserService.GetCurrentUser();
            ChangePasswordDto.Email = UserDto.Email;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Błąd: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SubmitAsync()
    {
        await form.Validate();
        if (!form.IsValid)
        {
            Snackbar.Add("Formularz zawiera błędy.", Severity.Warning);
            return;
        }

        try
        {
            IsLoading = true;
            await UserService.ChangePassword(ChangePasswordDto);

            Snackbar.Add($"Hasło zostało zmienione, zaloguj się ponownie", Severity.Success);
            Navigation.NavigateTo("/logowanie");
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Błąd: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsLoading = false;
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
    private IEnumerable<string> ValidateConfirmPassword(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            yield return "Potwierdzenie hasła jest wymagane.";
        }

        if (value != ChangePasswordDto.NewPassword)
        {
            yield return "Hasła muszą być identyczne.";
        }
    }
}
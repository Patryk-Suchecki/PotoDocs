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
            var result = await UserService.ChangePassword(ChangePasswordDto);

            if (string.IsNullOrEmpty(result))
            {
                Snackbar.Add($"Hasło zostało zmienione, zaloguj się ponownie", Severity.Success);
                Navigation.NavigateTo("/logowanie");
            }
            else
            {
                Snackbar.Add(result, Severity.Error);
            }
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
}
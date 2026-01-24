using Microsoft.AspNetCore.Components;
using MudBlazor;
using PotoDocs.Blazor.Dialogs;
using PotoDocs.Blazor.Services;
using PotoDocs.Shared.Models;
using System.Text.Json;

namespace PotoDocs.Blazor.Pages;

public partial class UsersPage
{
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IRoleService RoleService { get; set; } = default!;
    private MudTable<UserDto> table = default!;
    private readonly List<BreadcrumbItem> _items = [new("Strona główna", href: "#"), new("Użytkownicy", href: null, disabled: true)];
    private readonly DialogOptions DialogOptions = new()
    {
        CloseButton = true,
        FullWidth = true,
        MaxWidth = MaxWidth.Small
    };
    private List<string> Roles { get; set; } = [];

    private async Task<TableData<UserDto>> ServerReload(TableState state, CancellationToken token)
    {
        try
        {
            var users = await UserService.GetAll();

            return new TableData<UserDto>() { Items = users};
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Błąd ładowania użytkowników: {ex.Message}", Severity.Error);

            return new TableData<UserDto>() { Items = [] };
        }
    }
    private async Task Create()
    {
        var parameters = new DialogParameters
        {
            { nameof(UserDialog.Roles), Roles },
            { nameof(UserDialog.Type), UserFormType.Create }
        };

        var dialog = await DialogService.ShowAsync<UserDialog>("Dodawanie użytkownika", parameters, DialogOptions);
        var result = await dialog.Result;

        if (result != null && !result.Canceled && result.Data is UserDto user)
        {
            try
            {
                await UserService.RegisterAsync(user);
                Snackbar.Add("Użytkownik zapisany", Severity.Success);

                await table.ReloadServerData();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Błąd przy dodawaniu użytkownika: {ex.Message}", Severity.Error);
            }
        }
    }

    private async Task Edit(UserDto user)
    {
        var parameters = new DialogParameters
    {
        { nameof(UserDialog.UserDto), JsonSerializer.Deserialize<UserDto>(JsonSerializer.Serialize(user)) },
        { nameof(UserDialog.Roles), Roles },
        { nameof(UserDialog.Type), UserFormType.Update }
    };

        var dialog = await DialogService.ShowAsync<UserDialog>($"Edytuj użytkownika", parameters, DialogOptions);
        var result = await dialog.Result;

        if (result != null && !result.Canceled && result.Data is UserDto userdto)
        {
            try
            {
                await UserService.Update(userdto);

                Snackbar.Add("Pomyślnie zaktualizowano użytkownika.", Severity.Success);

                await table.ReloadServerData();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Błąd przy aktualizacji użytkownika: {ex.Message}", Severity.Error);
            }
        }
    }

    private async Task Delete(UserDto user)
    {
        var parameters = new DialogParameters
        {
            { nameof(UserDialog.UserDto), user },
            { nameof(UserDialog.Roles), Roles },
            { nameof(UserDialog.Type), UserFormType.Delete }
        };

        var dialog = await DialogService.ShowAsync<UserDialog>($"Usuń użytkownika", parameters, DialogOptions);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            try
            {
                await UserService.Delete(user.Email);
                Snackbar.Add("Pomyślnie usunięto użytkownika", Severity.Success);
                await table.ReloadServerData();
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }

    private async Task Details(UserDto user)
    {
        var parameters = new DialogParameters
        {
            { nameof(UserDialog.UserDto), user },
            { nameof(UserDialog.Type), UserFormType.Details }
        };

        await DialogService.ShowAsync<UserDialog>($"Szczegóły użytkownika", parameters, DialogOptions);
    }

    protected override async Task OnInitializedAsync()
    {
        await GetRolesAsync();
    }

    private async Task GetRolesAsync()
    {
        try
        {
            Roles = [.. (await RoleService.GetRoles())];
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }

    private async Task GeneratePassword(string email)
    {
        try
        {
            await UserService.GeneratePassword(email);
            Snackbar.Add("Wygenerowano nowe hasło.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }

}

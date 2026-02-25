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
    private readonly DialogOptions DialogOptions = new() { CloseButton = true, FullWidth = true, MaxWidth = MaxWidth.Small };
    private List<string> Roles { get; set; } = [];
    private bool _isDetailsOpen = false;
    private UserDto _selectedUser;

    private void OpenDetails(UserDto user)
    {
        _selectedUser = user;
        _isDetailsOpen = true;
    }
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
    private void OnRowClicked(TableRowClickEventArgs<UserDto> args)
    {
        OpenDetails(args.Item);
    }
    private async Task Create()
    {
        var parameters = new DialogParameters
        {
            { nameof(UserDialog.Roles), Roles },
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
        bool? result = await DialogService.ShowMessageBox(
            "Potwierdzenie usunięcia",
            $"Czy na pewno chcesz usunąć użytkownika {user.FirstAndLastName}? Tej operacji nie można cofnąć.",
            yesText: "Usuń",
            cancelText: "Anuluj");

        if (result == true)
        {
            try
            {
                await UserService.Delete(user.Id);
                Snackbar.Add("Pomyślnie usunięto użytkownika", Severity.Success);

                await table.ReloadServerData();
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
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

    private async Task GeneratePassword(Guid id)
    {
        try
        {
            await UserService.GeneratePassword(id);
            Snackbar.Add("Wygenerowano nowe hasło.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }

}

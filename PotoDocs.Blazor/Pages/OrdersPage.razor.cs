using Microsoft.AspNetCore.Components;
using MudBlazor;
using PotoDocs.Blazor.Dialogs;
using PotoDocs.Blazor.Helpers;
using PotoDocs.Blazor.Services;
using PotoDocs.Shared.Models;
using System.Text.Json;

namespace PotoDocs.Blazor.Pages;

public partial class OrdersPage
{
    [Inject] private IOrderService OrderService { get; set; } = default!;
    [Inject] private IInvoiceService InvoiceService { get; set; } = default!;
    [Inject] private IFileDownloadHelper FileDownloader { get; set; } = default!;
    [Inject] private IDownloadsService DownloadsService { get; set; } = default!;
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [SupplyParameterFromQuery(Name = "search")]
    public string? searchString { get; set; }

    private HashSet<OrderDto> selectedOrders = [];
    private bool IsLoading = true;
    private List<OrderDto> orders = [];
    private List<UserDto> users = [];
    private readonly List<BreadcrumbItem> _items = [new("Strona główna", href: "#"), new("Zlecenia", href: null, disabled: true)];

    private readonly TableGroupSorter<OrderDto> _sorter = new(x => x.UnloadingDate, "Data rozładunku");

    private void OnSortDirectionChanged(SortDirection direction)
    {
        if (_sorter.UpdateDirection(direction))
        {
            StateHasChanged();
        }
    }
   
    protected override async Task OnInitializedAsync()
    {
        await LoadUsers();
        await LoadOrders();
    }

    private async Task LoadUsers()
    {
        try
        {
            IsLoading = true;
            users = [.. (await UserService.GetAll())];
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Błąd podczas pobierania użytkowników: {ex.Message}", Severity.Error);
            users = [];
        }
        finally
        {
            IsLoading = false;
        }
    }
    private async Task LoadOrders()
    {
        try
        {
            IsLoading = true;
            orders = [.. (await OrderService.GetAll())];
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Błąd podczas pobierania zleceń: {ex.Message}", Severity.Error);
            orders = [];
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool FilterFunc1(OrderDto order) => FilterFunc(order, searchString);

    private static bool FilterFunc(OrderDto order, string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return true;
        if (order.OrderNumber.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (order.Invoice?.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false)
            return true;
        if (order.Invoice?.InvoiceNumber.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false)
            return true;
        if (order.Company?.Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false)
            return true;
        if (order.UnloadingDate?.ToString("dd.MM.yyyy").Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false)
            return true;
        if (order.Price.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    private async Task Create()
    {
        var order = new OrderDto() { Driver = await UserService.GetCurrentUser() };
        var parameters = new DialogParameters
        {
            { nameof(OrderDialog.OrderDto), order },
            { nameof(OrderDialog.Users), users },
            { nameof(OrderDialog.Type), OrderFormType.Create }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            FullWidth = true,
            MaxWidth = MaxWidth.ExtraLarge
        };

        var dialogRef = await DialogService.ShowAsync<OrderDialog>("Dodawanie zlecenia", parameters, options);
        var result = await dialogRef.Result;

        if (result is not null && !result.Canceled && result.Data is OrderDialogResult orderDialogResult)
        {
            try
            {
                await OrderService.Create(orderDialogResult.Order, orderDialogResult.OrderFiles, orderDialogResult.CmrFiles);

                Snackbar.Add("Pomyślnie zapisano zlecenie.", Severity.Success);

                await LoadOrders();
                searchString = orderDialogResult.Order.OrderNumber;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Błąd przy dodawaniu zlecenia: {ex.Message}", Severity.Error);
            }
        }
    }

    private async Task Edit(OrderDto order)
    {
        var clonedOrder = JsonSerializer.Deserialize<OrderDto>(JsonSerializer.Serialize(order))!;
        var parameters = new DialogParameters
        {
            { nameof(OrderDialog.OrderDto), clonedOrder },
            { nameof(OrderDialog.Users), users },
            { nameof(OrderDialog.Type), OrderFormType.Update }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            FullWidth = true,
            MaxWidth = MaxWidth.ExtraLarge
        };

        var dialogRef = await DialogService.ShowAsync<OrderDialog>($"Edytuj zlecenie nr {order.OrderNumber}", parameters, options);
        var result = await dialogRef.Result;

        if (result is not null && !result.Canceled && result.Data is OrderDialogResult orderFormDialogResult)
        {
            try
            {
                await OrderService.Update(orderFormDialogResult.Order, orderFormDialogResult.OrderFiles, orderFormDialogResult.CmrFiles, orderFormDialogResult.FileIdsToDelete);
                Snackbar.Add("Pomyślnie zaktualizowano zlecenie.", Severity.Success);

                await LoadOrders();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Błąd przy aktualizacji zlecenia: {ex.Message}", Severity.Error);
            }
        }
    }

    private async Task Delete(OrderDto order)
    {
        var parameters = new DialogParameters
        {
            { nameof(OrderDialog.OrderDto), order },
            { nameof(OrderDialog.Users), users },
            { nameof(OrderDialog.Type), OrderFormType.Delete }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            FullWidth = true,
            MaxWidth = MaxWidth.ExtraLarge
        };

        var dialogRef = await DialogService.ShowAsync<OrderDialog>($"Usuń zlecenie nr {order.OrderNumber}", parameters, options);
        var result = await dialogRef.Result;

        if (result is not null && !result.Canceled)
        {
            try
            {
                await OrderService.Delete(order.Id);

                Snackbar.Add($"Pomyślnie usunięto zlecenie {order.OrderNumber}", Severity.Success);

                orders?.Remove(order);

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }

    private async Task Details(OrderDto order)
    {
        var parameters = new DialogParameters
        {
            { nameof(OrderDialog.OrderDto), order },
            { nameof(OrderDialog.Type), OrderFormType.Details }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            FullWidth = true,
            MaxWidth = MaxWidth.ExtraLarge
        };

        await DialogService.ShowAsync<OrderDialog>($"Szczegóły zlecenia nr {order.OrderNumber}", parameters, options);
    }

    private async Task SendEmail(OrderDto order)
    {
        if (order == null) return;

        try
        {
            await OrderService.SendDocuments(order.Id);
            await LoadOrders();
            StateHasChanged();
            Snackbar.Add($"Pomyślnie wysłano dokumenty dla {order.OrderNumber}", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Błąd podczas wysyłania e-maila: {ex.Message}", Severity.Error);
        }
    }
    private async Task CreateInvoice(OrderDto order)
    {
        if (order == null) return;

        try
        {
            var invoice = await InvoiceService.CreateFromOrder(order.Id);
            await LoadOrders();
            StateHasChanged();
            Snackbar.Add($"Pomyślnie wystawiono fakturę nr {invoice.Name}", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Błąd podczas wystawiania faktury: {ex.Message}", Severity.Error);
        }
    }
    private async Task OpenMarkSentDialogAsync()
    {
        if (selectedOrders.Count == 0)
        {
            Snackbar.Add("Zaznacz najpierw zlecenia", Severity.Warning);
            return;
        }

        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<MarkSentDialog>("Rejestracja wysyłki", options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is DateTime selectedDate)
        {
            var orderIds = selectedOrders.Select(x => x.Id).ToList();

            await OrderService.MarkOrdersAsSentAsync(orderIds, selectedDate);
            Snackbar.Add($"Oznaczono {orderIds.Count} zleceń jako wysłane.", Severity.Success);
        }
    }
    private async Task DownloadOrders()
    {
        if (selectedOrders.Count == 0)
        {
            Snackbar.Add("Zaznacz najpierw zlecenia", Severity.Warning);
            return;
        }

        IsLoading = true;
        StateHasChanged();

        try
        {
            var ids = selectedOrders.Select(o => o.Id).ToList();
            var response = await DownloadsService.DownloadOrders(ids);
            await FileDownloader.DownloadFromResponseAsync(response);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Wystąpił błąd: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task DownloadDocuments()
    {
        if (selectedOrders.Count == 0)
        {
            Snackbar.Add("Zaznacz najpierw zlecenia", Severity.Warning);
            return;
        }

        IsLoading = true;
        StateHasChanged();

        try
        {
            var ids = selectedOrders.Select(o => o.Id).ToList();
            var response = await DownloadsService.DownloadDocuments(ids);

            await FileDownloader.DownloadFromResponseAsync(response);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Wystąpił błąd: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
    private void GoToInvoice(string invoiceName)
    {
        if (string.IsNullOrWhiteSpace(invoiceName)) return;

        var encodedName = Uri.EscapeDataString(invoiceName);

        NavigationManager.NavigateTo($"/faktury?search={encodedName}");
    }
}

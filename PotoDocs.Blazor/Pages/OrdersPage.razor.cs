using BlazorDownloadFile;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PotoDocs.Blazor.Dialogs;
using PotoDocs.Blazor.Services;
using PotoDocs.Shared.Models;
using System.Globalization;
using System.Text.Json;

namespace PotoDocs.Blazor.Pages;

public partial class OrdersPage
{
    [Inject] private IOrderService OrderService { get; set; } = default!;
    [Inject] private IInvoiceService InvoiceService { get; set; } = default!;
    [Inject] private IBlazorDownloadFileService BlazorDownloadFileService { get; set; } = default!;
    [Inject] private IDownloadsService DownloadsService { get; set; } = default!;
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    private HashSet<OrderDto> selectedOrders = [];
    private bool IsLoading = true;
    private string searchString = "";
    private List<OrderDto> orders = [];
    private List<UserDto> users = [];
    private readonly List<BreadcrumbItem> _items = [new("Strona główna", href: "#"), new("Zlecenia", href: null, disabled: true)];
    private static readonly CultureInfo _plCulture = new("pl-PL");
    private readonly TableGroupDefinition<OrderDto> _groupDefinition = new()
    {
        Indentation = false,
        Expandable = true,
        IsInitiallyExpanded = false,
        Selector = (e) =>
        {
            string dateString = e.UnloadingDate?.ToString("MMMM - yyyy", new CultureInfo("pl-PL")) ?? "Brak daty";
            return new CultureInfo("pl-PL").TextInfo.ToTitleCase(dateString);
        }
    };
    private static string FormatMoney(decimal price, CurrencyType currency)
    {
        var culture = currency == CurrencyType.PLN
            ? CultureInfo.GetCultureInfo("pl-PL")
            : CultureInfo.GetCultureInfo("de-DE");

        return price.ToString("C", culture);
    }
    protected override async Task OnInitializedAsync()
    {
        _groupDefinition.Selector = GroupSelector;
        await LoadUsers();
        await LoadOrders();
    }
    private string GroupSelector(OrderDto e)
    {
        if (e.UnloadingDate is null)
            return "Brak daty";

        string dateString = e.UnloadingDate.Value.ToString("MMMM - yyyy", _plCulture);

        return _plCulture.TextInfo.ToTitleCase(dateString);
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
        if (order.Driver?.FirstAndLastName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false)
            return true;
        if (order.Company?.Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false)
            return true;
        return false;
    }

    private async Task Create()
    {
        var parameters = new DialogParameters
        {
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

        if (result is not null && !result.Canceled && result.Data is OrderDialogResult orderFormDialogResult)
        {
            try
            {
                await OrderService.Create(orderFormDialogResult.Order, orderFormDialogResult.OrderFiles, orderFormDialogResult.CmrFiles);

                Snackbar.Add("Pomyślnie zapisano zlecenie.", Severity.Success);

                await LoadOrders();
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
    private async Task DownloadDocuments()
    {
        if (selectedOrders.Count == 0)
        {
            Snackbar.Add("Zaznacz najpierw zlecenia", Severity.Warning);
            return;
        }
        try
        {
            IsLoading = true;
            List<Guid> orderIds = [.. selectedOrders.Select(o => o.Id)];
            var file = await DownloadsService.DownloadDocuments(orderIds);

            if (file.FileContent == null || file.FileContent.Length == 0)
            {
                Snackbar.Add("Nie znaleziono plików dla wybranych kryteriów.", Severity.Info);
                return;
            }

            await BlazorDownloadFileService.DownloadFile(file.FileName, file.FileContent, file.ContentType);
            Snackbar.Add($"Pobrano {file.FileName}", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    private async Task DownloadOrders()
    {
        if (selectedOrders.Count == 0)
        {
            Snackbar.Add("Zaznacz najpierw zlecenia", Severity.Warning);
            return;
        }
        try
        {
            IsLoading = true;
            List<Guid> orderIds = [.. selectedOrders.Select(o => o.Id)];
            var file = await DownloadsService.DownloadOrders(orderIds);

            if (file.FileContent == null || file.FileContent.Length == 0)
            {
                Snackbar.Add("Nie znaleziono plików dla wybranych kryteriów.", Severity.Info);
                return;
            }

            await BlazorDownloadFileService.DownloadFile(file.FileName, file.FileContent, file.ContentType);
            Snackbar.Add($"Pobrano {file.FileName}", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
}

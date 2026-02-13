using BlazorDownloadFile;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PotoDocs.Blazor.Dialogs;
using PotoDocs.Blazor.Helpers;
using PotoDocs.Blazor.Services;
using PotoDocs.Blazor.Services.Strategies;
using PotoDocs.Shared.Models;
using System.Text.Json;

namespace PotoDocs.Blazor.Pages;

public partial class InvoicesPage
{
    [Inject] private IInvoiceService InvoiceService { get; set; } = default!;
    [Inject] private IDownloadsService DownloadsService { get; set; } = default!;
    [Inject] private IFileDownloadHelper FileDownloader { get; set; } = default!;
    [Inject] private InvoiceStrategyFactory InvoiceStrategyFactory { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "search")]
    public string? searchString { get; set; }

    private readonly List<BreadcrumbItem> _items = [new("Strona główna", href: "#"), new("Faktury", href: null, disabled: true)];
    private readonly TableGroupSorter<InvoiceDto> _sorter = new(x => x.IssueDate, "Data wystawienia");
    private List<InvoiceDto> invoices = [];
    private HashSet<InvoiceDto> selectedInvoices = [];
    private bool IsLoading = true;

    private void OnSortDirectionChanged(SortDirection direction)
    {
        if (_sorter.UpdateDirection(direction))
        {
            StateHasChanged();
        }
    }
    protected override async Task OnInitializedAsync()
    {
        await LoadInvoices();
    }
    private async Task LoadInvoices()
    {
        try
        {
            IsLoading = true;
            invoices = [.. (await InvoiceService.GetAll())];
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Błąd podczas pobierania faktur: {ex.Message}", Severity.Error);
            invoices = [];
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool FilterFunc1(InvoiceDto invoice) => FilterFunc(invoice, searchString);

    private static bool FilterFunc(InvoiceDto invoice, string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return true;
        if (invoice.InvoiceNumber.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (invoice.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (invoice.Comments.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (invoice.BuyerName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false)
            return true;
        if (invoice.IssueDate?.ToString("dd.MM.yyyy").Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false)
            return true;
        if (invoice.TotalNetAmount.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    private async Task Create()
    {
        var parameters = new DialogParameters
        {
            { nameof(InvoiceDialog.Type), InvoiceFormType.Create }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            FullWidth = true,
            MaxWidth = MaxWidth.ExtraLarge
        };

        var dialogRef = await DialogService.ShowAsync<InvoiceDialog>("Dodawanie faktury", parameters, options);
        var result = await dialogRef.Result;

        if (result is not null && !result.Canceled && result.Data is InvoiceDto invoice)
        {
            try
            {
                await InvoiceService.Create(invoice);
                Snackbar.Add("Pomyślnie zapisano fakturę.", Severity.Success);
                await LoadInvoices();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Błąd przy zapisywaniu faktury: {ex.Message}", Severity.Error);
            }
        }
    }
    private async Task DownloadInvoices()
    {
        if (selectedInvoices.Count == 0)
        {
            Snackbar.Add("Zaznacz najpierw faktury", Severity.Warning);
            return;
        }

        var ids = selectedInvoices.Select(i => i.Id).ToList();

        await FileDownloader.DownloadFromServerAsync(() => DownloadsService.DownloadInvoices(ids), isLoading => IsLoading = isLoading);
    }
    private async Task HandleAction(Func<Task> action)
    {
        try
        {
            await action();
            await LoadInvoices();
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }
    private void GoToOrder(string orderName)
    {
        if (string.IsNullOrWhiteSpace(orderName)) return;
        var encodedName = Uri.EscapeDataString(orderName);
        NavigationManager.NavigateTo($"/zlecenia?search={encodedName}");
    }
}

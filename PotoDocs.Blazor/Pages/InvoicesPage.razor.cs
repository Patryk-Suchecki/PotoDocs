using BlazorDownloadFile;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PotoDocs.Blazor.Dialogs;
using PotoDocs.Blazor.Services;
using PotoDocs.Shared.Models;
using System.Globalization;
using System.Text.Json;

namespace PotoDocs.Blazor.Pages;

public partial class InvoicesPage
{
    [Inject] private IInvoiceService InvoiceService { get; set; } = default!;
    [Inject] private IDownloadsService DownloadsService { get; set; } = default!;
    [Inject] private IBlazorDownloadFileService BlazorDownloadFileService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    private HashSet<InvoiceDto> selectedInvoices = [];
    private bool IsLoading = true;
    private string searchString = "";
    private List<InvoiceDto> invoices = [];
    private readonly List<BreadcrumbItem> _items = [new("Strona główna", href: "#"), new("Faktury", href: null, disabled: true)];

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
    private readonly TableGroupDefinition<InvoiceDto> _groupDefinition = new()
    {
        Indentation = false,
        Expandable = true,
        IsInitiallyExpanded = false,
        Selector = (e) =>
        {
            string dateString = e.IssueDate?.ToString("MMMM - yyyy", new CultureInfo("pl-PL")) ?? "Brak daty";
            return new CultureInfo("pl-PL").TextInfo.ToTitleCase(dateString);
        }
    };

    private bool FilterFunc1(InvoiceDto invoice) => FilterFunc(invoice, searchString);

    private static bool FilterFunc(InvoiceDto invoice, string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return true;
        if (invoice.InvoiceNumber.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (invoice.BuyerName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false)
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

    private async Task Edit(InvoiceDto invoice)
    {
        var parameters = new DialogParameters
        {
            { nameof(InvoiceDialog.InvoiceDto), JsonSerializer.Deserialize<InvoiceDto>(JsonSerializer.Serialize(invoice))! },
            { nameof(InvoiceDialog.Type), InvoiceFormType.Update }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            FullWidth = true,
            MaxWidth = MaxWidth.ExtraLarge
        };

        var dialogRef = await DialogService.ShowAsync<InvoiceDialog>($"Edytuj fakture nr {invoice.Name}", parameters, options);
        var result = await dialogRef.Result;

        if (result is not null && !result.Canceled && result.Data is InvoiceDto dto)
        {
            try
            {
                await InvoiceService.Update(dto);
                Snackbar.Add("Pomyślnie zaktualizowano fakturę.", Severity.Success);
                await LoadInvoices();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Błąd przy aktualizacji faktury: {ex.Message}", Severity.Error);
            }
        }
    }

    private async Task Delete(InvoiceDto invoice)
    {
        var parameters = new DialogParameters
        {
            { nameof(InvoiceDialog.InvoiceDto), invoice },
            { nameof(InvoiceDialog.Type), InvoiceFormType.Delete }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            FullWidth = true,
            MaxWidth = MaxWidth.ExtraLarge
        };

        var dialogRef = await DialogService.ShowAsync<InvoiceDialog>($"Usuń fakturę nr {invoice.Name}", parameters, options);
        var result = await dialogRef.Result;

        if (result is not null && !result.Canceled)
        {
            try
            {
                await InvoiceService.Delete(invoice.Id);
                Snackbar.Add($"Pomyślnie usunięto fakturę {invoice.Name}", Severity.Success);
                invoices?.Remove(invoice);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }

    private async Task Details(InvoiceDto invoice)
    {
        var parameters = new DialogParameters
        {
            { nameof(InvoiceDialog.InvoiceDto), invoice },
            { nameof(InvoiceDialog.Type), InvoiceFormType.Details }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            FullWidth = true,
            MaxWidth = MaxWidth.ExtraLarge
        };

        await DialogService.ShowAsync<InvoiceDialog>($"Szczegóły zlecenia nr {invoice.Name}", parameters, options);
    }

    private async Task Download(InvoiceDto invoice)
    {
        try
        {
            var file = await InvoiceService.Download(invoice.Id);
            await BlazorDownloadFileService.DownloadFile(file.FileName, file.FileContent, file.ContentType);
            Snackbar.Add($"Pobrano {file.FileName}", Severity.Info);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }
    private async Task DownloadInvoices()
    {
        if (selectedInvoices.Count == 0)
        {
            Snackbar.Add("Zaznacz najpierw faktury", Severity.Warning);
            return;
        }
        try
        {
            IsLoading = true;
            List<Guid> invoiceIds = [.. selectedInvoices.Select(i => i.Id)];
            var file = await DownloadsService.DownloadInvoices(invoiceIds);

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

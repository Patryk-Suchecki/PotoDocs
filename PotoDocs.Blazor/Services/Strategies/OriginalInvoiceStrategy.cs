using MudBlazor;
using PotoDocs.Blazor.Dialogs;
using PotoDocs.Blazor.Helpers;
using PotoDocs.Shared.Models;
using System.Text.Json;

namespace PotoDocs.Blazor.Services.Strategies;

public class OriginalInvoiceStrategy(IDialogService dialogService, IInvoiceService invoiceService, IFileDownloadHelper fileDownloader, ISnackbar snackbar) : IInvoiceActionStrategy
{
    private readonly IDialogService _dialogService = dialogService;
    private readonly IInvoiceService _invoiceService = invoiceService;
    private readonly IFileDownloadHelper _fileDownloader = fileDownloader;
    private readonly ISnackbar _snackbar = snackbar;

    public InvoiceType Type => InvoiceType.Original;

    public bool CanEdit(InvoiceDto invoice) => true; //invoice.SentDate == null;
    public bool CanDelete(InvoiceDto invoice) => invoice.SentDate == null;
    public bool CanCorrect(InvoiceDto invoice) => invoice.Corrections.Count == 0;

    public async Task EditAsync(InvoiceDto invoice)
    {
        var parameters = new DialogParameters
        {
            { nameof(InvoiceDialog.InvoiceDto), JsonSerializer.Deserialize<InvoiceDto>(JsonSerializer.Serialize(invoice))! },
            { nameof(InvoiceDialog.Type), InvoiceFormType.Update }
        };

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, FullWidth = true };
        var dialog = await _dialogService.ShowAsync<InvoiceDialog>($"Edytuj fakturę {invoice.Name}", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is InvoiceDto dto)
        {
            await _invoiceService.Update(dto);
            _snackbar.Add("Zaktualizowano fakturę.", Severity.Success);
        }
    }

    public async Task DeleteAsync(InvoiceDto invoice)
    {
        var parameters = new DialogParameters
        {
            { nameof(InvoiceDialog.InvoiceDto), invoice },
            { nameof(InvoiceDialog.Type), InvoiceFormType.Delete }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, FullWidth = true };
        var dialog = await _dialogService.ShowAsync<InvoiceDialog>($"Usuń fakturę {invoice.Name}", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await _invoiceService.Delete(invoice.Id);
            _snackbar.Add("Usunięto fakturę.", Severity.Success);
        }
    }

    public async Task CorrectAsync(InvoiceDto invoice)
    {
        var parameters = new DialogParameters
        {
            { nameof(InvoiceCorrectionDialog.Type), InvoiceCorrectionFormType.Create },
            { nameof(InvoiceCorrectionDialog.InvoiceCorrectionDto), new InvoiceCorrectionDto() { OriginalInvoice = invoice } }
        };

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, FullWidth = true };
        var dialog = await _dialogService.ShowAsync<InvoiceCorrectionDialog>($"Korekta faktury {invoice.Name}", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is InvoiceCorrectionDto correction)
        {
            await _invoiceService.CreateCorrection(correction);
            _snackbar.Add("Stworzono korektę.", Severity.Success);
        }
    }

    public async Task DetailsAsync(InvoiceDto invoice)
    {
        var parameters = new DialogParameters
        {
            { nameof(InvoiceDialog.InvoiceDto), invoice },
            { nameof(InvoiceDialog.Type), InvoiceFormType.Details }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, FullWidth = true };
        await _dialogService.ShowAsync<InvoiceDialog>($"Szczegóły faktury {invoice.Name}", parameters, options);
    }

    public async Task DownloadAsync(InvoiceDto invoice)
    {
        var response = await _invoiceService.Download(invoice.Id);
        await _fileDownloader.DownloadFromResponseAsync(response);
    }
}

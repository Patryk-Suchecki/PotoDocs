using MudBlazor;
using PotoDocs.Blazor.Dialogs;
using PotoDocs.Blazor.Helpers;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Services.Strategies;

public class CorrectionStrategy(IDialogService dialogService, IInvoiceService invoiceService, IFileDownloadHelper fileDownloader, ISnackbar snackbar) : IInvoiceActionStrategy
{
    private readonly IDialogService _dialogService = dialogService;
    private readonly IInvoiceService _invoiceService = invoiceService;
    private readonly IFileDownloadHelper _fileDownloader = fileDownloader;
    private readonly ISnackbar _snackbar = snackbar;
    public InvoiceType Type => InvoiceType.Correction;

    public bool CanEdit(InvoiceDto invoice) => invoice.SentDate == null;
    public bool CanDelete(InvoiceDto invoice) => invoice.SentDate == null;
    public bool CanCorrect(InvoiceDto invoice) => false;

    public async Task EditAsync(InvoiceDto invoice)
    {
        var parameters = new DialogParameters
        {
            { nameof(InvoiceCorrectionDialog.InvoiceCorrectionDto), invoice.ToCorrectionDto() },
            { nameof(InvoiceCorrectionDialog.Type), InvoiceCorrectionFormType.Update }
        };

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, FullWidth = true };
        var dialog = await _dialogService.ShowAsync<InvoiceCorrectionDialog>($"Edytuj korektę {invoice.Name}", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is InvoiceCorrectionDto dto)
        {
            await _invoiceService.UpdateCorrection(dto);
            _snackbar.Add("Zaktualizowano korektę.", Severity.Success);
        }
    }

    public Task CorrectAsync(InvoiceDto invoice) => Task.CompletedTask;

    public async Task DeleteAsync(InvoiceDto invoice)
    {
        var parameters = new DialogParameters
        {
            { nameof(InvoiceCorrectionDialog.InvoiceCorrectionDto), invoice.ToCorrectionDto() },
            { nameof(InvoiceCorrectionDialog.Type), InvoiceCorrectionFormType.Delete }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, FullWidth = true };
        var dialog = await _dialogService.ShowAsync<InvoiceCorrectionDialog>($"Usuń korektę {invoice.Name}", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await _invoiceService.Delete(invoice.Id);
            _snackbar.Add("Usunięto korektę.", Severity.Success);
        }
    }

    public async Task DetailsAsync(InvoiceDto invoice)
    {
        var parameters = new DialogParameters
        {
            { nameof(InvoiceCorrectionDialog.InvoiceCorrectionDto), invoice.ToCorrectionDto() },
            { nameof(InvoiceCorrectionDialog.Type), InvoiceCorrectionFormType.Details }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, FullWidth = true };
        await _dialogService.ShowAsync<InvoiceCorrectionDialog>($"Szczegóły korekty {invoice.Name}", parameters, options);
    }

    public async Task DownloadAsync(InvoiceDto invoice)
    {
        var response = await _invoiceService.Download(invoice.Id);
        await _fileDownloader.DownloadFromResponseAsync(response);
    }

}

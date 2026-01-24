using PotoDocs.Shared.Models;
namespace PotoDocs.Blazor.Services.Strategies;

public interface IInvoiceActionStrategy
{
    InvoiceType Type { get; }

    bool CanEdit(InvoiceDto invoice);
    bool CanDelete(InvoiceDto invoice);
    bool CanCorrect(InvoiceDto invoice);

    Task EditAsync(InvoiceDto invoice);
    Task DeleteAsync(InvoiceDto invoice);
    Task DetailsAsync(InvoiceDto invoice);
    Task DownloadAsync(InvoiceDto invoice);
    Task CorrectAsync(InvoiceDto invoice);
}


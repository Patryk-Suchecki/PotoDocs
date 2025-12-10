using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Services;

public interface IDownloadsService
{
    Task<FileDownloadResult> DownloadInvoices(List<Guid> invoiceIds);
    Task<FileDownloadResult> DownloadDocuments(List<Guid> orderIds);
    Task<FileDownloadResult> DownloadOrders(List<Guid> orderIds);
}

public class DownloadsService(IAuthService authService) : BaseService(authService), IDownloadsService
{
    public async Task<FileDownloadResult> DownloadInvoices(List<Guid> invoiceIds)
    {
        return await PostAndDownloadFileAsync($"api/downloads/invoices/", invoiceIds);
    }
    public async Task<FileDownloadResult> DownloadDocuments(List<Guid> orderIds)
    {
        return await PostAndDownloadFileAsync($"api/downloads/documents/", orderIds);
    }
    public async Task<FileDownloadResult> DownloadOrders(List<Guid> orderIds)
    {
        return await PostAndDownloadFileAsync($"api/downloads/orders/", orderIds);
    }
}
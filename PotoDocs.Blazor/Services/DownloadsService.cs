using PotoDocs.Shared.Models;
using System.Net.Http.Json;

namespace PotoDocs.Blazor.Services;

public interface IDownloadsService
{
    Task<HttpResponseMessage> DownloadInvoices(List<Guid> invoiceIds);
    Task<HttpResponseMessage> DownloadDocuments(List<Guid> orderIds);
    Task<HttpResponseMessage> DownloadOrders(List<Guid> orderIds);
}

public class DownloadsService(HttpClient http) : IDownloadsService
{
    private readonly HttpClient _http = http;

    public async Task<HttpResponseMessage> DownloadInvoices(List<Guid> invoiceIds)
    {
        var request = new DownloadRequest { Ids = invoiceIds };
        return await _http.PostAsJsonAsync("api/downloads/invoices", request);
    }

    public async Task<HttpResponseMessage> DownloadDocuments(List<Guid> orderIds)
    {
        var request = new DownloadRequest { Ids = orderIds };
        return await _http.PostAsJsonAsync("api/downloads/documents", request);
    }

    public async Task<HttpResponseMessage> DownloadOrders(List<Guid> orderIds)
    {
        var request = new DownloadRequest { Ids = orderIds };
        return await _http.PostAsJsonAsync("api/downloads/orders", request);
    }
}
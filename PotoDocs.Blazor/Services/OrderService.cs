using System.Net.Http.Headers;
using System.Net.Http.Json;
using PotoDocs.Shared.Models;
using PotoDocs.Blazor.Helpers;

namespace PotoDocs.Blazor.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAll(int page = 1, int pageSize = 5, string? driverEmail = null);
    Task<OrderDto?> GetById(Guid id);
    Task Delete(Guid id);
    Task<OrderDto?> Create(byte[] fileData, string filename);
    Task Update(OrderDto dto, Guid id);
    Task<byte[]> DownloadInvoices(int year, int month);
    Task<byte[]> DownloadFile(Guid id, string fileName);
    Task<byte[]> DownloadInvoice(Guid id);
    Task UploadCMR(List<byte[]> filesData, Guid id, string fileName);
    Task RemoveCMR(Guid id, string fileName);
    Task<Dictionary<int, List<int>>> GetAvailableYearsAndMonthsAsync();
}

public class OrderService : IOrderService
{
    private readonly IAuthService _authService;

    public OrderService(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<IEnumerable<OrderDto>> GetAll(int page = 1, int pageSize = 5, string? driverEmail = null)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var query = $"api/order/all?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(driverEmail))
        {
            query += $"&driverEmail={Uri.EscapeDataString(driverEmail)}";
        }

        var response = await httpClient.GetAsync(query);
        await response.ThrowIfNotSuccessWithProblemDetails();

        return await response.Content.ReadFromJsonAsync<IEnumerable<OrderDto>>() ?? new List<OrderDto>();
    }

    public async Task<OrderDto?> GetById(Guid id)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.GetAsync($"api/order/{id}");
        await response.ThrowIfNotSuccessWithProblemDetails();

        return await response.Content.ReadFromJsonAsync<OrderDto>();
    }

    public async Task Delete(Guid id)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.DeleteAsync($"api/order/{id}");
        await response.ThrowIfNotSuccessWithProblemDetails();
    }

    public async Task<OrderDto?> Create(byte[] fileData, string fileName)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        using var multipartFormContent = new MultipartFormDataContent();
        var streamContent = new ByteArrayContent(fileData);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

        multipartFormContent.Add(streamContent, "file", fileName);

        var response = await httpClient.PostAsync("api/order", multipartFormContent);
        await response.ThrowIfNotSuccessWithProblemDetails();

        return await response.Content.ReadFromJsonAsync<OrderDto>();
    }

    public async Task Update(OrderDto dto, Guid id)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.PutAsJsonAsync($"api/order/{id}", dto);
        await response.ThrowIfNotSuccessWithProblemDetails();
    }

    public async Task<byte[]> DownloadInvoices(int year, int month)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.GetAsync($"api/orders/invoices/{year}/{month}");
        await response.ThrowIfNotSuccessWithProblemDetails();

        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<byte[]> DownloadFile(Guid id, string fileName)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.GetAsync($"api/orders/{id}/pdf/{fileName}");
        await response.ThrowIfNotSuccessWithProblemDetails();

        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<byte[]> DownloadInvoice(Guid id)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.GetAsync($"api/orders/{id}/invoice");
        await response.ThrowIfNotSuccessWithProblemDetails();

        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task UploadCMR(List<byte[]> filesData, Guid id, string fileName)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        using var multipartContent = new MultipartFormDataContent();

        foreach (var fileData in filesData)
        {
            var fileContent = new ByteArrayContent(fileData);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            multipartContent.Add(fileContent, "files", fileName);
        }

        var response = await httpClient.PostAsync($"api/orders/{id}/cmr", multipartContent);
        await response.ThrowIfNotSuccessWithProblemDetails();
    }

    public async Task RemoveCMR(Guid id, string fileName)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.DeleteAsync($"api/orders/{id}/cmr/{fileName}");
        await response.ThrowIfNotSuccessWithProblemDetails();
    }

    public async Task<Dictionary<int, List<int>>> GetAvailableYearsAndMonthsAsync()
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        return await httpClient.GetFromJsonAsync<Dictionary<int, List<int>>>("api/orders/invoices")
               ?? new Dictionary<int, List<int>>();
    }
}

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAll(int page = 1, int pageSize = 5, string? driverEmail = null);
    Task<OrderDto> GetById(Guid id);
    Task Delete(Guid id);
    Task<OrderDto> Create(byte[] fileData, string filename);
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

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<OrderDto>>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return apiResponse?.Data;
        }

        throw new Exception($"API call failed with status code {response.StatusCode} and message {await response.Content.ReadAsStringAsync()}");
    }
    public async Task<OrderDto> GetById(Guid id)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync($"api/order/{id}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<OrderDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return apiResponse.Data;
        }

        return null;
    }
    public async Task Delete(Guid id)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.DeleteAsync($"api/order/{id}");
    }
    public async Task<OrderDto> Create(byte[] fileData, string fileName)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        using (var multipartFormContent = new MultipartFormDataContent())
        {
            var streamContent = new ByteArrayContent(fileData);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            multipartFormContent.Add(streamContent, "file", fileName);

            var response = await httpClient.PostAsync("api/order", multipartFormContent);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<OrderDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return apiResponse.Data;
            }
            else
            {
                throw new Exception($"Błąd API: {response.ReasonPhrase}");
            }
        }
    }

    public async Task Update(OrderDto dto, Guid id)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.PutAsJsonAsync($"api/order/{id}", dto);
    }
    public async Task<byte[]> DownloadInvoices(int year, int month)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync($"api/orders/invoices/{year}/{month}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsByteArrayAsync();
        }
        else
        {
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Błąd pobierania faktur: {response.StatusCode} - {errorMessage}");
        }
    }

    public async Task<byte[]> DownloadFile(Guid id, string fileName)
    {
        string archiveFileName = $"{fileName}";

        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync($"api/orders/{id}/pdf/{fileName}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsByteArrayAsync();
        }

        return null;
    }
    public async Task<byte[]> DownloadInvoice(Guid id)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.GetAsync($"api/orders/{id}/invoice");
        if (response.IsSuccessStatusCode)
        {
            var fileBytes = await response.Content.ReadAsByteArrayAsync();

            return fileBytes;
        }

        return null;
    }

    public async Task UploadCMR(List<byte[]> filesData, Guid id, string fileName)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        using var multipartContent = new MultipartFormDataContent();

        for (int i = 0; i < filesData.Count; i++)
        {
            var fileContent = new ByteArrayContent(filesData[i]);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            multipartContent.Add(fileContent, "files", fileName);
        }

        var response = await httpClient.PostAsync($"api/orders/{id}/cmr", multipartContent);
    }

    public async Task RemoveCMR(Guid id, string pdfname)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.DeleteAsync($"api/orders/{id}/cmr/{pdfname}");
    }
    public async Task<Dictionary<int, List<int>>> GetAvailableYearsAndMonthsAsync()
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.GetFromJsonAsync<Dictionary<int, List<int>>>($"api/orders/invoices");
        return response ?? new Dictionary<int, List<int>>();
    }
}

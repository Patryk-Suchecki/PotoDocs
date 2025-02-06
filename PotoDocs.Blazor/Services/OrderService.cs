using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAll(int page = 1, int pageSize = 5, string? driverEmail = null);
    Task<OrderDto> GetById(int invoiceNumber);
    Task Delete(int invoiceNumber);
    Task<OrderDto> Create(byte[] fileData);
    Task Update(OrderDto dto, int invoiceNumber);
    Task<byte[]> DownloadInvoices(DownloadDto downloadRequestDto);
    Task<byte[]> DownloadFile(int invoiceNumber, string fileName);
    Task<byte[]> DownloadInvoice(int invoiceNumber);
    Task<OrderDto> UploadCMR(List<string> filePaths, int invoiceNumber);
    Task RemoveCMR(int invoiceNumber, string pdfname);
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
    public async Task<OrderDto> GetById(int invoiceNumber)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync($"api/order/{invoiceNumber}");
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
    public async Task Delete(int invoiceNumber)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.DeleteAsync($"api/order/{invoiceNumber}");
    }
    public async Task<OrderDto> Create(byte[] fileData)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        using (var multipartFormContent = new MultipartFormDataContent())
        {
            var streamContent = new ByteArrayContent(fileData);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            multipartFormContent.Add(streamContent, "file", "document.pdf");

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

    public async Task Update(OrderDto dto, int invoiceNumber)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.PutAsJsonAsync($"api/order/{invoiceNumber}", dto);
    }
    public async Task<byte[]> DownloadInvoices(DownloadDto downloadRequestDto)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync($"invoices/{downloadRequestDto.Year}/{downloadRequestDto.Month}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsByteArrayAsync();
        }

        return null;
    }
    public async Task<byte[]> DownloadFile(int invoiceNumber, string fileName)
    {
        string archiveFileName = $"{fileName}";

        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync($"api/orders/{invoiceNumber}/pdf/{fileName}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsByteArrayAsync();
        }

        return null;
    }
    public async Task<byte[]> DownloadInvoice(int invoiceNumber)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync($"api/orders/{invoiceNumber}/invoice");
        if (response.IsSuccessStatusCode)
        {

            return await response.Content.ReadAsByteArrayAsync();
        }

        return null;
    }
    public async Task<OrderDto> UploadCMR(List<string> filePaths, int invoiceNumber)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        using (var multipartFormContent = new MultipartFormDataContent())
        {
            foreach (var filePath in filePaths)
            {
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                multipartFormContent.Add(streamContent, "files", Path.GetFileName(filePath));
            }

            var response = await httpClient.PostAsync($"api/orders/{invoiceNumber}/cmr", multipartFormContent);

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
    }
    public async Task RemoveCMR(int invoiceNumber, string pdfname)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.DeleteAsync($"api/orders/{invoiceNumber}/cmr/{pdfname}");
    }
}

using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace PotoDocs.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAll();
    Task<OrderDto> GetById(int invoiceNumber);
    Task Delete(int invoiceNumber);
    Task<OrderDto> Create(string filePath);
    Task Update(OrderDto dto, int invoiceNumber);
    Task<string> DownloadInvoices(DownloadDto downloadRequestDto);
    Task<string> DownloadFile(int invoiceNumber, string fileName);
    Task<string> DownloadInvoice(int invoiceNumber);
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

    public async Task<IEnumerable<OrderDto>> GetAll()
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync("api/order/all");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<OrderDto>>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return apiResponse.Data;
        }

        return null;
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
        var toast = Toast.Make("Zlecenie zostało usunięte.", ToastDuration.Short, 5);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            toast = Toast.Make("Błąd: " + errorContent, ToastDuration.Short, 5);
        }
        await toast.Show();
    }
    public async Task<OrderDto> Create(string filePath)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        using (var multipartFormContent = new MultipartFormDataContent())
        {
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            multipartFormContent.Add(streamContent, "file", Path.GetFileName(filePath));

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
            return null;
        }
    }
    public async Task Update(OrderDto dto, int invoiceNumber)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.PutAsJsonAsync($"api/order/{invoiceNumber}", dto);

        var toast = Toast.Make("Nowe hasło zostało wygenerowane.", ToastDuration.Short, 5);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            toast = Toast.Make("Błąd: " + errorContent, ToastDuration.Short, 5);
        }
        await toast.Show();
    }
    public async Task<string> DownloadInvoices(DownloadDto downloadRequestDto)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync($"invoices/{downloadRequestDto.Year}/{downloadRequestDto.Month}");
        if (response.IsSuccessStatusCode)
        {
            var rarData = await response.Content.ReadAsByteArrayAsync();
            string fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');
            string outputPath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(outputPath, rarData);
            return outputPath;
        }

        return null;
    }
    public async Task<string> DownloadFile(int invoiceNumber, string fileName)
    {
        string archiveFileName = $"{fileName}";
        string outputPath = Path.Combine(FileSystem.CacheDirectory, archiveFileName);

        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync($"api/orders/{invoiceNumber}/pdf/{fileName}");
        if (response.IsSuccessStatusCode)
        {
            var rarData = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(outputPath, rarData);
            return outputPath;
        }

        return null;
    }
    public async Task<string> DownloadInvoice(int invoiceNumber)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync($"api/orders/{invoiceNumber}/invoice");
        if (response.IsSuccessStatusCode)
        {

            var rarData = await response.Content.ReadAsByteArrayAsync();
            string fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');
            string outputPath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(outputPath, rarData);
            return outputPath;
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

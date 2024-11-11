using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PotoDocs.Services;

public class OrderService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthService _authService;
    private List<OrderDto> _orderList;

    public OrderService(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClientFactory = httpClientFactory;
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
        else
        {
            var statusCode = response.StatusCode;
        }
        return null;
    }
    public async Task<string?> Delete(int invoiceNumber)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.DeleteAsync($"api/order/{invoiceNumber}");
        if (response.IsSuccessStatusCode)
        {
            return "Ok";
        }
        else
        {
            var statusCode = response.StatusCode;
        }
        return null;
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
            else
            {
                var statusCode = response.StatusCode;
                return null;
            }
        }
    }
    public async Task<string?> Update(OrderDto dto, int invoiceNumber)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.PutAsJsonAsync($"api/order/{invoiceNumber}", dto);

        if (response.IsSuccessStatusCode)
        {
            return "Zaktualizowano";
        }
        else
        {
            return response.StatusCode.ToString();
        }
    }
    public async Task<string> DownloadInvoices(DownloadDto downloadRequestDto)
    {
        string archiveFileName = $"Zlecenia_{downloadRequestDto.Month}-{downloadRequestDto.Year}.rar";
        string outputPath = Path.Combine(FileSystem.CacheDirectory, archiveFileName);

        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync($"invoices/{downloadRequestDto.Year}/{downloadRequestDto.Month}");
        if (response.IsSuccessStatusCode)
        {
            var rarData = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(outputPath, rarData);
        }
        else
        {
            var statusCode = response.StatusCode;
        }
        return outputPath;
    }
    public async Task<string> DownloadInvoice(string fileName)
    {
        string archiveFileName = $"{fileName}";
        string outputPath = Path.Combine(FileSystem.CacheDirectory, archiveFileName);

        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync($"api/order/pdf/{fileName}");
        if (response.IsSuccessStatusCode)
        {
            var rarData = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(outputPath, rarData);
        }
        else
        {
            var statusCode = response.StatusCode;
        }
        return outputPath;
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

            var response = await httpClient.PostAsync($"api/order/{invoiceNumber}/cmr", multipartFormContent);

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
                var statusCode = response.StatusCode;
                Debug.WriteLine($"Błąd przesyłania plików: {statusCode}");
                return null;
            }
        }
    }

    public async Task RemoveCMR(string pdfname)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.DeleteAsync($"api/order/pdf/{pdfname}");
    }
}

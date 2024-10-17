using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PotoDocs.Services;

public class OrderService
{
    private readonly HttpClient _httpClient;
    private List<OrderDto> _orderList;

    public OrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<OrderDto>> GetOrders()
    {
        if (_orderList?.Count > 0)
            return _orderList;

        var jsonOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var response = await _httpClient.GetAsync(AppConstants.ApiUrl + "api/order/all");
            if (response.IsSuccessStatusCode)
            {
                _orderList = await response.Content.ReadFromJsonAsync<List<OrderDto>>(jsonOptions);
                return _orderList;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas pobierania danych online: {ex.Message}");
        }

        return _orderList;
    }
    public async Task<OrderDto> UploadFile(string filePath)
    {
        using (var multipartFormContent = new MultipartFormDataContent())
        {
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            multipartFormContent.Add(streamContent, "file", Path.GetFileName(filePath));

            var response = await _httpClient.PostAsync(AppConstants.ApiUrl + "api/order", multipartFormContent);

            if (response.IsSuccessStatusCode)
            {
                OrderDto order = await response.Content.ReadFromJsonAsync<OrderDto>();

                return order;
            }
            else
            {
                return null;
            }
        }
    }
    public async Task<string?> CreateOrder(OrderDto dto)
    {
        var result = await _httpClient.PostAsJsonAsync(AppConstants.ApiUrl + "api/order", dto);

        if (result.IsSuccessStatusCode)
        {
            int orderId = await result.Content.ReadFromJsonAsync<int>();

            return "orderId";
        }
        else
        {
            return null;
        }
    }
    public async Task<string> DownloadInvoices(DownloadDto downloadRequestDto)
    {
        string archiveFileName = $"Zlecenia_{downloadRequestDto.Month}-{downloadRequestDto.Year}.rar";
        string outputPath = Path.Combine(FileSystem.CacheDirectory, archiveFileName);

        var response = await _httpClient.GetAsync($"{AppConstants.ApiUrl}/invoices/{downloadRequestDto.Year}/{downloadRequestDto.Month}");

        if (response.IsSuccessStatusCode)
        {
            var rarData = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(outputPath, rarData);
        }
        else
        {
            throw new Exception("Nie udało się pobrać archiwum RAR.");
        }

        return outputPath;
    }


}

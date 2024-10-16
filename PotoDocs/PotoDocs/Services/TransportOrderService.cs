using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PotoDocs.Services;

public class TransportOrderService
{
    private readonly HttpClient _httpClient;
    private List<TransportOrderDto> _transportOrderList;


    public TransportOrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<TransportOrderDto>> GetTransportOrders()
    {
        if (_transportOrderList?.Count > 0)
            return _transportOrderList;

        var jsonOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var response = await _httpClient.GetAsync(AppConstants.ApiUrl + "api/transportorder/all");
            if (response.IsSuccessStatusCode)
            {
                _transportOrderList = await response.Content.ReadFromJsonAsync<List<TransportOrderDto>>(jsonOptions);
                return _transportOrderList;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas pobierania danych online: {ex.Message}");
        }

        return _transportOrderList;
    }
    public async Task<TransportOrderDto> UploadFile(string filePath)
    {
        using (var multipartFormContent = new MultipartFormDataContent())
        {
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            multipartFormContent.Add(streamContent, "file", Path.GetFileName(filePath));

            var response = await _httpClient.PostAsync(AppConstants.ApiUrl + "api/transportorder", multipartFormContent);

            if (response.IsSuccessStatusCode)
            {
                TransportOrderDto transportOrder = await response.Content.ReadFromJsonAsync<TransportOrderDto>();

                return transportOrder;
            }
            else
            {
                return null;
            }
        }
    }
    public async Task<string?> CreateTransportOrder(TransportOrderDto dto)
    {
        var result = await _httpClient.PostAsJsonAsync(AppConstants.ApiUrl + "api/transportorder", dto);

        if (result.IsSuccessStatusCode)
        {
            int transportOrderId = await result.Content.ReadFromJsonAsync<int>();

            return "transportOrderId";
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

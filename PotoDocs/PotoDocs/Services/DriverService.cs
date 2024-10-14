using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PotoDocs.Services;

public class DriverService
{
    private readonly HttpClient _httpClient;
    private List<RegisterUserDto> _driverList;


    public DriverService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<RegisterUserDto>> GetDrivers()
    {
        if (_driverList?.Count > 0)
            return _driverList;

        var jsonOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var response = await _httpClient.GetAsync(AppConstants.ApiUrl + "/drivers/all");
            if (response.IsSuccessStatusCode)
            {
                _driverList = await response.Content.ReadFromJsonAsync<List<RegisterUserDto>>(jsonOptions);
                return _driverList;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas pobierania danych online: {ex.Message}");
        }

        return _driverList;
    }
}
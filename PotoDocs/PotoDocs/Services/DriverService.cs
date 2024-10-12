using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PotoDocs.Services;

public class DriverService
{
    private readonly HttpClient _httpClient;
    private List<DriverDto> _driverList;


    public DriverService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<DriverDto>> GetDrivers(bool isOnline = false)
    {
        if (_driverList?.Count > 0)
            return _driverList;

        var jsonOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNameCaseInsensitive = true
        };

        if (isOnline)
        {
            try
            {
                var response = await _httpClient.GetAsync(AppConstants.ApiUrl + "/drivers/all");
                if (response.IsSuccessStatusCode)
                {
                    _driverList = await response.Content.ReadFromJsonAsync<List<DriverDto>>(jsonOptions);
                    return _driverList;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas pobierania danych online: {ex.Message}");
            }
        }

        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("drivers.json");
            using var reader = new StreamReader(stream);
            var contents = await reader.ReadToEndAsync();

            _driverList = JsonSerializer.Deserialize<List<DriverDto>>(contents, jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas odczytu danych offline: {ex.Message}");
            return new List<DriverDto>();
        }

        return _driverList;
    }
}
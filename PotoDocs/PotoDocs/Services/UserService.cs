using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PotoDocs.Services;

public class UserService
{
    HttpClient _httpClient;
    List<UserDto> _userList;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<string?> RegisterAsync(UserDto dto)
    {
        var result = await _httpClient.PostAsJsonAsync(AppConstants.ApiUrl + "/api/account/register", dto);
        if (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<LoginResponseDto>();
        }
        else
        {
            return "Error in logging in";
        }

        return null;
    }
    public async Task<string?> ResetPassword(int userId)
    {
        var result = await _httpClient.PostAsJsonAsync(AppConstants.ApiUrl + "/api/account/resetpassword", userId);
        if (result.IsSuccessStatusCode)
        {

        }
        else
        {
            return "Error in logging in";
        }

        return null;
    }
    public async Task<List<UserDto>> GetUsers()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var response = await _httpClient.GetAsync(AppConstants.ApiUrl + "api/account/all");
            if (response.IsSuccessStatusCode)
            {
                _userList = await response.Content.ReadFromJsonAsync<List<UserDto>>(jsonOptions);
                return _userList;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas pobierania danych online: {ex.Message}");
        }

        return _userList;
    }
}

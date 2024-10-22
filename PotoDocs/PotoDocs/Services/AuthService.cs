using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PotoDocs.Services;

public class AuthService
{
    HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> IsUserAuthenticated()
    {
        var serializedData = await SecureStorage.Default.GetAsync(AppConstants.AuthStorageKeyName);
        return !string.IsNullOrWhiteSpace(serializedData);
    }

    public async Task<LoginResponseDto?> GetAuthenticatedUserAsync()
    {
        var serializedData = await SecureStorage.Default.GetAsync(AppConstants.AuthStorageKeyName);
        if (!string.IsNullOrWhiteSpace(serializedData))
        {
            return JsonSerializer.Deserialize<LoginResponseDto>(serializedData);
        }
        return null;
    }

    public async Task<string?> LoginAsync(LoginDto dto)
    {
        var result = await _httpClient.PostAsJsonAsync(AppConstants.ApiUrl + "api/account/login", dto);
        if (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (response != null)
            {
                var serializeResponse = JsonSerializer.Serialize(response);
                await SecureStorage.Default.SetAsync(AppConstants.AuthStorageKeyName, serializeResponse);
            }
        }
        else
        {
            return "Error in logging in";
        }

        return null;
    }

    public void Logout() => SecureStorage.Default.Remove(AppConstants.AuthStorageKeyName);
}

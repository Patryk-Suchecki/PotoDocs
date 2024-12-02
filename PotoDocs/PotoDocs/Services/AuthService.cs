using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace PotoDocs.Services;

public interface IAuthService
{
    Task<bool> IsUserAuthenticated();
    Task<string?> LoginAsync(LoginDto dto);
    Task<LoginResponseDto?> GetAuthenticatedUserAsync();
    Task<HttpClient> GetAuthenticatedHttpClientAsync();
    Task RegisterAsync(UserDto dto);
    Task<IEnumerable<string>> GetRoles();
    void Logout();
    Task<IEnumerable<UserDto>> GetAll();
    Task GeneratePassword(string email);
}

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> IsUserAuthenticated()
    {
        var serializedData = await SecureStorage.Default.GetAsync(AppConstants.AuthStorageKeyName);

        if (string.IsNullOrWhiteSpace(serializedData)) return false;

        var user = JsonSerializer.Deserialize<LoginResponseDto>(serializedData);

        return user != null && !IsTokenExpired(user.Token);
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
        var httpClient = _httpClientFactory.CreateClient(AppConstants.HttpClientName);
        var response = await httpClient.PostAsJsonAsync<LoginDto> ("api/account/login", dto);

        var content = await response.Content.ReadAsStringAsync();
        var authResponse =JsonSerializer.Deserialize<ApiResponse<LoginResponseDto>>(content, new JsonSerializerOptions 
        {
            PropertyNameCaseInsensitive = true
        });
        if (authResponse.Status)
        {
            var serializedData = JsonSerializer.Serialize(authResponse.Data);
            await SecureStorage.Default.SetAsync(AppConstants.AuthStorageKeyName, serializedData);
        }
        else
        {
            return authResponse.Errors.FirstOrDefault();
        }
        return null;
    }

    public void Logout() => SecureStorage.Default.Remove(AppConstants.AuthStorageKeyName);

    public async Task<HttpClient> GetAuthenticatedHttpClientAsync()
    {
        var httpClient = _httpClientFactory.CreateClient(AppConstants.HttpClientName);

        var authenticatedUser = await GetAuthenticatedUserAsync();

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authenticatedUser.Token);

        return httpClient;
    }
    private bool IsTokenExpired(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var expiration = jwtToken.ValidTo;

        return expiration < DateTime.UtcNow;
    }
    public async Task RegisterAsync(UserDto dto)
    {
        var httpClient = await GetAuthenticatedHttpClientAsync();

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(dto),
            Encoding.UTF8,
            "application/json"
        );
        var response = await httpClient.PostAsync("api/account/register", jsonContent);
        var toast = Toast.Make("Użytkownik został zarejestrowany.", ToastDuration.Short, 5);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            toast = Toast.Make("Błąd: " + errorContent, ToastDuration.Short, 5);
        }
        await toast.Show();
    }
    public async Task<IEnumerable<string>> GetRoles()
    {
        var httpClient = await GetAuthenticatedHttpClientAsync();
        var response = await httpClient.GetAsync("api/account/all/roles");

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<string>>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return apiResponse.Data;
    }
    public async Task GeneratePassword(string email)
    {
        var httpClient = await GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync($"api/account/generate-password/{email}");
        var snackbar = Snackbar.Make("Nowe hasło zostało wygenerowane.", duration: TimeSpan.FromSeconds(3));
        

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            snackbar = Snackbar.Make("Błąd: " + errorContent, duration: TimeSpan.FromSeconds(3));
        }
        await snackbar.Show();
    }
    public async Task<IEnumerable<UserDto>> GetAll()
    {
        var httpClient = await GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync("api/account/all");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<UserDto>>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return apiResponse.Data;
        }

        return null;
    }
}


using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PotoDocs.Services;

public interface IAuthService
{
    Task<bool> IsUserAuthenticated();
    Task<string?> LoginAsync(LoginRequestDto dto);
    Task<LoginResponseDto?> GetAuthenticatedUserAsync();
    Task<HttpClient> GetAuthenticatedHttpClientAsync();
    void Logout();
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

    public async Task<string?> LoginAsync(LoginRequestDto dto)
    {
        var httpClient = _httpClientFactory.CreateClient(AppConstants.HttpClientName);
        var response = await httpClient.PostAsJsonAsync< LoginRequestDto> ("api/account/login", dto);

        var content = await response.Content.ReadAsStringAsync();
        ApiResponse<LoginResponseDto> authResponse =
            JsonSerializer.Deserialize<ApiResponse<LoginResponseDto>>(content, new JsonSerializerOptions
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
}


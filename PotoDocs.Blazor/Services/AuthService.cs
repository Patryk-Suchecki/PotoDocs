using System.Net.Http.Headers;
using System.Net.Http.Json;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Services;

public interface IAuthService
{
    Task<string?> LoginAsync(LoginDto dto);
    Task Logout();
    Task<HttpClient> GetAuthenticatedHttpClientAsync();
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly JwtAuthenticationStateProvider _authProvider;

    public AuthService(HttpClient httpClient, JwtAuthenticationStateProvider authProvider)
    {
        _httpClient = httpClient;
        _authProvider = authProvider;
    }

    public async Task<string?> LoginAsync(LoginDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/account/login", dto);
        var authResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();

        if (authResponse != null && authResponse.Status)
        {
            var token = authResponse.Data.Token;
            await _authProvider.Login(token);
            return null;
        }

        return authResponse?.Errors?.FirstOrDefault();
    }

    public async Task Logout()
    {
        await _authProvider.Logout();
    }

    public async Task<HttpClient> GetAuthenticatedHttpClientAsync()
    {
        var authState = await _authProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated == true)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await _authProvider.GetToken());
        }
        return _httpClient;
    }
}

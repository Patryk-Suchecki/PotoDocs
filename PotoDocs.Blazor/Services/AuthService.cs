using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
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

        if (response.IsSuccessStatusCode)
        {
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (loginResponse is not null)
            {
                await _authProvider.Login(loginResponse.Token);
                return null;
            }

            return "Nie udało się odczytać odpowiedzi z serwera.";
        }

        var errorContent = await response.Content.ReadAsStringAsync();

        try
        {
            var problem = JsonSerializer.Deserialize<ProblemDetails>(errorContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return problem?.Title ?? $"Błąd logowania: {response.StatusCode}";
        }
        catch
        {
            return $"Błąd logowania: {response.StatusCode} - {errorContent}";
        }
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
            var token = await _authProvider.GetToken();

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return _httpClient;
    }
}

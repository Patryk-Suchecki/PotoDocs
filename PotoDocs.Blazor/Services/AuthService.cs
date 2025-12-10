using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Services;

public record LoginResult(bool IsSuccess, string? ErrorMessage);
public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginDto dto);
    Task Logout();
    Task<HttpClient> GetAuthenticatedHttpClientAsync();
}

public class AuthService(HttpClient httpClient, JwtAuthenticationStateProvider authProvider) : IAuthService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly JwtAuthenticationStateProvider _authProvider = authProvider;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<LoginResult> LoginAsync(LoginDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/login", dto);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>(_json);

                if (loginResponse is not null)
                {
                    await _authProvider.Login(loginResponse.Token);
                    return new LoginResult(true, null);
                }
            }

            var raw = await response.Content.ReadAsStringAsync();

            try
            {
                var pd = JsonSerializer.Deserialize<ProblemDetailsDto>(raw, _json);
                var msg = pd?.Detail ?? pd?.Title ?? "Błąd logowania.";
                return new LoginResult(false, msg);
            }
            catch (JsonException)
            {
                return new LoginResult(false, $"Błąd logowania: {response.ReasonPhrase} ({(int)response.StatusCode})");
            }
        }
        catch (Exception)
        {
            return new LoginResult(false, "Nie udało się połączyć z serwerem.");
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

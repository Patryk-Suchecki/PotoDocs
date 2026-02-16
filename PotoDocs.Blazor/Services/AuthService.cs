using System.Net.Http.Json;
using System.Text.Json;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Services;

public record LoginResult(bool IsSuccess, string? ErrorMessage);
public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginDto dto);
    Task Logout();
}

public class AuthService(IHttpClientFactory httpClientFactory, JwtAuthenticationStateProvider authProvider) : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly JwtAuthenticationStateProvider _authProvider = authProvider;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<LoginResult> LoginAsync(LoginDto dto)
    {
        try
        {
            // Pobieramy klienta PUBLICZNEGO (bez interceptora), 
            // żeby błąd logowania (np. złe hasło -> 401) nie wywołał wylogowania
            var client = _httpClientFactory.CreateClient("PotoDocs.Public");

            var response = await client.PostAsJsonAsync("api/account/login", dto);

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
                return new LoginResult(false, $"Błąd logowania: {response.ReasonPhrase}");
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
}
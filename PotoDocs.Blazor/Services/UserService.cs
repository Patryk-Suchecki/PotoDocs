using System.Net.Http.Json;
using System.Text.Json;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Services;

public interface IUserService
{
    Task RegisterAsync(UserDto dto);
    Task Update(UserDto dto);
    Task<IEnumerable<UserDto>> GetAll();
    Task GeneratePassword(string email);
    Task<UserDto> GetCurrentUser();
    Task<string> ChangePassword(ChangePasswordDto dto);
    Task Delete(string email);
}

public class UserService(IAuthService authService) : BaseService(authService), IUserService
{
    private readonly IAuthService _authService = authService;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task RegisterAsync(UserDto dto)
    {
        await PostAsync("api/user/register", dto);
    }
    public async Task Update(UserDto dto)
    {
        await PutAsync("api/user", dto);
    }

    public async Task GeneratePassword(string email)
    {
        await PostAsync("api/user/generate-password", email);
    }

    public async Task<IEnumerable<UserDto>> GetAll()
    {
        return await GetAsync<IEnumerable<UserDto>>("api/user/all");
    }

    public async Task<UserDto> GetCurrentUser()
    {
        return await GetAsync<UserDto>("api/user/me");
    }

    public async Task<string> ChangePassword(ChangePasswordDto dto)
    {
        var http = await _authService.GetAuthenticatedHttpClientAsync();
        var resp = await http.PostAsJsonAsync("api/user/change-password", dto);

        var raw = await resp.Content.ReadAsStringAsync();
        try
        {
            var pd = JsonSerializer.Deserialize<ProblemDetailsDto>(raw, _json);

            var msg = pd?.Detail ?? pd?.Title ?? $"Błąd zmiany hasła: {(int)resp.StatusCode}";

            return msg;
        }
        catch (JsonException)
        {
            return $"Błąd zmiany hasła: {(int)resp.StatusCode} - {raw}";
        }
    }

    public async Task Delete(string email)
    {
        await DeleteAsync($"api/user/{email}");
    }
}
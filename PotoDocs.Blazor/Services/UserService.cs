using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.Shared.Models;
using PotoDocs.Blazor.Helpers;

namespace PotoDocs.Blazor.Services;

public interface IUserService
{
    Task RegisterAsync(UserDto dto);
    Task<IEnumerable<UserDto>> GetAll();
    Task GeneratePassword(string email);
    Task<UserDto?> GetCurrentUser();
    Task<string?> ChangePassword(ChangePasswordDto dto);
    Task Delete(string email);
}

public class UserService : IUserService
{
    private readonly IAuthService _authService;

    public UserService(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task RegisterAsync(UserDto dto)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.PostAsJsonAsync("api/user/register", dto);
        await response.ThrowIfNotSuccessWithProblemDetails();
    }

    public async Task GeneratePassword(string email)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.PostAsJsonAsync("api/user/generate-password", email);
        await response.ThrowIfNotSuccessWithProblemDetails();
    }

    public async Task<IEnumerable<UserDto>> GetAll()
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.GetAsync("api/user/all");

        await response.ThrowIfNotSuccessWithProblemDetails();

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
        return result ?? new List<UserDto>();
    }

    public async Task<UserDto?> GetCurrentUser()
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.GetAsync("api/user/me");

        await response.ThrowIfNotSuccessWithProblemDetails();

        return await response.Content.ReadFromJsonAsync<UserDto>();
    }

    public async Task<string?> ChangePassword(ChangePasswordDto dto)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.PostAsJsonAsync("api/user/change-password", dto);

        if (response.IsSuccessStatusCode)
            return null;

        var errorText = await response.Content.ReadAsStringAsync();

        try
        {
            var problem = JsonSerializer.Deserialize<ProblemDetails>(errorText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return problem?.Title ?? "Wystąpił nieznany błąd.";
        }
        catch
        {
            return $"Błąd zmiany hasła: {response.StatusCode}";
        }
    }

    public async Task Delete(string email)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.DeleteAsync($"api/user/{email}");
        await response.ThrowIfNotSuccessWithProblemDetails();
    }
}

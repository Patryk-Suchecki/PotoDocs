using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Services;

public interface IUserService
{
    Task RegisterAsync(UserDto dto);
    Task<IEnumerable<UserDto>> GetAll();
    Task GeneratePassword(string email);
    Task<UserDto> GetUser();
    Task<string?> ChangePassword(ChangePasswordDto dto);
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

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(dto),
            Encoding.UTF8,
            "application/json"
        );
        var response = await httpClient.PostAsync("api/user/register", jsonContent);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
        }
    }
    public async Task GeneratePassword(string email)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync($"api/user/generate-password/{email}");


        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
        }
    }
    public async Task<IEnumerable<UserDto>> GetAll()
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync("api/user/all");
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
    public async Task<UserDto> GetUser()
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync("api/user");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return apiResponse.Data;
        }

        return null;
    }
    public async Task<string?> ChangePassword(ChangePasswordDto dto)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.PostAsJsonAsync("api/user/change-password", dto);


        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(errorContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return apiResponse?.Errors?.FirstOrDefault() ?? "Wystąpił nieznany błąd.";
        }
        return null;
    }
}
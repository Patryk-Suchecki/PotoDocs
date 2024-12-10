using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.Text;

namespace PotoDocs.Services;

public interface IUserService
{
    Task RegisterAsync(UserDto dto);
    Task<IEnumerable<UserDto>> GetAll();
    Task GeneratePassword(string email);
    Task<UserDto> GetUser();
    Task ChangePassword(ChangePasswordDto dto);
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
        var toast = Toast.Make("Użytkownik został zarejestrowany.", ToastDuration.Short, 5);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            toast = Toast.Make("Błąd: " + errorContent, ToastDuration.Short, 5);
        }
        await toast.Show();
    }
    public async Task GeneratePassword(string email)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

        var response = await httpClient.GetAsync($"api/user/generate-password/{email}");
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
    public async Task ChangePassword(ChangePasswordDto dto)
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(dto),
            Encoding.UTF8,
            "application/json"
        );
        var response = await httpClient.PostAsync("api/user/change-password", jsonContent);
        var snackbar = Snackbar.Make("Hasło zostało zmienione wylogowywanie...", duration: TimeSpan.FromSeconds(3));


        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            snackbar = Snackbar.Make("Błąd: " + errorContent, duration: TimeSpan.FromSeconds(3));
        }
        await snackbar.Show();
    }
}
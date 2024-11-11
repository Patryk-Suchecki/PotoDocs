using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PotoDocs.Services;

public class UserService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthService _authService;
    List<UserDto> _userList;

    public UserService(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
    }
    public async Task<string?> RegisterAsync(UserDto dto)
    {
        //var result = await _httpClient.PostAsJsonAsync(AppConstants.ApiUrl + "/api/account/register", dto);
        //if (result.IsSuccessStatusCode)
        //{
        //    var response = await result.Content.ReadFromJsonAsync<LoginResponseDto>();
        //}
        //else
        //{
        //    return "Error in logging in";
        //}

        return null;
    }
    public async Task<string?> ResetPassword(int userId)
    {
        //var result = await _httpClient.PostAsJsonAsync(AppConstants.ApiUrl + "/api/account/resetpassword", userId);
        //if (result.IsSuccessStatusCode)
        //{

        //}
        //else
        //{
        //    return "Error in logging in";
        //}

        return null;
    }
    public async Task<IEnumerable<UserDto>> GetAll()
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();

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
        else
        {
            var statusCode = response.StatusCode;
        }
        return null;
    }
}

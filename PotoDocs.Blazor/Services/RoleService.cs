using System.Text.Json;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Services;

public interface IRoleService
{
    Task<IEnumerable<string>> GetRoles();
}

public class RoleService : IRoleService
{
    private readonly IAuthService _authService;

    public RoleService(IAuthService authService)
    {
        _authService = authService;
    }
    public async Task<IEnumerable<string>> GetRoles()
    {
        var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await httpClient.GetAsync("api/role/all");

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<string>>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return apiResponse.Data;
    }
}
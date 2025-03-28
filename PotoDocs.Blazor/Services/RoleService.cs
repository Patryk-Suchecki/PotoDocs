using System.Net.Http.Json;
using PotoDocs.Blazor.Helpers;

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
        await response.ThrowIfNotSuccessWithProblemDetails(); // 💥

        var roles = await response.Content.ReadFromJsonAsync<IEnumerable<string>>();
        return roles ?? new List<string>();
    }
}

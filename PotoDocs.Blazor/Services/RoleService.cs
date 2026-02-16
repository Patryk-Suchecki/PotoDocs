
namespace PotoDocs.Blazor.Services;

public interface IRoleService
{
    Task<IEnumerable<string>> GetRoles();
}

public class RoleService(HttpClient http) : BaseService(http), IRoleService
{
    public async Task<IEnumerable<string>> GetRoles()
    {
        return await GetAsync<IEnumerable<string>>("api/role/all");
    }
}
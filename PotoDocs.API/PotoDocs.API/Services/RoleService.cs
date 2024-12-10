using PotoDocs.API.Models;
using PotoDocs.Shared.Models;


namespace PotoDocs.API.Services;

public interface IRoleService
{
    ApiResponse<List<string>> GetRoles();
}

public class RoleService : IRoleService
{
    private readonly PotodocsDbContext _context;

    public RoleService(PotodocsDbContext context)
    {
        _context = context;
    }

    public ApiResponse<List<string>> GetRoles()
    {
        var roleNames = _context.Roles.Select(role => role.Name).ToList();
        return ApiResponse<List<string>>.Success(roleNames);
    }
}

using PotoDocs.API.Entities;
using PotoDocs.API.Exceptions;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;


namespace PotoDocs.API.Services;

public interface IRoleService
{
    List<string> GetRoles();
}


public class RoleService : IRoleService
{
    private readonly PotodocsDbContext _context;

    public RoleService(PotodocsDbContext context)
    {
        _context = context;
    }

    public List<string> GetRoles()
    {
        var roleNames = _context.Roles
            .Select(role => role.Name)
            .ToList();

        if (roleNames == null || roleNames.Count == 0)
            throw new BadRequestException("Brak zdefiniowanych ról w systemie.");

        return roleNames;
    }
}


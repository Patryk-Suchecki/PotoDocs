using Microsoft.EntityFrameworkCore;
using PotoDocs.API.Entities;
using PotoDocs.API.Exceptions;

namespace PotoDocs.API.Services;

public interface IRoleService
{
    Task<List<string>> GetRolesAsync();
}

public class RoleService(PotodocsDbContext context) : IRoleService
{
    private readonly PotodocsDbContext _context = context;

    public async Task<List<string>> GetRolesAsync()
    {
        var roleNames = await _context.Roles
            .AsNoTracking()
            .Select(role => role.Name)
            .ToListAsync();

        if (roleNames.Count == 0)
        {
            throw new BadRequestException("Brak zdefiniowanych ról w systemie.");
        }

        return roleNames;
    }
}
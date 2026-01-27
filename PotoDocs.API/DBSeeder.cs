using Microsoft.AspNetCore.Identity;
using PotoDocs.API.Entities;
using PotoDocs.API.Models;

namespace PotoDocs.API
{
    public class DBSeeder(PotodocsDbContext dbContext, IPasswordHasher<User> hasher)
    {
        private readonly PotodocsDbContext _dbContext = dbContext;
        private readonly IPasswordHasher<User> _hasher = hasher;

        public void Seed()
        {
            if (_dbContext.Database.CanConnect())
            {
                if (!_dbContext.Roles.Any())
                {
                    var roles = GetRoles();
                    _dbContext.Roles.AddRange(roles);
                    _dbContext.SaveChanges();
                }

                if (!_dbContext.Users.Any(u => u.Email == "a@a.a"))
                {
                    var adminRole = _dbContext.Roles.FirstOrDefault(r => r.Name == "admin");

                    if (adminRole != null)
                    {
                        var adminUser = new User
                        {
                            FirstName = "Admin",
                            LastName = "User",
                            Email = "a@a.a",
                            RoleId = adminRole.Id
                        };
                        adminUser.PasswordHash = _hasher.HashPassword(adminUser, "a");

                        _dbContext.Users.Add(adminUser);
                        _dbContext.SaveChanges();
                    }
                }
            }
        }

        private IEnumerable<Role> GetRoles()
        {
            var roles = new List<Role>()
            {
                new()
                {
                    Name = "user",
                },
                new()
                {
                    Name = "manager"
                },
                new()
                {
                    Name = "admin"
                }
            };
            return roles;
        }
    }
}

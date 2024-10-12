using Microsoft.EntityFrameworkCore;

namespace PotoDocs.API.Models;

public class PotodocsDbContext : DbContext
{

    public DbSet<CMRFile> CMRFiles { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<User> Users { get; set; }

    public PotodocsDbContext(DbContextOptions<PotodocsDbContext> options) : base(options)
    {
    }
}

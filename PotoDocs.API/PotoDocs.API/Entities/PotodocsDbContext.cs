using Microsoft.EntityFrameworkCore;
using PotoDocs.API.Entities;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Models;

public class PotodocsDbContext : DbContext
{

    public DbSet<CMRFile> CMRFiles { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }

    public PotodocsDbContext(DbContextOptions<PotodocsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}

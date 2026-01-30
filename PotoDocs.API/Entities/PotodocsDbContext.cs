using Microsoft.EntityFrameworkCore;

namespace PotoDocs.API.Entities;

public class PotodocsDbContext(DbContextOptions<PotodocsDbContext> options) : DbContext(options)
{

    public DbSet<OrderFile> OrderFiles { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceSequence> InvoiceSequences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasMany(o => o.Stops)
                .WithOne(s => s.Order)
                .HasForeignKey(s => s.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(o => o.Company)
                .WithMany()
                .HasForeignKey(o => o.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(o => o.Files)
                .WithOne(f => f.Order)
                .HasForeignKey(f => f.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Invoice>()
                    .HasMany(i => i.Items)
                    .WithOne(it => it.Invoice)
                    .HasForeignKey(it => it.InvoiceId);

        modelBuilder.Entity<InvoiceSequence>()
        .HasKey(s => new { s.Year, s.Month, s.Type });

        var decimalProperties = modelBuilder.Model
        .GetEntityTypes()
        .SelectMany(t => t.GetProperties())
        .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

        foreach (var property in decimalProperties)
        {
            property.SetColumnType("decimal(18,2)");
        }

        base.OnModelCreating(modelBuilder);
    }
}
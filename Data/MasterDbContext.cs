using BankOs.Models;
using Microsoft.EntityFrameworkCore;

namespace BankOs.Data;

public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }

    public DbSet<TenantRegistry> Tenants => Set<TenantRegistry>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<TenantRegistry>()
            .HasIndex(t => t.Slug)
            .IsUnique();

        builder.Entity<AdminUser>()
            .HasIndex(a => a.Email)
            .IsUnique();
    }
}
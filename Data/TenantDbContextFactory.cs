using BankOs.Interfaces;
using BankOs.Models;
using Microsoft.EntityFrameworkCore;

namespace BankOs.Data;

public class TenantDbContextFactory
{
    private readonly ITenantResolver _resolver;
    private readonly MasterDbContext _master;

    public TenantDbContextFactory(ITenantResolver resolver, MasterDbContext master)
    {
        _resolver = resolver;
        _master = master;
    }

    // Usado por todos los servicios de tenant (resuelve por slug en la URL)
    public async Task<TenantDbContext> CreateAsync()
    {
        var tenant = await _resolver.ResolveAsync()
                     ?? throw new InvalidOperationException("Tenant no encontrado para este subdominio");

        return BuildContext(tenant.ConnectionString);
    }

    // Usado por servicios del admin global (resuelve por admin_id del JWT)
    public async Task<TenantDbContext> CreateForAdminAsync(Guid adminId)
    {
        var admin = await _master.AdminUsers.FindAsync(adminId)
                    ?? throw new InvalidOperationException("Admin no encontrado");

        if (!admin.TenantId.HasValue)
            throw new InvalidOperationException("Este admin aún no tiene un tenant creado");

        var tenant = await _master.Tenants.FindAsync(admin.TenantId.Value)
                     ?? throw new InvalidOperationException("Tenant no encontrado");

        return BuildContext(tenant.ConnectionString);
    }

    private static TenantDbContext BuildContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new TenantDbContext(options);
    }
}
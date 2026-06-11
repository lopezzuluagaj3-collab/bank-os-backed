using BankOs.Data;
using BankOs.Interfaces;
using BankOs.Models;
using Microsoft.EntityFrameworkCore;

namespace BankOs.Services;

public class TenantResolver : ITenantResolver
{
    private readonly MasterDbContext _master;
    private readonly IHttpContextAccessor _http;

    public TenantResolver(MasterDbContext master, IHttpContextAccessor http)
    {
        _master = master;
        _http = http;
    }

    public async Task<TenantRegistry?> ResolveAsync()
    {
        var context = _http.HttpContext;
        if (context == null) return null;

        // Lee el {tenantSlug} que ASP.NET pone en RouteValues
        // gracias a la ruta base "{tenantSlug}/api/v1/..."
        if (!context.Request.RouteValues.TryGetValue("tenantSlug", out var slugValue))
            return null;

        var slug = slugValue?.ToString();
        if (string.IsNullOrEmpty(slug)) return null;

        return await _master.Tenants.FirstOrDefaultAsync(t => t.Slug == slug);
    }
}
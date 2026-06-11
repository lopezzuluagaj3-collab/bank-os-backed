using BankOs.Interfaces;

namespace BankOs.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolver resolver)
    {
        var isGlobal =
            context.Request.Path.StartsWithSegments("/api/v1/tenants") ||
            context.Request.Path.StartsWithSegments("/api/v1/admin") ||  // ← nuevo
            context.Request.Path.StartsWithSegments("/swagger");

        if (!isGlobal)
        {
            var tenant = await resolver.ResolveAsync();
            if (tenant == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsJsonAsync(new
                {
                    code = "TENANT_NOT_FOUND",
                    message = "No se encontró un tenant para esta URL"
                });
                return;
            }

            context.Items["TenantRegistry"] = tenant;
        }

        await _next(context);
    }
}
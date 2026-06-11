using Microsoft.Extensions.Caching.Memory;

namespace BankOs.Middleware;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    private static readonly HashSet<string> AuthPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/v1/admin/login"
    };

    private const int AuthMaxRequests        = 5;
    private const int TransactionMaxRequests = 10;

    public RateLimitMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next  = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path   = context.Request.Path.Value ?? "";
        var method = context.Request.Method;
        var ip     = GetClientIp(context);

        bool isAuthPost =
            method.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
            (AuthPaths.Contains(path) ||
             path.EndsWith("/api/v1/auth/login",             StringComparison.OrdinalIgnoreCase) ||
             path.EndsWith("/api/v1/auth/register/cliente",  StringComparison.OrdinalIgnoreCase));

        bool isTxPost =
            method.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
            path.EndsWith("/api/v1/transactions", StringComparison.OrdinalIgnoreCase);

        // Auth → por IP
        if (isAuthPost)
        {
            if (IsRateLimited($"rl:auth:ip:{ip}", AuthMaxRequests, TimeSpan.FromMinutes(1), out int rem))
            {
                await WriteRateLimitResponse(context, "AUTH_RATE_LIMIT",
                    "Demasiados intentos de autenticación. Espera 1 minuto.", rem);
                return;
            }
        }

        // Transacciones → por IP y por usuario
        if (isTxPost)
        {
            if (IsRateLimited($"rl:tx:ip:{ip}", TransactionMaxRequests, TimeSpan.FromMinutes(1), out int rem))
            {
                await WriteRateLimitResponse(context, "TRANSACTION_RATE_LIMIT",
                    "Demasiadas transacciones desde tu IP. Espera 1 minuto.", rem);
                return;
            }

            var userId = context.User.FindFirst("user_id")?.Value;
            if (userId != null)
            {
                if (IsRateLimited($"rl:tx:user:{userId}", TransactionMaxRequests, TimeSpan.FromMinutes(1), out int rem2))
                {
                    await WriteRateLimitResponse(context, "TRANSACTION_RATE_LIMIT",
                        "Has superado tu límite personal de transferencias por minuto.", rem2);
                    return;
                }
            }
        }

        await _next(context);
    }

    private bool IsRateLimited(string cacheKey, int maxRequests, TimeSpan window, out int remaining)
    {
        var expKey = $"{cacheKey}:exp";

        var expiration = _cache.GetOrCreate(expKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = window;
            return DateTimeOffset.UtcNow.Add(window);
        });

        var count = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpiration = expiration;
            return 0;
        });

        count++;
        _cache.Set(cacheKey, count, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = expiration
        });

        remaining = Math.Max(0, maxRequests - count);
        return count > maxRequests;
    }

    private static string GetClientIp(HttpContext context)
    {
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static async Task WriteRateLimitResponse(
        HttpContext context, string code, string message, int remaining)
    {
        context.Response.StatusCode  = 429;
        context.Response.ContentType = "application/json";
        context.Response.Headers["Retry-After"]           = "60";
        context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();

        await context.Response.WriteAsJsonAsync(new
        {
            success = false,
            code    = code,
            message = message
        });
    }
}
using System.Text.Json;
using BankOs.Data;
using BankOs.Interfaces;
using BankOs.Models;
using Microsoft.EntityFrameworkCore;

namespace BankOs.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantDbContextFactory factory, ITenantResolver resolver)
    {
        // Solo interceptar POST a /{tenantSlug}/api/v1/transactions
        if (!context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase)
            || !context.Request.Path.Value!.Contains("/api/v1/transactions"))
        {
            await _next(context);
            return;
        }

        // Leer el header
        if (!context.Request.Headers.TryGetValue("X-Idempotency-Key", out var keyValue)
            || string.IsNullOrWhiteSpace(keyValue))
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                code = "MISSING_IDEMPOTENCY_KEY",
                message = "Se requiere el header X-Idempotency-Key para esta operación"
            }));
            return;
        }

        var idempotencyKey = keyValue.ToString();

        // Necesitamos el tenantId para el scope de la clave
        var tenant = await resolver.ResolveAsync();
        if (tenant == null)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                code = "TENANT_NOT_FOUND",
                message = "Tenant no resuelto"
            }));
            return;
        }

        // Obtener userId del JWT
        var userIdClaim = context.User.FindFirst("user_id")?.Value;
        var userId = userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;

        var db = await factory.CreateAsync();
        var existing = await db.IdempotencyKeys
            .FirstOrDefaultAsync(k => k.Key == idempotencyKey && k.TenantId == tenant.Id);

        if (existing != null)
        {
            if (existing.Status == "completed")
            {
                // Devolver respuesta cacheada sin ejecutar nada
                context.Response.StatusCode = existing.ResponseCode ?? 200;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(existing.ResponseBody ?? "{}");
                return;
            }

            if (existing.Status == "processing")
            {
                // Petición duplicada en vuelo
                context.Response.StatusCode = 409;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    success = false,
                    code = "DUPLICATE_REQUEST",
                    message = "Ya hay una petición en proceso con esta clave de idempotencia. Espera e intenta de nuevo."
                }));
                return;
            }
        }

        // Registrar como "processing"
        var record = new IdempotencyKey
        {
            Key = idempotencyKey,
            TenantId = tenant.Id,
            UserId = userId,
            Status = "processing",
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        db.IdempotencyKeys.Add(record);
        await db.SaveChangesAsync();

        // Interceptar el response para capturarlo
        var originalBody = context.Response.Body;
        using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        try
        {
            await _next(context);
        }
        finally
        {
            responseBuffer.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(responseBuffer).ReadToEndAsync();

            var statusCode = context.Response.StatusCode;

            if (statusCode >= 200 && statusCode < 300)
            {
                // Solo cachear respuestas exitosas
                record.Status = "completed";
                record.ResponseBody = responseBody;
                record.ResponseCode = statusCode;
                await db.SaveChangesAsync();
            }
            else
            {
                // Si fue error, eliminar el registro para permitir que el usuario reintente
                db.IdempotencyKeys.Remove(record);
                await db.SaveChangesAsync();
            }

            responseBuffer.Seek(0, SeekOrigin.Begin);
            await responseBuffer.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BankOs.Data;
using BankOs.DTOs.Auth;
using BankOs.Interfaces;
using BankOs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BankOs.Services;

public class AuthService : IAuthService
{
    private readonly MasterDbContext _master;
    private readonly IConfiguration _config;

    public AuthService(MasterDbContext master, IConfiguration config)
    {
        _master = master;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(string tenantSlug, RegisterDto dto)
    {
        // 1. Verificar que el tenant existe en la BD maestra
        var tenant = await _master.Tenants.FirstOrDefaultAsync(t => t.Slug == tenantSlug)
            ?? throw new InvalidOperationException($"No existe el tenant '{tenantSlug}'");

        // 2. Conectar a la BD de ese tenant específico
        using var db = CreateTenantContext(tenant.ConnectionString);

        // 3. Verificar que el email no esté ya registrado EN ESTE TENANT
        var exists = await db.Users.AnyAsync(u => u.Email == dto.Email);
        if (exists)
            throw new InvalidOperationException("El email ya está registrado en este tenant");

        // 4. Crear el usuario
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            IsActive = true,
            DocumentId = dto.DocumentId,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);

        // 5. Si es cliente, crear su cuenta bancaria automáticamente
        if (dto.Role == "cliente")
        {
            db.Accounts.Add(new Account
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Number = GenerateAccountNumber(),
                Balance = 0,
                Currency = "COP",
                Status = "active",
                CreatedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();

        return GenerateToken(user, tenant);
    }

    public async Task<AuthResponseDto> LoginAsync(string tenantSlug, LoginDto dto)
    {
        // 1. Verificar que el tenant existe
        var tenant = await _master.Tenants.FirstOrDefaultAsync(t => t.Slug == tenantSlug)
            ?? throw new InvalidOperationException($"No existe el tenant '{tenantSlug}'");

        // 2. Conectar a la BD de ese tenant
        using var db = CreateTenantContext(tenant.ConnectionString);

        // 3. Buscar el usuario por email
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new UnauthorizedAccessException("Credenciales inválidas");

        // 4. Verificar contraseña
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas");

        return GenerateToken(user, tenant);
    }

    // ─── Privados ────────────────────────────────────────────────────────────────

    private TenantDbContext CreateTenantContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new TenantDbContext(options);
    }

    private AuthResponseDto GenerateToken(User user, TenantRegistry tenant)
    {
        var claims = new[]
        {
            new Claim("user_id", user.Id.ToString()),
            new Claim("tenant_id", tenant.Id.ToString()),
            new Claim("tenant_slug", tenant.Slug),
            new Claim("role", user.Role),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Email = user.Email,
            Role = user.Role,
            TenantId = tenant.Id,
            TenantSlug = tenant.Slug
        };
    }

    private static string GenerateAccountNumber()
        => $"BNK{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
}
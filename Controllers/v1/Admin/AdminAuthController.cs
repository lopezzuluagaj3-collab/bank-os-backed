

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BankOs.Data;
using BankOs.DTOs.Admin;
using BankOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BankOs.Controllers.v1.Admin;

[ApiController]
[Route("api/v1/admin")]
public class AdminAuthController : ControllerBase
{
    private readonly MasterDbContext _master;
    private readonly IConfiguration _config;

    public AdminAuthController(MasterDbContext master, IConfiguration config)
    {
        _master = master;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginDto dto)
    {
        var admin = await _master.AdminUsers
            .FirstOrDefaultAsync(a => a.Email == dto.Email);

        if (admin == null || !BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash))
            return Unauthorized(ApiResponse<AdminAuthResponseDto>.Fail(
                "INVALID_CREDENTIALS", "Credenciales inválidas"));

        var claims = new[]
        {
            new Claim("admin_id", admin.Id.ToString()),
            new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "admin_global"),
            new Claim(ClaimTypes.Email, admin.Email)
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

        var response = new AdminAuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Email = admin.Email,
            AdminId = admin.Id,
            HasTenant = admin.TenantId.HasValue,
            TenantId = admin.TenantId
        };

        return Ok(ApiResponse<AdminAuthResponseDto>.Ok(response, "Login exitoso"));
    }
}
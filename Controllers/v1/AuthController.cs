using BankOs.DTOs.Auth;
using BankOs.Interfaces;
using BankOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BankOs.Controllers.v1;

[ApiController]
[Route("{tenantSlug}/api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register/cliente")]
    public async Task<IActionResult> RegisterCliente(
        [FromRoute] string tenantSlug,
        [FromBody] RegisterDto dto)
    {
        try
        {
            dto.Role = "cliente";
            var result = await _authService.RegisterAsync(tenantSlug, dto);
            return StatusCode(201, ApiResponse<AuthResponseDto>.Ok(result, "Cliente registrado exitosamente"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<AuthResponseDto>.Fail("REGISTER_ERROR", ex.Message));
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromRoute] string tenantSlug,
        [FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(tenantSlug, dto);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login exitoso"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("INVALID_CREDENTIALS", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<AuthResponseDto>.Fail("TENANT_NOT_FOUND", ex.Message));
        }
    }
}
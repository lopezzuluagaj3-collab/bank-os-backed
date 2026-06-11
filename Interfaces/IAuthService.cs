using BankOs.DTOs.Auth;

namespace BankOs.Interfaces;

public interface IAuthService
{
    // El slug viene de la URL, no del body
    Task<AuthResponseDto> RegisterAsync(string tenantSlug, RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(string tenantSlug, LoginDto dto);
}
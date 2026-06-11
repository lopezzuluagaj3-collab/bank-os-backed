namespace BankOs.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public Guid TenantId { get; set; }
    public string TenantSlug { get; set; } = null!; // útil para que el front construya las URLs
}
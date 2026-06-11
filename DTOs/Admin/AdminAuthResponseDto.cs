namespace BankOs.DTOs.Admin;

public class AdminAuthResponseDto
{
    public string Token { get; set; } = null!;
    public string Email { get; set; } = null!;
    public Guid AdminId { get; set; }
    public bool HasTenant { get; set; }
    public Guid? TenantId { get; set; }
}
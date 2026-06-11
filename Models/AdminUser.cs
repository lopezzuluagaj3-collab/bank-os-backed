namespace BankOs.Models;

public class AdminUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public Guid? TenantId { get; set; }  // null = aún no ha creado su tenant
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

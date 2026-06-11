namespace BankOs.Models;

public class IdempotencyKey
{
    public string Key { get; set; } = null!;
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = null!; // "processing", "completed"
    public string? ResponseBody { get; set; }
    public int? ResponseCode { get; set; }
    public DateTime ExpiresAt { get; set; }
}
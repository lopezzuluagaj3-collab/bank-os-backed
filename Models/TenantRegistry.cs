namespace BankOs.Models;

public class TenantRegistry
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string ConnectionString { get; set; } = null!;
    public string? WebhookUrl { get; set; }
    public string Config { get; set; } = null!; // JSONB
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
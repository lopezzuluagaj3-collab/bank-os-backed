namespace BankOs.Models;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid? FromAccountId { get; set; }
    public Guid? ToAccountId { get; set; }
    public string Type { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal? ConvertedAmount { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal Commission { get; set; }
    public string Status { get; set; } = null!;
    public string IdempotencyKey { get; set; } = null!;
    public string CorrelationId { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Account? FromAccount { get; set; }
    public Account? ToAccount { get; set; }
}
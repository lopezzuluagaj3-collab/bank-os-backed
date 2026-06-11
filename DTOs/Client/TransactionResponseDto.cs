namespace BankOs.DTOs.Transaction;

public class TransactionResponseDto
{
    public Guid Id { get; set; }

    public Guid? FromAccountId { get; set; }
    public string? FromAccountNumber { get; set; }

    public Guid? ToAccountId { get; set; }
    public string? ToAccountNumber { get; set; }

    public string Type { get; set; } = null!;       // "transfer"
    public decimal Amount { get; set; }             // monto enviado
    public decimal Commission { get; set; }         // fee cobrado
    public decimal TotalDebited { get; set; }       // amount + commission

    public string Status { get; set; } = null!;     // "completed"
    public string IdempotencyKey { get; set; } = null!;
    public string CorrelationId { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
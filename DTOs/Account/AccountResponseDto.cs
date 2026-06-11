namespace BankOs.DTOs.Account;

public class AccountResponseDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = null!;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = null!;
    public string Status { get; set; } = null!;
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
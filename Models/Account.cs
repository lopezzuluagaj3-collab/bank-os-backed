namespace BankOs.Models;

public class Account
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Number { get; set; } = null!;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = null!;
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
using BankOs.DTOs.Account;

namespace BankOs.DTOs.Admin;

public class UserDetailDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<AccountResponseDto> Accounts { get; set; } = [];
}
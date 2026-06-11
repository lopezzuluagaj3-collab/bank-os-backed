using System.ComponentModel.DataAnnotations;

namespace BankOs.DTOs.Admin;

public class AdminLoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
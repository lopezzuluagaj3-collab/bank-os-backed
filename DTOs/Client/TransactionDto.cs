using System.ComponentModel.DataAnnotations;

namespace BankOs.DTOs.Transaction;

public class CreateTransactionDto
{
    // Número de cuenta destino (el cliente escribe el número, no el ID)
    [Required]
    public string ToAccountNumber { get; set; } = null!;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Amount { get; set; }
}
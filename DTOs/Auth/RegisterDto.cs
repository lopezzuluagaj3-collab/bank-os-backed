using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BankOs.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = null!;
    
    [Required(ErrorMessage = "El número de documento es obligatorio")]
    public string DocumentId { get; set; } = null!;

    // El rol lo asigna el endpoint, no el cliente
    [JsonIgnore]
    public string Role { get; set; } = "Client";
}
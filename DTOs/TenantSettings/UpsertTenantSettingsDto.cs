using System.ComponentModel.DataAnnotations;

namespace BankOs.DTOs.TenantSettings;

public class UpsertTenantSettingsDto
{
    // ── Financiero ────────────────────────────────────────────────
    [Range(0, double.MaxValue)]
    public decimal TransferFee { get; set; }

    [Range(0, 100)]
    public decimal ExchangeFeePercent { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinTransactionAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MaxTransactionAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DailyLimit { get; set; }

    [Required]
    public string MainCurrency { get; set; } = "COP";

    [Required]
    [RegularExpression("^(fixed|percentage)$", ErrorMessage = "CommissionType debe ser 'fixed' o 'percentage'")]
    public string CommissionType { get; set; } = "fixed";

    [Range(0, double.MaxValue)]
    public decimal CommissionValue { get; set; }

    // {"USD_COP": 4200, "EUR_COP": 4500}
    public Dictionary<string, decimal> ExchangeRates { get; set; } = [];

    // ── Branding ─────────────────────────────────────────────────
    [Required]
    public string BankName { get; set; } = null!;

    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Debe ser hex válido (#RRGGBB)")]
    public string PrimaryColor { get; set; } = "#1A73E8";

    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Debe ser hex válido (#RRGGBB)")]
    public string SecondaryColor { get; set; } = "#FBBC04";

    public string? LogoUrl { get; set; }
}
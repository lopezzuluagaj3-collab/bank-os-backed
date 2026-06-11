namespace BankOs.Models;

public class TenantSettings
{
    public Guid Id { get; set; }

    // ── Financiero ────────────────────────────────────────────────
    public decimal TransferFee { get; set; }
    public decimal ExchangeFeePercent { get; set; }
    public decimal MinTransactionAmount { get; set; }
    public decimal MaxTransactionAmount { get; set; }
    public decimal DailyLimit { get; set; }
    public string MainCurrency { get; set; } = "COP";

    // "fixed" → cobra TransferFee como monto fijo
    // "percentage" → cobra CommissionValue% sobre el monto
    public string CommissionType { get; set; } = "fixed";
    public decimal CommissionValue { get; set; }

    // Tasas de cambio estáticas — serializado como JSON
    // Ejemplo: {"USD_COP":"4200","EUR_COP":"4500"}
    public string ExchangeRates { get; set; } = "{}";

    // ── Branding (para la app Flutter) ───────────────────────────
    public string BankName { get; set; } = null!;
    public string PrimaryColor { get; set; } = "#1A73E8";
    public string SecondaryColor { get; set; } = "#FBBC04";
    public string? LogoUrl { get; set; }

    // ── Auditoría ────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
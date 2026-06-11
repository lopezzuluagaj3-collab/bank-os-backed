namespace BankOs.DTOs.TenantSettings;

public class TenantSettingsResponseDto
{
    public Guid Id { get; set; }

    // Financiero
    public decimal TransferFee { get; set; }
    public decimal ExchangeFeePercent { get; set; }
    public decimal MinTransactionAmount { get; set; }
    public decimal MaxTransactionAmount { get; set; }
    public decimal DailyLimit { get; set; }
    public string MainCurrency { get; set; } = null!;
    public string CommissionType { get; set; } = null!;
    public decimal CommissionValue { get; set; }
    public Dictionary<string, decimal> ExchangeRates { get; set; } = [];

    // Branding
    public string BankName { get; set; } = null!;
    public string PrimaryColor { get; set; } = null!;
    public string SecondaryColor { get; set; } = null!;
    public string? LogoUrl { get; set; }

    public DateTime UpdatedAt { get; set; }
}
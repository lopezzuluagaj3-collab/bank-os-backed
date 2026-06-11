namespace BankOs.DTOs.Tenant;

public class CreateTenantDto
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? WebhookUrl { get; set; }
    public CreateTenantConfigDto Config { get; set; } = null!;
}

public class CreateTenantConfigDto
{
    public string MainCurrency { get; set; } = "COP";
    public string CommissionType { get; set; } = "fixed";
    public decimal CommissionValue { get; set; }
    public Dictionary<string, decimal> ExchangeRates { get; set; } = [];

    // Campos que van a TenantSettings en la BD del tenant
    public decimal TransferFee { get; set; } = 0;
    public decimal ExchangeFeePercent { get; set; } = 0;
    public decimal MinTransactionAmount { get; set; } = 0;
    public decimal MaxTransactionAmount { get; set; } = 10_000_000;
    public decimal DailyLimit { get; set; } = 50_000_000;
}
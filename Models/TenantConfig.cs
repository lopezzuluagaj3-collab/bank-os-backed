namespace BankOs.Models;

public class TenantConfig
{
    public string MainCurrency { get; set; } = "COP";
    public decimal MaxTransactionAmount { get; set; }
    public string CommissionType { get; set; } = "fixed"; // "fixed" o "percentage"
    public decimal CommissionValue { get; set; }
    public Dictionary<string, decimal> ExchangeRates { get; set; } = []; // ej: "USD_COP": 4200
}
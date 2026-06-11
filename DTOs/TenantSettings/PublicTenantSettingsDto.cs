namespace BankOs.DTOs.TenantSettings;

public class PublicTenantSettingsDto
{
    public string BankName { get; set; } = null!;
    public string PrimaryColor { get; set; } = null!;
    public string SecondaryColor { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string MainCurrency { get; set; } = null!;
}
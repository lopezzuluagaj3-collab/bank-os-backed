using System.Text.Json;
using BankOs.Data;
using BankOs.DTOs.TenantSettings;
using BankOs.Interfaces;
using BankOs.Models;
using Microsoft.EntityFrameworkCore;

namespace BankOs.Services;

public class TenantSettingsService : ITenantSettingsService
{
    private readonly TenantDbContextFactory _factory;

    public TenantSettingsService(TenantDbContextFactory factory)
    {
        _factory = factory;
    }

    public async Task<TenantSettingsResponseDto> UpsertAsync(UpsertTenantSettingsDto dto, Guid adminUserId)
    {
        var db = await _factory.CreateAsync();
        var settings = await db.TenantSettings.FirstOrDefaultAsync();

        string? oldValue = null;

        if (settings == null)
        {
            settings = new TenantSettings { Id = Guid.NewGuid() };
            db.TenantSettings.Add(settings);
        }
        else
        {
            oldValue = $"BankName:{settings.BankName}, TransferFee:{settings.TransferFee}, MainCurrency:{settings.MainCurrency}";
        }

        // Financiero
        settings.TransferFee = dto.TransferFee;
        settings.ExchangeFeePercent = dto.ExchangeFeePercent;
        settings.MinTransactionAmount = dto.MinTransactionAmount;
        settings.MaxTransactionAmount = dto.MaxTransactionAmount;
        settings.DailyLimit = dto.DailyLimit;
        settings.MainCurrency = dto.MainCurrency;
        settings.CommissionType = dto.CommissionType;
        settings.CommissionValue = dto.CommissionValue;
        settings.ExchangeRates = JsonSerializer.Serialize(dto.ExchangeRates);

        // Branding
        settings.BankName = dto.BankName;
        settings.PrimaryColor = dto.PrimaryColor;
        settings.SecondaryColor = dto.SecondaryColor;
        settings.LogoUrl = dto.LogoUrl;

        settings.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        //await db.AppendAuditLogAsync(
        //    adminUserId,
        //   "UPSERT_TENANT_SETTINGS",
        //   oldValue,
        //   $"BankName:{dto.BankName}, TransferFee:{dto.TransferFee}, MainCurrency:{dto.MainCurrency}"
        //);

        return ToDto(settings);
    }

    public async Task<TenantSettingsResponseDto> GetAsync()
    {
        var db = await _factory.CreateAsync();
        var settings = await db.TenantSettings.FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Este tenant no tiene configuración aún");

        return ToDto(settings);
    }

    public async Task<PublicTenantSettingsDto> GetPublicAsync()
    {
        var db = await _factory.CreateAsync();
        var settings = await db.TenantSettings.FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Este tenant no tiene configuración aún");

        return new PublicTenantSettingsDto
        {
            BankName = settings.BankName,
            PrimaryColor = settings.PrimaryColor,
            SecondaryColor = settings.SecondaryColor,
            LogoUrl = settings.LogoUrl,
            MainCurrency = settings.MainCurrency
        };
    }

    // ── Privado ───────────────────────────────────────────────────────────────

    private static TenantSettingsResponseDto ToDto(TenantSettings s) => new()
    {
        Id = s.Id,
        TransferFee = s.TransferFee,
        ExchangeFeePercent = s.ExchangeFeePercent,
        MinTransactionAmount = s.MinTransactionAmount,
        MaxTransactionAmount = s.MaxTransactionAmount,
        DailyLimit = s.DailyLimit,
        MainCurrency = s.MainCurrency,
        CommissionType = s.CommissionType,
        CommissionValue = s.CommissionValue,
        ExchangeRates = JsonSerializer.Deserialize<Dictionary<string, decimal>>(s.ExchangeRates) ?? [],
        BankName = s.BankName,
        PrimaryColor = s.PrimaryColor,
        SecondaryColor = s.SecondaryColor,
        LogoUrl = s.LogoUrl,
        UpdatedAt = s.UpdatedAt
    };
}

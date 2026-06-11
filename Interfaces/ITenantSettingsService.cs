using BankOs.DTOs.TenantSettings;

namespace BankOs.Interfaces;

public interface ITenantSettingsService
{
    Task<TenantSettingsResponseDto> UpsertAsync(UpsertTenantSettingsDto dto, Guid adminUserId);
    Task<TenantSettingsResponseDto> GetAsync();
    Task<PublicTenantSettingsDto> GetPublicAsync();
}
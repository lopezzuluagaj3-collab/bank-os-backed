using BankOs.DTOs.Account;

namespace BankOs.Interfaces;

public interface IAccountService
{
    Task<AccountResponseDto> CreateAsync(CreateAccountDto dto, Guid adminUserId);
    Task<List<AccountResponseDto>> GetByUserAsync(Guid userId);
    Task<List<AccountResponseDto>> GetAllAsync();
    Task<AccountResponseDto> DeactivateAsync(Guid accountId, Guid adminUserId);
}
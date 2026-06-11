using BankOs.Data;
using BankOs.DTOs.Account;
using BankOs.Interfaces;
using BankOs.Models;
using Microsoft.EntityFrameworkCore;

namespace BankOs.Services;

public class AccountService : IAccountService
{
    private readonly TenantDbContextFactory _factory;

    public AccountService(TenantDbContextFactory factory)
    {
        _factory = factory;
    }

    public async Task<AccountResponseDto> CreateAsync(CreateAccountDto dto, Guid adminUserId)
    {
        var db = await _factory.CreateAsync();

        var userExists = await db.Users.AnyAsync(u => u.Id == dto.UserId);
        if (!userExists)
            throw new InvalidOperationException("El usuario no existe en este tenant");

        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            Number = GenerateAccountNumber(),
            Balance = 0,
            Currency = dto.Currency.ToUpper(),
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };

        db.Accounts.Add(account);

        await db.AppendAuditLogAsync(
            adminUserId,
            "CREATE_ACCOUNT",
            null,
            $"Cuenta {account.Number} creada para usuario {dto.UserId}"
        );

        await db.SaveChangesAsync();
        return ToDto(account);
    }

    public async Task<List<AccountResponseDto>> GetByUserAsync(Guid userId)
    {
        var db = await _factory.CreateAsync();
        var accounts = await db.Accounts
            .Where(a => a.UserId == userId)
            .ToListAsync();

        return accounts.Select(ToDto).ToList();
    }

    public async Task<List<AccountResponseDto>> GetAllAsync()
    {
        var db = await _factory.CreateAsync();
        var accounts = await db.Accounts.ToListAsync();
        return accounts.Select(ToDto).ToList();
    }

    public async Task<AccountResponseDto> DeactivateAsync(Guid accountId, Guid adminUserId)
    {
        var db = await _factory.CreateAsync();

        var account = await db.Accounts.FindAsync(accountId)
            ?? throw new InvalidOperationException("Cuenta no encontrada");

        var oldStatus = account.Status;
        account.Status = "inactive";

        await db.AppendAuditLogAsync(
            adminUserId,
            "DEACTIVATE_ACCOUNT",
            oldStatus,
            "inactive"
        );

        await db.SaveChangesAsync();
        return ToDto(account);
    }

    private static string GenerateAccountNumber()
        => $"BNK{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";

    private static AccountResponseDto ToDto(Account account) => new()
    {
        Id = account.Id,
        Number = account.Number,
        Balance = account.Balance,
        Currency = account.Currency,
        Status = account.Status,
        UserId = account.UserId,
        CreatedAt = account.CreatedAt
    };
}
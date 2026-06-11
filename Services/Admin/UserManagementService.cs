using BankOs.Data;
using BankOs.DTOs.Account;
using BankOs.DTOs.Admin;
using BankOs.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankOs.Services.Admin;

public class UserManagementService : IUserManagementService
{
    private readonly TenantDbContextFactory _factory;

    public UserManagementService(TenantDbContextFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<UserListItemDto>> GetAllUsersAsync(Guid adminId)
    {
        var db = await _factory.CreateForAdminAsync(adminId);

        return await db.Users
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                AccountCount = u.Accounts.Count
            })
            .ToListAsync();
    }

    public async Task<UserDetailDto> GetUserByIdAsync(Guid userId, Guid adminId)
    {
        var db = await _factory.CreateForAdminAsync(adminId);

        var user = await db.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("Usuario no encontrado");

        return new UserDetailDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Accounts = user.Accounts.Select(a => new AccountResponseDto
            {
                Id = a.Id,
                Number = a.Number,
                Balance = a.Balance,
                Currency = a.Currency,
                Status = a.Status,
                UserId = a.UserId,
                CreatedAt = a.CreatedAt
            }).ToList()
        };
    }

    public async Task<UserListItemDto> ToggleUserStatusAsync(Guid userId, Guid adminId)
    {
        var db = await _factory.CreateForAdminAsync(adminId);

        var user = await db.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("Usuario no encontrado");

        var oldStatus = user.IsActive.ToString();
        user.IsActive = !user.IsActive;

        await db.AppendAuditLogAsync(
            adminId,
            user.IsActive ? "ACTIVATE_USER" : "DEACTIVATE_USER",
            oldStatus,
            user.IsActive.ToString()
        );

        await db.SaveChangesAsync();

        return new UserListItemDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            AccountCount = user.Accounts.Count
        };
    }
}
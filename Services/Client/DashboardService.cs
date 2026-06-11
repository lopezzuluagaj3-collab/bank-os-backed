using BankOs.Data;
using BankOs.DTOs.Dashboard;
using BankOs.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankOs.Services;

public class DashboardService : IDashboardService
{
    private readonly TenantDbContextFactory _factory;

    public DashboardService(TenantDbContextFactory factory)
    {
        _factory = factory;
    }

    public async Task<DashboardDto> GetAsync(Guid userId)
    {
        await using var db = await _factory.CreateAsync();

        // 1. Usuario
        var user = await db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado");

        // 2. Primera cuenta activa del usuario (la principal)
        var account = await db.Accounts
            .Where(a => a.UserId == userId && a.Status == "active")
            .OrderBy(a => a.CreatedAt)
            .FirstOrDefaultAsync();

        // 3. Últimas 10 transacciones relacionadas con la cuenta
        List<DashboardActivityDto> activity = [];

        if (account != null)
        {
            activity = await db.Transactions
                .Where(t => t.FromAccountId == account.Id || t.ToAccountId == account.Id)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(t => new DashboardActivityDto
                {
                    Id = t.Id,
                    // Si el dinero salió de esta cuenta es débito, si entró es crédito
                    Type = t.FromAccountId == account.Id ? "debit" : "credit",
                    // Negativo para débitos, positivo para créditos
                    Amount = t.FromAccountId == account.Id ? -t.Amount : t.Amount,
                    Status = t.Status,
                    Date = t.CreatedAt
                })
                .ToListAsync();
        }

        return new DashboardDto
        {
            User = new DashboardUserDto
            {
                Email = user.Email
            },
            Account = account == null ? null : new DashboardAccountDto
            {
                Id = account.Id,
                // Muestra solo los últimos 4 dígitos: "••••4290"
                MaskedNumber = $"{account.Number}",
                Balance = account.Balance,
                Currency = account.Currency,
                Status = account.Status
            },
            RecentActivity = activity
        };
    }
}
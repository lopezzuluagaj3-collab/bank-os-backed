using BankOs.Data;
using BankOs.DTOs.Transaction;
using BankOs.Interfaces;
using BankOs.Models;
using Microsoft.EntityFrameworkCore;

namespace BankOs.Services;

public class TransactionService : ITransactionService
{
    private readonly TenantDbContextFactory _factory;

    public TransactionService(TenantDbContextFactory factory)
    {
        _factory = factory;
    }

    public async Task<TransactionResponseDto> TransferAsync(Guid userId, CreateTransactionDto dto, string idempotencyKey)
    {
        var db = await _factory.CreateAsync();

        // 1. Configuración del tenant (fees y límites)
        var settings = await db.TenantSettings.FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("El tenant no tiene configuración. Contacta al administrador.");

        // 2. Validar monto contra min/max
        if (dto.Amount < settings.MinTransactionAmount)
            throw new InvalidOperationException($"El monto mínimo permitido es {settings.MinTransactionAmount}");

        if (dto.Amount > settings.MaxTransactionAmount)
            throw new InvalidOperationException($"El monto máximo permitido es {settings.MaxTransactionAmount}");

        // 3. Cuenta origen: la primera cuenta activa del usuario
        var fromAccount = await db.Accounts
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Status == "active")
            ?? throw new InvalidOperationException("No tienes una cuenta activa para realizar transferencias");

        // 4. Cuenta destino: buscar por número
        var toAccount = await db.Accounts
            .FirstOrDefaultAsync(a => a.Number == dto.ToAccountNumber && a.Status == "active")
            ?? throw new InvalidOperationException($"La cuenta destino '{dto.ToAccountNumber}' no existe o está inactiva");

        // 5. No transferir a la misma cuenta
        if (fromAccount.Id == toAccount.Id)
            throw new InvalidOperationException("No puedes transferir dinero a tu propia cuenta");

        // 6. Verificar límite diario
        var today = DateTime.UtcNow.Date;
        var spentToday = await db.Transactions
            .Where(t => t.FromAccountId == fromAccount.Id
                     && t.Status == "completed"
                     && t.CreatedAt >= today)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        if (spentToday + dto.Amount > settings.DailyLimit)
            throw new InvalidOperationException(
                $"Límite diario excedido. Has usado {spentToday} de {settings.DailyLimit} hoy.");

        // 7. Calcular comisión y total a debitar
        var commission = settings.TransferFee;
        var totalDebited = dto.Amount + commission;

        // 8. Verificar saldo suficiente (monto + comisión)
        if (fromAccount.Balance < totalDebited)
            throw new InvalidOperationException(
                $"Saldo insuficiente. Necesitas {totalDebited} ({dto.Amount} + {commission} de comisión), tienes {fromAccount.Balance}");

        // 9. Ejecutar dentro de una transacción de BD
        await using var dbTransaction = await db.Database.BeginTransactionAsync();
        try
        {
            fromAccount.Balance -= totalDebited;
            toAccount.Balance += dto.Amount;

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                FromAccountId = fromAccount.Id,
                ToAccountId = toAccount.Id,
                Type = "transfer",
                Amount = dto.Amount,
                Commission = commission,
                ConvertedAmount = null,   // mismo tenant, sin cambio de divisa por ahora
                ExchangeRate = null,
                Status = "completed",
                IdempotencyKey = idempotencyKey,
                CorrelationId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };

            db.Transactions.Add(transaction);

            db.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "TRANSFER",
                OldValue = $"From:{fromAccount.Number} balance={fromAccount.Balance + totalDebited}",
                NewValue = $"To:{toAccount.Number} amount={dto.Amount} commission={commission}",
                CreatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return ToDto(transaction, fromAccount.Number, toAccount.Number);
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<TransactionResponseDto>> GetMyHistoryAsync(Guid userId)
    {
        var db = await _factory.CreateAsync();

        // Obtener todas las cuentas del usuario
        var accountIds = await db.Accounts
            .Where(a => a.UserId == userId)
            .Select(a => a.Id)
            .ToListAsync();

        var transactions = await db.Transactions
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .Where(t => accountIds.Contains(t.FromAccountId!.Value)
                     || accountIds.Contains(t.ToAccountId!.Value))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return transactions.Select(t => ToDto(t, t.FromAccount?.Number, t.ToAccount?.Number)).ToList();
    }

    public async Task<TransactionResponseDto> GetByIdAsync(Guid transactionId, Guid userId, bool isAdmin)
    {
        var db = await _factory.CreateAsync();

        var transaction = await db.Transactions
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .FirstOrDefaultAsync(t => t.Id == transactionId)
            ?? throw new KeyNotFoundException("Transacción no encontrada");

        // Si no es admin, verificar que la transacción le pertenece
        if (!isAdmin)
        {
            var accountIds = await db.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => a.Id)
                .ToListAsync();

            var belongs = accountIds.Contains(transaction.FromAccountId ?? Guid.Empty)
                       || accountIds.Contains(transaction.ToAccountId ?? Guid.Empty);

            if (!belongs)
                throw new UnauthorizedAccessException("No tienes acceso a esta transacción");
        }

        return ToDto(transaction, transaction.FromAccount?.Number, transaction.ToAccount?.Number);
    }

    public async Task<List<TransactionResponseDto>> GetAllAsync()
    {
        var db = await _factory.CreateAsync();

        var transactions = await db.Transactions
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return transactions.Select(t => ToDto(t, t.FromAccount?.Number, t.ToAccount?.Number)).ToList();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────────

    private static TransactionResponseDto ToDto(Transaction t, string? fromNumber, string? toNumber) => new()
    {
        Id = t.Id,
        FromAccountId = t.FromAccountId,
        FromAccountNumber = fromNumber,
        ToAccountId = t.ToAccountId,
        ToAccountNumber = toNumber,
        Type = t.Type,
        Amount = t.Amount,
        Commission = t.Commission,
        TotalDebited = t.Amount + t.Commission,
        Status = t.Status,
        IdempotencyKey = t.IdempotencyKey,
        CorrelationId = t.CorrelationId,
        CreatedAt = t.CreatedAt
    };
}
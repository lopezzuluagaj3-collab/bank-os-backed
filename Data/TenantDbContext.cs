// ── DIFF: Data/TenantDbContext.cs ────────────────────────────────────────────
// Dos cambios:
//   1. Agregar DbSet<IdempotencyKey>
//   2. Agregar índice único (Key, TenantId) en OnModelCreating
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using BankOs.Models;

namespace BankOs.Data;

public class TenantDbContext : DbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();  // ← NUEVO

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Account>()
            .HasIndex(a => a.Number)
            .IsUnique();

        builder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        builder.Entity<Transaction>()
            .HasOne(t => t.FromAccount)
            .WithMany()
            .HasForeignKey(t => t.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Transaction>()
            .HasOne(t => t.ToAccount)
            .WithMany()
            .HasForeignKey(t => t.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── NUEVO: IdempotencyKey única por (Key + TenantId) ─────────────────
        builder.Entity<IdempotencyKey>()
            .HasKey(k => k.Key);

        builder.Entity<IdempotencyKey>()
            .HasIndex(k => new { k.Key, k.TenantId })
            .IsUnique();
    }

    public async Task AppendAuditLogAsync(Guid userId, string action, string? oldValue, string? newValue)
    {
        AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            OldValue = oldValue,
            NewValue = newValue,
            CreatedAt = DateTime.UtcNow
        });
        await SaveChangesAsync();
    }
}
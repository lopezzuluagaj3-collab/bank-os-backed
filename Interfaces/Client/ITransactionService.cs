using BankOs.DTOs.Transaction;

namespace BankOs.Interfaces;

public interface ITransactionService
{
    /// <summary>
    /// Transfiere dinero desde la cuenta activa del usuario hacia una cuenta destino (por número).
    /// </summary>
    Task<TransactionResponseDto> TransferAsync(Guid userId, CreateTransactionDto dto, string idempotencyKey);

    /// <summary>
    /// Historial de transacciones del usuario autenticado.
    /// </summary>
    Task<List<TransactionResponseDto>> GetMyHistoryAsync(Guid userId);

    /// <summary>
    /// Detalle de una transacción (el usuario debe ser dueño de alguna de las cuentas).
    /// </summary>
    Task<TransactionResponseDto> GetByIdAsync(Guid transactionId, Guid userId, bool isAdmin);

    /// <summary>
    /// Todas las transacciones del tenant (solo admin).
    /// </summary>
    Task<List<TransactionResponseDto>> GetAllAsync();
}
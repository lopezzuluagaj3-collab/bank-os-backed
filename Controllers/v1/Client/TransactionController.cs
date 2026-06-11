using BankOs.DTOs.Transaction;
using BankOs.Interfaces;
using BankOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankOs.Controllers.v1;

[ApiController]
[Route("{tenantSlug}/api/v1/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst("user_id")!.Value);

    private bool IsAdmin =>
        User.FindFirst("role")?.Value == "administrador";

    /// <summary>
    /// POST /api/v1/transactions
    /// Requiere header: X-Idempotency-Key: {uuid}
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Transfer([FromBody] CreateTransactionDto dto)
    {
        // El middleware ya validó que el header existe y lo pasó como "processing".
        // Lo leemos aquí para pasarlo al servicio y guardarlo en la transacción.
        var idempotencyKey = Request.Headers["X-Idempotency-Key"].ToString();

        try
        {
            var result = await _transactionService.TransferAsync(CurrentUserId, dto, idempotencyKey);
            return StatusCode(201, ApiResponse<TransactionResponseDto>.Ok(result, "Transferencia realizada exitosamente"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TransactionResponseDto>.Fail("TRANSACTION_ERROR", ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// GET /api/v1/transactions/me — historial del usuario autenticado
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyHistory()
    {
        var result = await _transactionService.GetMyHistoryAsync(CurrentUserId);
        return Ok(ApiResponse<List<TransactionResponseDto>>.Ok(result));
    }

    /// <summary>
    /// GET /api/v1/transactions/{id} — detalle (propio o admin)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _transactionService.GetByIdAsync(id, CurrentUserId, IsAdmin);
            return Ok(ApiResponse<TransactionResponseDto>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TransactionResponseDto>.Fail("NOT_FOUND", ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<TransactionResponseDto>.Fail("FORBIDDEN", ex.Message));
        }
    }

    /// <summary>
    /// GET /api/v1/transactions — todas (solo admin)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _transactionService.GetAllAsync();
        return Ok(ApiResponse<List<TransactionResponseDto>>.Ok(result));
    }
}
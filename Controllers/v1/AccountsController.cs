using BankOs.DTOs.Account;
using BankOs.Interfaces;
using BankOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankOs.Controllers.v1;

[ApiController]
[Route("{tenantSlug}/api/v1/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst("user_id")!.Value);

    [HttpPost]
    [Authorize(Policy = "AdminGlobal")]
    public async Task<IActionResult> Create([FromBody] CreateAccountDto dto)
    {
        try
        {
            var result = await _accountService.CreateAsync(dto, CurrentUserId);
            return StatusCode(201, ApiResponse<AccountResponseDto>.Ok(result, "Cuenta creada exitosamente"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AccountResponseDto>.Fail("ACCOUNT_ERROR", ex.Message));
        }
    }

    [HttpGet]
    [Authorize(Policy = "AdminGlobal")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _accountService.GetAllAsync();
        return Ok(ApiResponse<List<AccountResponseDto>>.Ok(result));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyAccounts()
    {
        var result = await _accountService.GetByUserAsync(CurrentUserId);
        return Ok(ApiResponse<List<AccountResponseDto>>.Ok(result));
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = "AdminGlobal")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        var result = await _accountService.GetByUserAsync(userId);
        return Ok(ApiResponse<List<AccountResponseDto>>.Ok(result));
    }

    [HttpPatch("{accountId}/deactivate")]
    [Authorize(Policy = "AdminGlobal")]
    public async Task<IActionResult> Deactivate(Guid accountId)
    {
        try
        {
            var result = await _accountService.DeactivateAsync(accountId, CurrentUserId);
            return Ok(ApiResponse<AccountResponseDto>.Ok(result, "Cuenta desactivada exitosamente"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<AccountResponseDto>.Fail("ACCOUNT_NOT_FOUND", ex.Message));
        }
    }
}
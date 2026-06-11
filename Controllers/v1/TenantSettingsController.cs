using BankOs.DTOs.TenantSettings;
using BankOs.Interfaces;
using BankOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankOs.Controllers.v1;

[ApiController]
[Route("{tenantSlug}/api/v1/settings")]
public class TenantSettingsController : ControllerBase
{
    private readonly ITenantSettingsService _settingsService;

    public TenantSettingsController(ITenantSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst("user_id")!.Value);

    // GET /{slug}/api/v1/settings/public
    // Sin auth — Flutter lo llama antes del login para cargar branding
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublic()
    {
        try
        {
            var result = await _settingsService.GetPublicAsync();
            return Ok(ApiResponse<PublicTenantSettingsDto>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<PublicTenantSettingsDto>.Fail("SETTINGS_NOT_FOUND", ex.Message));
        }
    }

    // GET /{slug}/api/v1/settings
    // Solo admin
    [HttpGet]
    [Authorize(Policy = "AdminGlobal")]
    public async Task<IActionResult> Get()
    {
        try
        {
            var result = await _settingsService.GetAsync();
            return Ok(ApiResponse<TenantSettingsResponseDto>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<TenantSettingsResponseDto>.Fail("SETTINGS_NOT_FOUND", ex.Message));
        }
    }

    // PUT /{slug}/api/v1/settings
    // Solo admin
    [HttpPut]
    [Authorize(Policy = "AdminGlobal")]
    public async Task<IActionResult> Upsert([FromBody] UpsertTenantSettingsDto dto)
    {
        try
        {
            var result = await _settingsService.UpsertAsync(dto, CurrentUserId);
            return Ok(ApiResponse<TenantSettingsResponseDto>.Ok(result, "Configuración actualizada"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TenantSettingsResponseDto>.Fail("SETTINGS_ERROR", ex.Message));
        }
    }
}
using BankOs.DTOs.Dashboard;
using BankOs.Interfaces;
using BankOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankOs.Controllers.v1;

[ApiController]
[Route("{tenantSlug}/api/v1/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // GET /bancolombia/api/v1/dashboard
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Lee el user_id directamente del JWT, el usuario no puede pasarlo manualmente
            var userIdClaim = User.FindFirst("user_id")?.Value
                              ?? throw new UnauthorizedAccessException("Token inválido");

            var userId = Guid.Parse(userIdClaim);
            var result = await _dashboardService.GetAsync(userId);

            return Ok(ApiResponse<DashboardDto>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail("USER_NOT_FOUND", ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail("UNAUTHORIZED", ex.Message));
        }
    }
}
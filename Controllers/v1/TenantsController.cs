using BankOs.Data;
using BankOs.DTOs.Tenant;
using BankOs.Interfaces;
using BankOs.Models;
using BankOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankOs.Controllers.v1;

[ApiController]
[Route("api/v1/tenants")]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly MasterDbContext _master;

    public TenantsController(ITenantService tenantService, MasterDbContext master)
    {
        _tenantService = tenantService;
        _master = master;
    }

    [HttpPost]
    [Authorize(Policy = "AdminGlobal")]
    public async Task<IActionResult> Create([FromBody] CreateTenantDto dto)
    {
        // Extraer admin_id del JWT
        var adminIdClaim = User.FindFirst("admin_id")?.Value;
        if (adminIdClaim == null || !Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized(ApiResponse<TenantRegistry>.Fail(
                "UNAUTHORIZED", "Token inválido"));

        var admin = await _master.AdminUsers.FindAsync(adminId);
        if (admin == null)
            return Unauthorized(ApiResponse<TenantRegistry>.Fail(
                "UNAUTHORIZED", "Admin no encontrado"));

        // Bloqueo: un admin solo puede tener un tenant
        if (admin.TenantId.HasValue)
            return Conflict(ApiResponse<TenantRegistry>.Fail(
                "TENANT_ALREADY_EXISTS",
                "Este admin ya tiene un tenant asociado"));

        try
        {
            var tenant = await _tenantService.CreateAsync(dto);

            // Vincular el tenant al admin
            admin.TenantId = tenant.Id;
            await _master.SaveChangesAsync();

            return StatusCode(201, ApiResponse<TenantRegistry>.Ok(
                tenant, "Tenant creado exitosamente"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<TenantRegistry>.Fail(
                "TENANT_SLUG_CONFLICT", ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tenants = await _tenantService.GetAllAsync();
        return Ok(ApiResponse<List<TenantRegistry>>.Ok(tenants));
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var tenant = await _tenantService.GetBySlugAsync(slug);
        if (tenant == null)
            return NotFound(ApiResponse<TenantRegistry>.Fail(
                "TENANT_NOT_FOUND", $"No existe un tenant con slug '{slug}'"));

        return Ok(ApiResponse<TenantRegistry>.Ok(tenant));
    }
}
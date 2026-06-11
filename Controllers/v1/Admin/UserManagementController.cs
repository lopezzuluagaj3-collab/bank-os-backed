using BankOs.DTOs.Admin;
using BankOs.Interfaces;
using BankOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankOs.Controllers.v1.Admin;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Policy = "AdminGlobal")]
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userService;

    public UserManagementController(IUserManagementService userService)
    {
        _userService = userService;
    }

    private Guid CurrentAdminId =>
        Guid.Parse(User.FindFirst("admin_id")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync(CurrentAdminId);
            return Ok(ApiResponse<List<UserListItemDto>>.Ok(users));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<List<UserListItemDto>>.Fail("ERROR", ex.Message));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id, CurrentAdminId);
            return Ok(ApiResponse<UserDetailDto>.Ok(user));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<UserDetailDto>.Fail("USER_NOT_FOUND", ex.Message));
        }
    }

    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id)
    {
        try
        {
            var result = await _userService.ToggleUserStatusAsync(id, CurrentAdminId);
            var msg = result.IsActive ? "Usuario activado" : "Usuario desactivado";
            return Ok(ApiResponse<UserListItemDto>.Ok(result, msg));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<UserListItemDto>.Fail("USER_NOT_FOUND", ex.Message));
        }
    }
}
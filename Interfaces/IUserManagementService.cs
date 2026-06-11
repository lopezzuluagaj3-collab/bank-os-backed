using BankOs.DTOs.Admin;

namespace BankOs.Interfaces;

public interface IUserManagementService
{
    Task<List<UserListItemDto>> GetAllUsersAsync(Guid adminId);
    Task<UserDetailDto> GetUserByIdAsync(Guid userId, Guid adminId);
    Task<UserListItemDto> ToggleUserStatusAsync(Guid userId, Guid adminId);
}
using BankOs.DTOs.Dashboard;

namespace BankOs.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(Guid userId);
}
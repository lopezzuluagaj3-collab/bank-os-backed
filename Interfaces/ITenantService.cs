using BankOs.DTOs.Tenant;
using BankOs.Models;

namespace BankOs.Interfaces;

public interface ITenantService
{
    Task<TenantRegistry> CreateAsync(CreateTenantDto dto);
    Task<TenantRegistry?> GetBySlugAsync(string slug);
    Task<List<TenantRegistry>> GetAllAsync();
}
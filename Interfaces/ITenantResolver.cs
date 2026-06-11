using BankOs.Models;

namespace BankOs.Interfaces;

public interface ITenantResolver
{
    Task<TenantRegistry?> ResolveAsync();
}
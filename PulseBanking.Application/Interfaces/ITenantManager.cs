// Update src/PulseBanking.Application/Interfaces/ITenantManager.cs
using PulseBanking.Application.Common.Models;

namespace PulseBanking.Application.Interfaces;

public interface ITenantManager
{
    Task<TenantSettings> GetTenantAsync(string tenantId);
    Task<IEnumerable<TenantSettings>> GetAllTenantsAsync();
    Task<bool> TenantExistsAsync(string tenantId);
}
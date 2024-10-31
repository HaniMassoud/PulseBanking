// Create new file: src/PulseBanking.WebApp/Services/ITenantService.cs
using PulseBanking.WebApp.Models;

namespace PulseBanking.WebApp.Services;

public interface ITenantService
{
    Task RegisterTenantAsync(TenantRegistrationModel model);
}
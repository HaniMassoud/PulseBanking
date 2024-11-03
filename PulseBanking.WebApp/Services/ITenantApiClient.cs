using PulseBanking.WebApp.Models;

namespace PulseBanking.WebApp.Services;

public interface ITenantApiClient
{
    Task RegisterTenantAsync(TenantRegistrationModel model);
}
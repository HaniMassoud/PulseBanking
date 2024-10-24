// Create new file: src/PulseBanking.Application/Interfaces/ITenantService.cs
namespace PulseBanking.Application.Interfaces;

public interface ITenantService
{
    string GetCurrentTenant();
}
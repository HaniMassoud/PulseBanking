// src/PulseBanking.Application/Interfaces/ITenantService.cs
using PulseBanking.Domain.Entities;

namespace PulseBanking.Application.Interfaces;

public interface ITenantService
{
    Tenant? GetCurrentTenant();
}
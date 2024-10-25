// Create new file: src/PulseBanking.Application/Interfaces/ITenantDbContextFactory.cs
using Microsoft.EntityFrameworkCore;

namespace PulseBanking.Application.Interfaces;

public interface ITenantDbContextFactory<TContext> where TContext : DbContext
{
    TContext CreateDbContext(string tenantId);
}
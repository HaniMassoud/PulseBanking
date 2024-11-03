// src/PulseBanking.Application/Interfaces/IApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Account> Accounts { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<BankTransaction> Transactions { get; }
    DbSet<AuditTrail> AuditTrails { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
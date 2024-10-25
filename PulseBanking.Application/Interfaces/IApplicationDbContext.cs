// src/PulseBanking.Application/Interfaces/IApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Account> Accounts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
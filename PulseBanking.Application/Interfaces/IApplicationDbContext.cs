using Microsoft.EntityFrameworkCore;
using PulseBanking.Domain.Entities;
using System.Collections.Generic;
using System.Security.Principal;
using System.Transactions;
namespace PulseBanking.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Account> Accounts { get; }
    DbSet<Customer> Customers { get; }
    DbSet<BankTransaction> Transactions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
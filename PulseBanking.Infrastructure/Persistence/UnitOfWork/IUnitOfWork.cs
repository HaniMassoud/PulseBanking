using PulseBanking.Domain.Entities;
using PulseBanking.Infrastructure.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseBanking.Infrastructure.Persistence.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IRepository<Account> Accounts { get; }
    IRepository<Customer> Customers { get; }
    IRepository<BankTransaction> Transactions { get; }
    Task<int> SaveChangesAsync();
}
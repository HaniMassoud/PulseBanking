using PulseBanking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseBanking.Application.Common.Interfaces
{
    public interface ITransactionProcessor
    {
        Task ProcessTransactionAsync(BankTransaction transaction, CancellationToken cancellationToken = default);
        Task ReverseTransactionAsync(Guid transactionId, string reason, CancellationToken cancellationToken = default);
    }
}

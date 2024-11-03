using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseBanking.Application.Common.Interfaces
{
    public interface IAuditService
    {
        void AddAuditTrail(string action, string entityName, string entityId, string? details = null);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseBanking.Application.Interfaces
{
    public interface ITenantValidator
    {
        Task ValidateAsync(CancellationToken cancellationToken);
    }
}

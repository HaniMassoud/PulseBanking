using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseBanking.Application.Common.Interfaces
{
    public interface ICurrencyConverter
    {
        Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);
        bool IsValidCurrency(string currencyCode);
    }
}

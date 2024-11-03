using Microsoft.Extensions.Logging;
using PulseBanking.Application.Common.Interfaces;
using PulseBanking.Application.Interfaces;

namespace PulseBanking.Infrastructure.Services;

public class CurrencyConverter : ICurrencyConverter
{
    private readonly ITenantService _tenantService;
    private readonly ITenantManager _tenantManager;
    private readonly ILogger<CurrencyConverter> _logger;
    private static readonly HashSet<string> ValidCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "USD", "EUR", "GBP", "AUD", "CAD", "SGD", "JPY", "CNY", "INR"
        // Add more as needed
    };

    public CurrencyConverter(
        ITenantService tenantService,
        ITenantManager tenantManager,
        ILogger<CurrencyConverter> logger)
    {
        _tenantService = tenantService;
        _tenantManager = tenantManager;
        _logger = logger;
    }

    public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
            return amount;

        if (!IsValidCurrency(fromCurrency) || !IsValidCurrency(toCurrency))
            throw new ArgumentException("Invalid currency code");

        // TODO: Implement real currency conversion
        // For now, return a mock conversion
        var rate = await GetExchangeRateAsync(fromCurrency, toCurrency);
        return Math.Round(amount * rate, 2);
    }

    private async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
    {
        // TODO: Implement real exchange rate lookup
        // For now, return mock rates
        await Task.Delay(100); // Simulate API call
        return 1.0m;
    }

    public bool IsValidCurrency(string currencyCode)
    {
        return ValidCurrencies.Contains(currencyCode);
    }
}
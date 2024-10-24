// Create new file: src/PulseBanking.Infrastructure/Services/TenantManager.cs
using Microsoft.Extensions.Configuration;
using PulseBanking.Application.Common.Models;
using PulseBanking.Application.Interfaces;

namespace PulseBanking.Infrastructure.Services;

public class TenantManager : ITenantManager
{
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, TenantSettings> _tenantSettings;

    public TenantManager(IConfiguration configuration)
    {
        _configuration = configuration;
        _tenantSettings = new Dictionary<string, TenantSettings>();
        InitializeTenantSettings();
    }

    private void InitializeTenantSettings()
    {
        var tenantsSection = _configuration.GetSection("Tenants");
        foreach (var tenantSection in tenantsSection.GetChildren())
        {
            var settings = new TenantSettings
            {
                TenantId = tenantSection["Id"] ?? throw new InvalidOperationException($"Tenant ID not found in configuration"),
                Name = tenantSection["Name"] ?? throw new InvalidOperationException($"Name not found for tenant {tenantSection["Id"]}"),
                ConnectionString = tenantSection["ConnectionString"] ?? throw new InvalidOperationException($"ConnectionString not found for tenant {tenantSection["Id"]}"),
                CurrencyCode = tenantSection["CurrencyCode"] ?? "USD",
                DefaultTransactionLimit = decimal.Parse(tenantSection["DefaultTransactionLimit"] ?? "10000"),
                IsActive = bool.Parse(tenantSection["IsActive"] ?? "true"),
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById(tenantSection["TimeZone"] ?? "UTC")
            };

            _tenantSettings[settings.TenantId] = settings;
        }
    }

    public Task<TenantSettings> GetTenantAsync(string tenantId)
    {
        if (_tenantSettings.TryGetValue(tenantId, out var settings))
        {
            return Task.FromResult(settings);
        }

        throw new KeyNotFoundException($"Tenant '{tenantId}' not found.");
    }

    public Task<IEnumerable<TenantSettings>> GetAllTenantsAsync()
    {
        return Task.FromResult(_tenantSettings.Values.AsEnumerable());
    }

    public Task<bool> TenantExistsAsync(string tenantId)
    {
        return Task.FromResult(_tenantSettings.ContainsKey(tenantId));
    }
}
// Update src/PulseBanking.Infrastructure/Services/TenantManager.cs
using Microsoft.Extensions.Configuration;
using PulseBanking.Application.Common.Models;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Enums;

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
                Id = tenantSection["Id"] ?? throw new InvalidOperationException($"Tenant ID not found in configuration"),
                Name = tenantSection["Name"] ?? throw new InvalidOperationException($"Name not found for tenant {tenantSection["Id"]}"),
                DeploymentType = Enum.Parse<DeploymentType>(tenantSection["DeploymentType"] ?? "Shared"),
                Region = Enum.Parse<RegionCode>(tenantSection["Region"] ?? "AUS"),
                InstanceType = Enum.Parse<InstanceType>(tenantSection["InstanceType"] ?? "Production"),
                ConnectionString = tenantSection["ConnectionString"] ?? throw new InvalidOperationException($"ConnectionString not found for tenant {tenantSection["Id"]}"),
                CurrencyCode = tenantSection["CurrencyCode"] ?? "USD",
                DefaultTransactionLimit = decimal.Parse(tenantSection["DefaultTransactionLimit"] ?? "10000"),
                TimeZone = tenantSection["TimeZone"] ?? "UTC",
                IsActive = bool.Parse(tenantSection["IsActive"] ?? "true"),
                CreatedAt = DateTime.Parse(tenantSection["CreatedAt"] ?? DateTime.UtcNow.ToString("O")),
                DataSovereigntyCompliant = bool.Parse(tenantSection["DataSovereigntyCompliant"] ?? "true")
            };

            _tenantSettings[settings.Id] = settings;
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
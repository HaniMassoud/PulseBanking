// Create new file: src/PulseBanking.Application/Common/Models/TenantSettings.cs
namespace PulseBanking.Application.Common.Models;

public class TenantSettings
{
    public required string TenantId { get; init; }
    public required string Name { get; init; }
    public required string ConnectionString { get; init; }
    public string TimeZone { get; init; } = "UTC";  // Changed from TimeZoneInfo to string
    public string CurrencyCode { get; init; } = "USD";
    public decimal DefaultTransactionLimit { get; init; } = 10000m;
    public bool IsActive { get; init; } = true;
}
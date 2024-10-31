// Update src/PulseBanking.Application/Features/Tenants/Commands/CreateTenant/CreateTenantCommandHandler.cs
using MediatR;
using Microsoft.Extensions.Configuration;
using PulseBanking.Application.Common.Models;
using PulseBanking.Application.Features.Tenants.Common;

namespace PulseBanking.Application.Features.Tenants.Commands.CreateTenant;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, TenantDto>
{
    private readonly IConfiguration _configuration;

    public CreateTenantCommandHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // Validate time zone
        if (!TimeZoneInfo.GetSystemTimeZones().Any(tz => tz.Id == request.TimeZone))
        {
            throw new ArgumentException("Invalid time zone", nameof(request.TimeZone));
        }

        // Generate tenant ID
        var tenantId = request.BankName.ToLower().Replace(" ", "-");

        // Create tenant configuration
        var tenantSettings = new TenantSettings
        {
            TenantId = tenantId,
            Name = request.BankName,
            ConnectionString = _configuration.GetConnectionString("DefaultConnection") ?? "",
            CurrencyCode = request.CurrencyCode,
            DefaultTransactionLimit = request.DefaultTransactionLimit,
            TimeZone = request.TimeZone,
            IsActive = true
        };

        // Add tenant to configuration
        // TODO: In a real application, this would be an async database operation
        await Task.Run(() =>
        {
            var tenants = _configuration.GetSection("Tenants")
                .Get<Dictionary<string, TenantSettings>>() ?? new();
            tenants[tenantId] = tenantSettings;
            // TODO: Save to database instead of configuration
        }, cancellationToken);

        return new TenantDto
        {
            TenantId = tenantId,
            Name = request.BankName,
            IsActive = true
        };
    }
}
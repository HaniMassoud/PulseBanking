// Update src/PulseBanking.Application/Features/Tenants/Commands/CreateTenant/CreateTenantCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PulseBanking.Application.Common.Models;
using PulseBanking.Application.Features.Tenants.Common;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;

namespace PulseBanking.Application.Features.Tenants.Commands.CreateTenant;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, TenantDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateTenantCommandHandler> _logger;

    public CreateTenantCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateTenantCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating tenant: {@Request}", request);

            var tenantId = request.BankName.ToLower().Replace(" ", "");

            // Check if tenant already exists
            var existingTenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

            if (existingTenant != null)
            {
                throw new InvalidOperationException($"Tenant with ID '{tenantId}' already exists");
            }

            var tenant = new Tenant
            {
                Id = tenantId,
                Name = request.BankName,
                DeploymentType = DeploymentType.Shared,
                Region = RegionCode.AUS,
                InstanceType = InstanceType.Production,
                ConnectionString = $"Server=(local);Database=PulseBanking_{tenantId};Trusted_Connection=True;MultipleActiveResultSets=true;Trust Server Certificate=True;",
                CurrencyCode = request.CurrencyCode,
                DefaultTransactionLimit = request.DefaultTransactionLimit,
                TimeZone = request.TimeZone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.AdminEmail,
                DataSovereigntyCompliant = true
            };

            _logger.LogInformation("Adding tenant to database: {@Tenant}", tenant);

            await _context.Tenants.AddAsync(tenant, cancellationToken);
            var saveResult = await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("SaveChanges result: {SaveResult}", saveResult);
            _logger.LogInformation("Successfully created tenant: {TenantId}", tenant.Id);

            return new TenantDto
            {
                TenantId = tenant.Id,
                Name = tenant.Name,
                IsActive = tenant.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant: {Message}", ex.Message);
            throw;
        }
    }
}
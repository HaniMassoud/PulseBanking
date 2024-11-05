// Update src/PulseBanking.Application/Features/Tenants/Commands/CreateTenant/CreateTenantCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PulseBanking.Application.Common.Models;
using PulseBanking.Application.Features.Tenants.Common;
using PulseBanking.Application.Features.Users.Common;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;

namespace PulseBanking.Application.Features.Tenants.Commands.CreateTenant;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, TenantDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateTenantCommandHandler> _logger;
    private readonly IUserService _userService;

    public CreateTenantCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateTenantCommandHandler> logger,
        IUserService userService)
    {
        _context = context;
        _logger = logger;
        _userService = userService;
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

            await _context.Tenants.AddAsync(tenant, cancellationToken);
            var saveResult = await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created tenant: {TenantId}", tenant.Id);

            // Create admin user
            var createUserDto = new CreateUserDto
            {
                UserName = request.AdminEmail,
                Email = request.AdminEmail,
                Password = request.AdminPassword,
                PhoneNumber = request.AdminPhoneNumber,
                Roles = new List<string> { "TenantAdmin" },
                TenantId = tenant.Id
            };

            var createUserResult = await _userService.CreateAsync(createUserDto);
            if (!createUserResult.Succeeded)
            {
                _logger.LogError("Failed to create admin user for tenant {TenantId}", tenant.Id);
                throw new Exception("Failed to create admin user");
            }

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
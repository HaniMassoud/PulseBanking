using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseBanking.Application.Common.Models;
using PulseBanking.Application.Features.Tenants.Common;
using PulseBanking.Application.Features.Users.Common;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Constants;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;

namespace PulseBanking.Application.Features.Tenants.Commands.CreateTenant;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, TenantDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateTenantCommandHandler> _logger;
    private readonly IUserService _userService;
    private readonly RoleManager<CustomIdentityRole> _roleManager;

    public CreateTenantCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateTenantCommandHandler> logger,
        IUserService userService,
        RoleManager<CustomIdentityRole> roleManager)
    {
        _context = context;
        _logger = logger;
        _userService = userService;
        _roleManager = roleManager;
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
                DeploymentType = request.DeploymentType,
                Region = request.Region,
                InstanceType = request.InstanceType,
                ConnectionString = $"Server=(local);Database=PulseBanking_{tenantId};Trusted_Connection=True;MultipleActiveResultSets=true;Trust Server Certificate=True;",
                CurrencyCode = request.CurrencyCode,
                DefaultTransactionLimit = request.DefaultTransactionLimit,
                TimeZone = request.TimeZone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.AdminEmail,
                DataSovereigntyCompliant = request.DataSovereigntyCompliant
            };

            await _context.Tenants.AddAsync(tenant, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created tenant: {TenantId}", tenant.Id);

            // Create roles for the tenant
            await CreateTenantRolesAsync(tenant.Id);

            // Create admin user
            var createUserDto = new CreateUserDto
            {
                UserName = request.AdminEmail,
                Email = request.AdminEmail,
                Password = request.AdminPassword,
                PhoneNumber = request.AdminPhoneNumber,
                Roles = new List<string> { PulseBanking.Domain.Constants.Roles.TenantAdmin },
                TenantId = tenant.Id
            };

            _logger.LogInformation("Creating admin user with roles: {Roles}", string.Join(", ", createUserDto.Roles));

            var createUserResult = await _userService.CreateAsync(createUserDto);
            if (!createUserResult.Succeeded)
            {
                _logger.LogError("Failed to create admin user for tenant {TenantId}: {Errors}",
                    tenant.Id, string.Join(", ", createUserResult.Errors));
                throw new Exception("Failed to create admin user: " +
                    string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
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

    private async Task CreateTenantRolesAsync(string tenantId)
    {
        foreach (var roleName in PulseBanking.Domain.Constants.Roles.AllRoles)
        {
            var normalizedRoleName = roleName.ToUpperInvariant();
            var roleExists = await _roleManager.FindByNameAsync(normalizedRoleName);

            if (roleExists == null)
            {
                _logger.LogInformation("Creating role {RoleName} for tenant {TenantId}", roleName, tenantId);

                var roleDescription = PulseBanking.Domain.Constants.Roles.RoleDescriptions[roleName];
                var role = CustomIdentityRole.Create(tenantId, roleName, roleDescription);

                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to create role {RoleName}: {Errors}",
                        roleName, string.Join(", ", result.Errors));
                    throw new Exception($"Failed to create role {roleName}: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
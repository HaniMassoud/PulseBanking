// Services/RoleService.cs
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PulseBanking.Application.Features.Roles.Common;
using PulseBanking.Domain.Entities;
using PulseBanking.Application.Interfaces;
using Microsoft.Extensions.Logging;
using PulseBanking.Application.Common.Exceptions;

namespace PulseBanking.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<CustomIdentityRole> _roleManager;
    private readonly ITenantService _tenantService;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        RoleManager<CustomIdentityRole> roleManager, 
        ITenantService tenantService,
        ILogger<RoleService> logger)
    {
        _roleManager = roleManager;
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task<CustomIdentityRole> GetByIdAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            throw new NotFoundException(nameof(CustomIdentityRole), id);

        return role;
    }

    public async Task<CustomIdentityRole> FindByNameAsync(string name)
    {
        var tenant = _tenantService.GetCurrentTenant()
            ?? throw new UnauthorizedAccessException("No tenant context found");

        var role = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.NormalizedName == name.ToUpper() && r.TenantId == tenant.Id);

        if (role == null)
            throw new NotFoundException(nameof(CustomIdentityRole), name);

        return role;

        //var role = await _roleManager.FindByNameAsync(name);
        //if (role == null)
        //    throw new NotFoundException(nameof(CustomIdentityRole), name);

        //return role;
    }

    public async Task<IEnumerable<CustomIdentityRole>> GetAllAsync()
    {
        var tenant = _tenantService.GetCurrentTenant()
            ?? throw new UnauthorizedAccessException("No tenant context found");

        return await _roleManager.Roles
            .Where(r => r.TenantId == tenant.Id)
            .ToListAsync();

        //var roles = await _roleManager.Roles.ToListAsync();
        //return roles;
    }

    public async Task<IdentityResult> CreateAsync(CreateRoleDto createRoleDto)
    {
        var tenant = _tenantService.GetCurrentTenant()
            ?? throw new UnauthorizedAccessException("No tenant context found");

        var role = CustomIdentityRole.Create(
            tenant.Id,
            createRoleDto.Name,
            createRoleDto.Description);

        var result = await _roleManager.CreateAsync(role);

        if (result.Succeeded)
        {
            _logger.LogInformation(
                "Created role {RoleName} for tenant {TenantId}",
                role.Name,
                role.TenantId);
        }
        else
        {
            _logger.LogWarning(
                "Failed to create role {RoleName} for tenant {TenantId}: {Errors}",
                role.Name,
                role.TenantId,
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return result;
    }

    public async Task<IdentityResult> UpdateAsync(string id, UpdateRoleDto updateRoleDto)
    {
        var role = await GetByIdAsync(id);

        role.Name = updateRoleDto.Name;
        role.Description = updateRoleDto.Description;

        return await _roleManager.UpdateAsync(role);
    }

    public async Task<IdentityResult> DeleteAsync(string id)
    {
        var role = await GetByIdAsync(id);
        if (role == null)
            return IdentityResult.Failed(new IdentityError { Description = "Role not found" });

        return await _roleManager.DeleteAsync(role);
    }
}
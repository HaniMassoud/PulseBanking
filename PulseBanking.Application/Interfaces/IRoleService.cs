// Services/Interfaces/IRoleService.cs
using Microsoft.AspNetCore.Identity;
using PulseBanking.Application.Features.Roles.Common;
using PulseBanking.Domain.Entities;

public interface IRoleService
{
    Task<CustomIdentityRole> GetByIdAsync(string id);
    Task<CustomIdentityRole> FindByNameAsync(string name);
    Task<IEnumerable<CustomIdentityRole>> GetAllAsync();
    Task<IdentityResult> CreateAsync(CreateRoleDto createRoleDto);
    Task<IdentityResult> UpdateAsync(string id, UpdateRoleDto updateRoleDto);
    Task<IdentityResult> DeleteAsync(string id);

    //Task<RoleDto> GetByIdAsync(string id);
    //Task<RoleDto> FindByNameAsync(string name);
    //Task<IEnumerable<RoleDto>> GetAllAsync();
    //Task<IdentityResult> CreateAsync(CreateRoleDto createRoleDto);
    //Task<IdentityResult> DeleteAsync(string id);
}
// Services/Interfaces/IRoleService.cs
using Microsoft.AspNetCore.Identity;
using PulseBanking.Application.Features.Roles.Common;

public interface IRoleService
{
    Task<RoleDto> GetByIdAsync(string id);
    Task<RoleDto> FindByNameAsync(string name);
    Task<IEnumerable<RoleDto>> GetAllAsync();
    Task<IdentityResult> CreateAsync(CreateRoleDto createRoleDto);
    Task<IdentityResult> DeleteAsync(string id);
}
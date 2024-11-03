// Services/RoleService.cs
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PulseBanking.Application.Features.Roles.Common;

public class RoleService : IRoleService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;

    public RoleService(RoleManager<IdentityRole> roleManager, IMapper mapper)
    {
        _roleManager = roleManager;
        _mapper = mapper;
    }

    public async Task<RoleDto> GetByIdAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        return _mapper.Map<RoleDto>(role);
    }

    public async Task<RoleDto> FindByNameAsync(string name)
    {
        var role = await _roleManager.FindByNameAsync(name);
        return _mapper.Map<RoleDto>(role);
    }

    public async Task<IEnumerable<RoleDto>> GetAllAsync()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return _mapper.Map<IEnumerable<RoleDto>>(roles);
    }

    public async Task<IdentityResult> CreateAsync(CreateRoleDto createRoleDto)
    {
        var role = _mapper.Map<IdentityRole>(createRoleDto);
        return await _roleManager.CreateAsync(role);
    }

    public async Task<IdentityResult> DeleteAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return IdentityResult.Failed(new IdentityError { Description = "Role not found" });

        return await _roleManager.DeleteAsync(role);
    }
}
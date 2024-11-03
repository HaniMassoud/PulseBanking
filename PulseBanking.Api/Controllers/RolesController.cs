// Controllers/RolesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulseBanking.Application.Features.Roles.Common;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetById(string id)
    {
        var role = await _roleService.GetByIdAsync(id);
        if (role == null)
            return NotFound();
        return Ok(role);
    }

    [HttpGet("find-by-name")]
    public async Task<ActionResult<RoleDto>> FindByName([FromQuery] string name)
    {
        var role = await _roleService.FindByNameAsync(name);
        if (role == null)
            return NotFound();
        return Ok(role);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll()
    {
        var roles = await _roleService.GetAllAsync();
        return Ok(roles);
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleDto createRoleDto)
    {
        var result = await _roleService.CreateAsync(createRoleDto);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var role = await _roleService.FindByNameAsync(createRoleDto.Name);
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _roleService.DeleteAsync(id);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return NoContent();
    }
}
// Controllers/UsersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulseBanking.Application.Features.Users.Common;
using System.Threading.Tasks;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpGet("find-by-email")]
    public async Task<ActionResult<UserDto>> FindByEmail([FromQuery] string email)
    {
        var user = await _userService.FindByEmailAsync(email);
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpGet("find-by-name")]
    public async Task<ActionResult<UserDto>> FindByName([FromQuery] string userName)
    {
        var user = await _userService.FindByNameAsync(userName);
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto createUserDto)
    {
        var result = await _userService.CreateAsync(createUserDto);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var user = await _userService.FindByNameAsync(createUserDto.UserName);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto updateUserDto)
    {
        var result = await _userService.UpdateAsync(id, updateUserDto);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _userService.DeleteAsync(id);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return NoContent();
    }
}
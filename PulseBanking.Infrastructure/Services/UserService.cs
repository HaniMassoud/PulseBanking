// Services/UserService.cs
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using PulseBanking.Application.Features.Users.Common;

public class UserService : IUserService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMapper _mapper;

    public UserService(UserManager<IdentityUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<UserDto> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> FindByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> FindByNameAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<IdentityResult> CreateAsync(CreateUserDto createUserDto)
    {
        var user = _mapper.Map<IdentityUser>(createUserDto);
        var result = await _userManager.CreateAsync(user, createUserDto.Password);

        if (result.Succeeded && createUserDto.Roles != null)
        {
            await _userManager.AddToRolesAsync(user, createUserDto.Roles);
        }

        return result;
    }

    public async Task<IdentityResult> UpdateAsync(string id, UpdateUserDto updateUserDto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });

        _mapper.Map(updateUserDto, user);
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });

        return await _userManager.DeleteAsync(user);
    }
}

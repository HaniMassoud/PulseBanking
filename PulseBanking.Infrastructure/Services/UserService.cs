// Services/UserService.cs
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PulseBanking.Application.Features.Users.Common;
using PulseBanking.Domain.Entities;

public class UserService : IUserService
{
    private readonly UserManager<CustomIdentityUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(UserManager<CustomIdentityUser> userManager, IMapper mapper, ILogger<UserService> logger)
    {
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
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
        _logger.LogInformation("Creating user with the following details: " +
            "UserName: {UserName}, " +
            "Email: {Email}, " +
            "PhoneNumber: {PhoneNumber}, " +
            "Roles: {Roles}, " +
            "TenantId: {TenantId}",
            createUserDto.UserName,
            createUserDto.Email,
            createUserDto.PhoneNumber,
            string.Join(", ", createUserDto.Roles),
            createUserDto.TenantId);

        var user = _mapper.Map<CustomIdentityUser>(createUserDto);

        // Try setting TenantId using reflection
        typeof(CustomIdentityUser)
            .GetProperty("TenantId")
            ?.SetValue(user, createUserDto.TenantId);

        // Also try direct property setting as backup
        if (user.GetType().GetProperty("TenantId") != null)
        {
            user.TenantId = createUserDto.TenantId;
        }

        _logger.LogInformation("User mapped with TenantId: {TenantId}",
            user.GetType().GetProperty("TenantId")?.GetValue(user));

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

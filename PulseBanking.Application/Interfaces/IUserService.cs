// Services/Interfaces/IUserService.cs
using Microsoft.AspNetCore.Identity;
using PulseBanking.Application.Features.Users.Common;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(string id);
    Task<UserDto> FindByEmailAsync(string email);
    Task<UserDto> FindByNameAsync(string userName);
    Task<IdentityResult> CreateAsync(CreateUserDto createUserDto);
    Task<IdentityResult> UpdateAsync(string id, UpdateUserDto updateUserDto);
    Task<IdentityResult> DeleteAsync(string id);
}
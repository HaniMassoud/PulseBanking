using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using PulseBanking.Domain.Entities;

namespace PulseBanking.WebApp.Identity;

public class ApiUserStore : IUserStore<CustomIdentityUser>, IUserEmailStore<CustomIdentityUser>, IUserPasswordStore<CustomIdentityUser>
{
    private readonly HttpClient _httpClient;

    public ApiUserStore(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IdentityResult> CreateAsync(CustomIdentityUser user, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/users", user, cancellationToken);
        return await HandleResponseAsync(response);
    }

    public async Task<IdentityResult> DeleteAsync(CustomIdentityUser user, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync($"api/users/{user.Id}", cancellationToken);
        return await HandleResponseAsync(response);
    }

    public async Task<IdentityResult> UpdateAsync(CustomIdentityUser user, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/users/{user.Id}", user, cancellationToken);
        return await HandleResponseAsync(response);
    }

    public async Task<CustomIdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CustomIdentityUser>($"api/users/{userId}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<CustomIdentityUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CustomIdentityUser>($"api/users/email/{normalizedEmail}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public Task<string?> GetNormalizedUserNameAsync(CustomIdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<string?> GetUserNameAsync(CustomIdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetNormalizedUserNameAsync(CustomIdentityUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(CustomIdentityUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(CustomIdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(CustomIdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task<string?> GetNormalizedEmailAsync(CustomIdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetEmailAsync(CustomIdentityUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task SetEmailConfirmedAsync(CustomIdentityUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task SetNormalizedEmailAsync(CustomIdentityUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(CustomIdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(CustomIdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }

    public Task SetPasswordHashAsync(CustomIdentityUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // No-op
    }

    private async Task<IdentityResult> HandleResponseAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return IdentityResult.Success;
        }

        var errorResponse = await response.Content.ReadFromJsonAsync<IdentityErrorResponse>();
        var errors = errorResponse?.Errors?.Select(e => new IdentityError { Code = e.Code, Description = e.Description })
            ?? Array.Empty<IdentityError>();
        return IdentityResult.Failed(errors.ToArray());
    }

    public Task<string> GetUserIdAsync(CustomIdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id);
    }

    public async Task<CustomIdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CustomIdentityUser>($"api/users/name/{normalizedUserName}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private class IdentityErrorResponse
    {
        public required IEnumerable<IdentityErrorItem> Errors { get; set; }
    }

    private class IdentityErrorItem
    {
        public required string Code { get; set; }
        public required string Description { get; set; }
    }
}
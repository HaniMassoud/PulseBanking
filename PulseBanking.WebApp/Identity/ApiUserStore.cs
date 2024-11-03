using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;

namespace PulseBanking.WebApp.Identity;

public class ApiUserStore : IUserStore<IdentityUser>, IUserEmailStore<IdentityUser>, IUserPasswordStore<IdentityUser>
{
    private readonly HttpClient _httpClient;

    public ApiUserStore(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/users", user, cancellationToken);
        return await HandleResponseAsync(response);
    }

    public async Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/users/{user.Id}", user, cancellationToken);
        return await HandleResponseAsync(response);
    }

    public async Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync($"api/users/{user.Id}", cancellationToken);
        return await HandleResponseAsync(response);
    }

    public async Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await _httpClient.GetFromJsonAsync<IdentityUser>($"api/users/{userId}", cancellationToken);
    }

    public async Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return await _httpClient.GetFromJsonAsync<IdentityUser>($"api/users/name/{normalizedUserName}", cancellationToken);
    }

    public async Task<IdentityUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return await _httpClient.GetFromJsonAsync<IdentityUser>($"api/users/email/{normalizedEmail}", cancellationToken);
    }

    public Task<string> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id);
    }

    public Task<string> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string> GetEmailAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task<string> GetNormalizedEmailAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetEmailAsync(IdentityUser user, string email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task SetNormalizedEmailAsync(IdentityUser user, string normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    public Task<string> GetPasswordHashAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }

    public Task SetPasswordHashAsync(IdentityUser user, string passwordHash, CancellationToken cancellationToken)
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
        var errors = errorResponse?.Errors.Select(e => new IdentityError { Code = e.Code, Description = e.Description });
        return IdentityResult.Failed(errors.ToArray());
    }

    private class IdentityErrorResponse
    {
        public IEnumerable<IdentityErrorItem> Errors { get; set; }
    }

    private class IdentityErrorItem
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }
}
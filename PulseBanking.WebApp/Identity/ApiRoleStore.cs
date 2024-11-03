using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;

namespace PulseBanking.WebApp.Identity;

public class ApiRoleStore : IRoleStore<IdentityRole>
{
    private readonly HttpClient _httpClient;

    public ApiRoleStore(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/roles", role, cancellationToken);
        return await HandleResponseAsync(response);
    }

    public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/roles/{role.Id}", role, cancellationToken);
        return await HandleResponseAsync(response);
    }

    public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync($"api/roles/{role.Id}", cancellationToken);
        return await HandleResponseAsync(response);
    }

    public async Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        return await _httpClient.GetFromJsonAsync<IdentityRole>($"api/roles/{roleId}", cancellationToken);
    }

    public async Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        return await _httpClient.GetFromJsonAsync<IdentityRole>($"api/roles/name/{normalizedRoleName}", cancellationToken);
    }

    public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.NormalizedName);
    }

    public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id);
    }

    public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Name);
    }

    public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
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
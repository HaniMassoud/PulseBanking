﻿using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using PulseBanking.Domain.Entities;

namespace PulseBanking.WebApp.Identity;

public class ApiRoleStore : IRoleStore<CustomIdentityRole>
{
    private readonly HttpClient _httpClient;

    public ApiRoleStore(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IdentityResult> CreateAsync(CustomIdentityRole role, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/roles", role, cancellationToken);
        return await HandleResponseAsync(response);
    }

    public async Task<IdentityResult> UpdateAsync(CustomIdentityRole role, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/roles/{role.Id}", role, cancellationToken);
        return await HandleResponseAsync(response);
    }

    public async Task<IdentityResult> DeleteAsync(CustomIdentityRole role, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync($"api/roles/{role.Id}", cancellationToken);
        return await HandleResponseAsync(response);
    }

    public async Task<CustomIdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CustomIdentityRole>($"api/roles/{roleId}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<CustomIdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CustomIdentityRole>($"api/roles/name/{normalizedRoleName}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public Task<string?> GetNormalizedRoleNameAsync(CustomIdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.NormalizedName);
    }

    public Task<string> GetRoleIdAsync(CustomIdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id ?? string.Empty);
    }

    public Task<string?> GetRoleNameAsync(CustomIdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Name);
    }

    public Task SetNormalizedRoleNameAsync(CustomIdentityRole role, string? normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetRoleNameAsync(CustomIdentityRole role, string? roleName, CancellationToken cancellationToken)
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
        var errors = errorResponse?.Errors?.Select(e => new IdentityError { Code = e.Code, Description = e.Description })
            ?? Array.Empty<IdentityError>();
        return IdentityResult.Failed(errors.ToArray());
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
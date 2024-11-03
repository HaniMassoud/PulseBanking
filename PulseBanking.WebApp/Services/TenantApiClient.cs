using System.Net.Http.Json;
using PulseBanking.WebApp.Models;
using System.Text.Json;

namespace PulseBanking.WebApp.Services;

public class TenantApiClient : ITenantApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TenantApiClient> _logger;

    public TenantApiClient(HttpClient httpClient, ILogger<TenantApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task RegisterTenantAsync(TenantRegistrationModel model)
    {
        try
        {
            _logger.LogInformation("Attempting to register tenant: {@Model}", model);

            var response = await _httpClient.PostAsJsonAsync("api/Tenants/register", model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to register tenant. Status: {Status}, Error: {Error}",
                    response.StatusCode, error);

                var errorMessage = error;
                try
                {
                    var errorObj = JsonSerializer.Deserialize<JsonElement>(error);
                    if (errorObj.TryGetProperty("message", out var messageElement))
                    {
                        errorMessage = messageElement.GetString() ?? error;
                    }
                }
                catch (JsonException) { /* Use original error message */ }

                throw new HttpRequestException($"Failed to register tenant: {errorMessage}");
            }

            var result = await response.Content.ReadFromJsonAsync<object>();
            _logger.LogInformation("Successfully registered tenant {BankName}. Response: {@Result}",
                model.BankName, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering tenant");
            throw;
        }
    }
}
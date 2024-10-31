// Update src/PulseBanking.WebApp/Services/TenantService.cs
using System.Net.Http.Json;
using PulseBanking.WebApp.Models;
using System.Text.Json;

namespace PulseBanking.WebApp.Services;

public class TenantService : ITenantService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TenantService> _logger;

    public TenantService(HttpClient httpClient, ILogger<TenantService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task RegisterTenantAsync(TenantRegistrationModel model)
    {
        try
        {
            _logger.LogInformation("Attempting to register tenant: {@Model}", model);

            // Create the command object that matches what the API expects
            var command = new
            {
                BankName = model.BankName,
                TimeZone = model.TimeZone,
                CurrencyCode = model.CurrencyCode,
                DefaultTransactionLimit = model.DefaultTransactionLimit,
                AdminFirstName = model.AdminFirstName,
                AdminLastName = model.AdminLastName,
                AdminEmail = model.AdminEmail,
                AdminPassword = model.AdminPassword,
                AdminPhoneNumber = model.AdminPhoneNumber
            };

            _logger.LogInformation("Sending registration command to API: {@Command}", command);

            var response = await _httpClient.PostAsJsonAsync("api/Tenants/register", command);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to register tenant. Status: {Status}, Error: {Error}",
                    response.StatusCode, error);

                // Try to parse the error message to get more details
                try
                {
                    var errorObj = JsonSerializer.Deserialize<JsonElement>(error);
                    if (errorObj.TryGetProperty("message", out var messageElement))
                    {
                        throw new HttpRequestException($"Failed to register tenant: {messageElement}");
                    }
                }
                catch (JsonException)
                {
                    // If we can't parse the error, just use the raw error string
                }

                throw new HttpRequestException($"Failed to register tenant: {error}");
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
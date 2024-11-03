namespace PulseBanking.WebApp.Services;

public class IdentityApiClient : IIdentityApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IdentityApiClient> _logger;

    public IdentityApiClient(HttpClient httpClient, ILogger<IdentityApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> ValidateUserAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/users/validate", new { email, password });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user");
            return false;
        }
    }

    public async Task<bool> RegisterUserAsync(string email, string password, string firstName, string lastName)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/users", new
            {
                email,
                password,
                firstName,
                lastName
            });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user");
            return false;
        }
    }
}
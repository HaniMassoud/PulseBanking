namespace PulseBanking.WebApp.Services;

public interface IIdentityApiClient
{
    Task<bool> ValidateUserAsync(string email, string password);
    Task<bool> RegisterUserAsync(string email, string password, string firstName, string lastName);
    // Add other identity-related methods as needed
}
using Microsoft.AspNetCore.Identity;

namespace PulseBanking.Domain.Entities
{
    public class CustomIdentityUser : IdentityUser
    {
        public string TenantId { get; set; } = string.Empty;
    }
}
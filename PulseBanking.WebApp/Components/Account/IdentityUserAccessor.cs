using Microsoft.AspNetCore.Identity;
using PulseBanking.Domain.Entities;

namespace PulseBanking.WebApp.Components.Account
{
    internal sealed class IdentityUserAccessor(UserManager<CustomIdentityUser> userManager, IdentityRedirectManager redirectManager)
    {
        public async Task<CustomIdentityUser> GetRequiredUserAsync(HttpContext context)
        {
            var user = await userManager.GetUserAsync(context.User);

            if (user is null)
            {
                redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
            }

            return user;
        }
    }
}

// PulseBanking.Infrastructure/Security/TenantAwareAntiforgeryMiddleware.cs
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace PulseBanking.Infrastructure.Security;

public class TenantAwareAntiforgeryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAntiforgery _antiforgery;

    public TenantAwareAntiforgeryMiddleware(
        RequestDelegate next,
        IAntiforgery antiforgery)
    {
        _next = next;
        _antiforgery = antiforgery;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (HttpMethods.IsGet(context.Request.Method))
        {
            var tokens = _antiforgery.GetAndStoreTokens(context);
            context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
                new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });
        }
        else if (!HttpMethods.IsGet(context.Request.Method) &&
                 !HttpMethods.IsOptions(context.Request.Method) &&
                 !HttpMethods.IsHead(context.Request.Method))
        {
            try
            {
                await _antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
                return;
            }
        }

        await _next(context);
    }
}
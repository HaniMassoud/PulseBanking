using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using PulseBanking.WebApp.Components;
using PulseBanking.WebApp.Components.Account;
using PulseBanking.WebApp.Identity;
using PulseBanking.WebApp.Services;
using PulseBanking.Domain.Entities;
using PulseBanking.Infrastructure.Extensions;
using PulseBanking.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure HTTP clients
builder.Services.AddHttpClient<ITenantApiClient, TenantApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7060/");
});

builder.Services.AddHttpClient<IIdentityApiClient, IdentityApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7060/");
});

builder.Services.AddHttpClient("PulseBankingAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7060/");
});

// Authentication and Identity
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Identity stores
builder.Services.AddScoped<IUserStore<CustomIdentityUser>, ApiUserStore>();
builder.Services.AddScoped<IRoleStore<CustomIdentityRole>, ApiRoleStore>();

builder.Services.AddIdentityCore<CustomIdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
})
    .AddRoles<CustomIdentityRole>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

builder.Services.AddSingleton<IEmailSender<CustomIdentityUser>, IdentityNoOpEmailSender>();

// Add Antiforgery services
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "XSRF-TOKEN";
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

//app.UseMiddleware<TenantAwareAntiforgeryMiddleware>();

// Antiforgery middleware - now correctly placed after auth middleware
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
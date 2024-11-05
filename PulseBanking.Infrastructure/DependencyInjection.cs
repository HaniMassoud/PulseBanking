using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PulseBanking.Application.Interfaces;
using PulseBanking.Infrastructure.Persistence;
using PulseBanking.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PulseBanking.Application.Common.Interfaces;
using PulseBanking.Infrastructure.Persistence.Repositories;
using PulseBanking.Infrastructure.Persistence.UnitOfWork;
using PulseBanking.Infrastructure.Services.Events;
using PulseBanking.Infrastructure.Security;
using Microsoft.AspNetCore.Antiforgery;


namespace PulseBanking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Create and configure the options builder
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

        // Register base DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

        // Register Identity
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            // Configure identity options here
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Configure antiforgery
        services.AddAntiforgery(options =>
        {
            options.Cookie.Name = "XSRF-TOKEN";
            options.HeaderName = "X-XSRF-TOKEN";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        // Configure tenant-aware antiforgery
        services.Decorate<IAntiforgery, TenantAwareAntiforgeryService>();

        // Register services
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<ITenantManager, TenantManager>();
        services.AddScoped<ITenantService>(provider =>
        {
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            var tenantManager = provider.GetRequiredService<ITenantManager>();
            return new TenantService(httpContextAccessor, tenantManager);
        });
        services.AddScoped<ITenantValidator, TenantValidator>();

        // Register the options builder
        services.AddSingleton(optionsBuilder);

        // Register the factory
        services.AddScoped<ITenantDbContextFactory<ApplicationDbContext>, TenantDbContextFactory>(provider =>
        {
            var tenantManager = provider.GetRequiredService<ITenantManager>();
            var serviceProvider = provider.GetRequiredService<IServiceProvider>();
            return new TenantDbContextFactory(tenantManager, optionsBuilder, serviceProvider);
        });

        // Register DbContext using factory
        services.AddScoped<IApplicationDbContext>(provider =>
        {
            var factory = provider.GetRequiredService<ITenantDbContextFactory<ApplicationDbContext>>();
            var httpContext = provider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            var tenantId = httpContext?.Request.Headers["X-TenantId"].FirstOrDefault();
            return factory.CreateDbContext(tenantId!);
        });

        // Register application services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IDateTime, DateTimeService>();
        services.AddScoped<ICurrencyConverter, CurrencyConverter>();
        services.AddScoped<ITransactionProcessor, TransactionProcessor>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();

        // Add new service registrations
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEventDispatcher, EventDispatcher>();

        return services;
    }
}
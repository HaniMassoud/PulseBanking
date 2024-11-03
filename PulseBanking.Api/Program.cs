using Microsoft.OpenApi.Models;
using PulseBanking.Api.Filters;
using PulseBanking.Application;
using PulseBanking.Application.Features.Roles.Common;
using PulseBanking.Application.Features.Users.Common;
using PulseBanking.Infrastructure;
using PulseBanking.Infrastructure.Middleware;
using PulseBanking.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pulse Banking API", Version = "v1" });

    // Add support for the X-TenantId header
    c.AddSecurityDefinition("TenantId", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "X-TenantId",
        In = ParameterLocation.Header,
        Description = "Please enter your tenant ID"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "TenantId"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp",
        builder => builder
            //.WithOrigins("https://localhost:7261") // Your WebApp URL
            .WithOrigins("https://localhost:7199") // Your WebApp URL
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddAutoMapper(config =>
{
    // Option 1: Register profiles manually
    config.AddProfile<UserMappingProfile>();
    config.AddProfile<RoleMappingProfile>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Initialize database before tenant middleware
await app.InitializeDatabaseAsync();

// Add tenant middleware after database initialization
app.UseTenantMiddleware();

app.UseCors("AllowWebApp");

app.UseAuthorization();
app.MapControllers();

app.Run();

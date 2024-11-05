using Microsoft.AspNetCore.Http;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;

namespace PulseBanking.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantManager _tenantManager;
    private readonly string? _fixedTenantId;
    private Tenant? _currentTenant;

    public TenantService(
        IHttpContextAccessor httpContextAccessor,
        ITenantManager tenantManager,
        string? fixedTenantId = null)
    {
        _httpContextAccessor = httpContextAccessor;
        _tenantManager = tenantManager;
        _fixedTenantId = fixedTenantId;
    }

    public Tenant GetCurrentTenant()
    {
        if (_currentTenant != null)
            return _currentTenant;

        var tenantId = GetTenantId();
        var settings = _tenantManager.GetTenantAsync(tenantId).Result;

        if (settings == null)
            throw new UnauthorizedAccessException("Invalid tenant");

        _currentTenant = new Tenant
        {
            Id = settings.Id,
            Name = settings.Name,
            DeploymentType = settings.DeploymentType,
            Region = settings.Region,
            InstanceType = settings.InstanceType,
            ConnectionString = settings.ConnectionString,
            DedicatedHostName = settings.DedicatedHostName,
            CurrencyCode = settings.CurrencyCode,
            DefaultTransactionLimit = settings.DefaultTransactionLimit,
            TimeZone = settings.TimeZone,
            IsActive = settings.IsActive,
            CreatedAt = settings.CreatedAt,
            CreatedBy = settings.CreatedBy,
            LastModifiedAt = settings.LastModifiedAt,
            LastModifiedBy = settings.LastModifiedBy,
            TrialEndsAt = settings.TrialEndsAt,
            DataSovereigntyCompliant = settings.DataSovereigntyCompliant,
            RegulatoryNotes = settings.RegulatoryNotes
        };

        return _currentTenant;
    }

    private string GetTenantId()
    {
        if (!string.IsNullOrEmpty(_fixedTenantId))
            return _fixedTenantId;

        var tenantId = _httpContextAccessor.HttpContext?.Request.Headers["X-TenantId"].FirstOrDefault();

        if (string.IsNullOrEmpty(tenantId))
        {
            throw new UnauthorizedAccessException("TenantId not specified");
        }

        return tenantId;
    }
}
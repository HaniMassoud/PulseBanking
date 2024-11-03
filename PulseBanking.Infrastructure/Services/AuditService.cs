using Microsoft.Extensions.Logging;
using PulseBanking.Application.Common.Interfaces;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using PulseBanking.Infrastructure.Persistence;

namespace PulseBanking.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantService _tenantService;
    private readonly IDateTime _dateTime;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        ITenantService tenantService,
        IDateTime dateTime,
        ILogger<AuditService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _tenantService = tenantService;
        _dateTime = dateTime;
        _logger = logger;
    }

    public void AddAuditTrail(string action, string entityName, string entityId, string? details = null)
    {
        try
        {
            var tenantId = _tenantService.GetCurrentTenant();

            var audit = AuditTrail.Create(
                tenantId: tenantId,
                action: action,
                entityName: entityName,
                entityId: entityId,
                details: details,
                timestamp: _dateTime.UtcNow,
                userId: _currentUserService.UserId,
                userName: _currentUserService.UserName
            );

            _context.AuditTrails.Add(audit);

            _logger.LogInformation(
                "Audit trail created: {Action} on {EntityName} ({EntityId}) by {UserName}",
                action,
                entityName,
                entityId,
                _currentUserService.UserName ?? "System");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating audit trail for {Action} on {EntityName} ({EntityId})",
                action,
                entityName,
                entityId);
            throw;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
using MediatR;
using Microsoft.Extensions.Logging;
using PulseBanking.Application.Common.Interfaces;
using PulseBanking.Application.Interfaces;

namespace PulseBanking.Application.Common.Behaviors;

public class TransactionProcessingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TransactionProcessingBehavior<TRequest, TResponse>> _logger;
    private readonly ITenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IDateTime _dateTime;

    public TransactionProcessingBehavior(
        ILogger<TransactionProcessingBehavior<TRequest, TResponse>> logger,
        ITenantService tenantService,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IDateTime dateTime)
    {
        _logger = logger;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _dateTime = dateTime;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenant();

        if (request is ITransactionRequest transactionRequest)
        {
            _logger.LogInformation(
                "Processing transaction request. Type: {RequestType}, User: {User}, Tenant: {Tenant}",
                typeof(TRequest).Name,
                _currentUserService.UserName,
                tenantId);

            // Pre-processing audit
            _auditService.AddAuditTrail(
                "TransactionRequest",
                typeof(TRequest).Name,
                transactionRequest.GetTransactionId().ToString(),
                $"Request initiated by {_currentUserService.UserName}");
        }

        try
        {
            // Process the request
            var response = await next();

            // Post-processing audit for successful transactions
            if (request is ITransactionRequest)
            {
                _auditService.AddAuditTrail(
                    "TransactionComplete",
                    typeof(TRequest).Name,
                    "Success",
                    $"Completed by {_currentUserService.UserName}");

                await _auditService.SaveChangesAsync(cancellationToken);
            }

            return response;
        }
        catch (Exception ex)
        {
            if (request is ITransactionRequest transactionRequest2)
            {
                _logger.LogError(ex,
                    "Error processing transaction request. Type: {RequestType}, User: {User}, Tenant: {Tenant}",
                    typeof(TRequest).Name,
                    _currentUserService.UserName,
                    tenantId);

                _auditService.AddAuditTrail(
                    "TransactionError",
                    typeof(TRequest).Name,
                    transactionRequest2.GetTransactionId().ToString(),
                    $"Error: {ex.Message}");

                await _auditService.SaveChangesAsync(cancellationToken);
            }

            throw;
        }
    }
}


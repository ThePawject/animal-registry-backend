using AnimalRegistry.Modules.Audit.Application.Services;
using AnimalRegistry.Shared.Auditing;
using AnimalRegistry.Shared.MediatorPattern;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AnimalRegistry.Modules.Audit.Infrastructure.Interceptors;

/// <summary>
///     Decorator for IMediator that audits commands marked with [Auditable]
/// </summary>
public class AuditCommandInterceptor(
    IMediator innerMediator,
    IAuditService auditService,
    ILogger<AuditCommandInterceptor> logger)
    : IMediator
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        if (!ShouldAudit(request))
        {
            return await innerMediator.Send(request, cancellationToken);
        }

        var stopwatch = Stopwatch.StartNew();

        TResponse response;
        try
        {
            response = await innerMediator.Send(request, cancellationToken);
        }
        catch (Exception)
        {
            stopwatch.Stop();

            try
            {
                await auditService.AuditCommandAsync(
                    request,
                    default!,
                    stopwatch.Elapsed,
                    cancellationToken);
            }
            catch (Exception auditEx)
            {
                logger.LogError(auditEx, "Failed to audit command failure for {CommandType}", request.GetType().Name);
            }

            throw; // Re-throw original exception
        }

        stopwatch.Stop();

        try
        {
            await auditService.AuditCommandAsync(
                request,
                response,
                stopwatch.Elapsed,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to audit command {CommandType}", request.GetType().Name);
        }

        return response;
    }

    private static bool ShouldAudit<TResponse>(IRequest<TResponse> request)
    {
        var requestType = request.GetType();
        return Attribute.IsDefined(requestType, typeof(AuditableAttribute));
    }
}
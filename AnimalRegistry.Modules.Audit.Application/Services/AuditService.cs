using AnimalRegistry.Modules.Audit.Application.CreateAuditEntry;
using AnimalRegistry.Modules.Audit.Domain.AuditEntries;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.DDD;
using AnimalRegistry.Shared.MediatorPattern;
using System.Text.Json;

namespace AnimalRegistry.Modules.Audit.Application.Services;

public sealed class AuditService(
    IMediator mediator,
    IAuditMetadataProvider metadataProvider)
    : IAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false, PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task AuditDomainEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var metadata = CreateMetadata();
        var entityType = domainEvent.GetType().FullName ?? domainEvent.GetType().Name;
        var entityData = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions);

        var command = new CreateAuditEntryCommand(
            AuditEntryType.DomainEvent,
            entityType,
            entityData,
            metadata);

        await mediator.Send(command, cancellationToken);
    }

    public async Task AuditCommandAsync<TResponse>(
        IRequest<TResponse> command,
        TResponse result,
        TimeSpan executionTime,
        CancellationToken cancellationToken = default)
    {
        var metadata = CreateMetadata();
        var entityType = command.GetType().FullName ?? command.GetType().Name;
        var entityData = JsonSerializer.Serialize(command, command.GetType(), JsonOptions);

        var isSuccess = true;
        string? errorMessage = null;

        if (result != null)
        {
            var resultType = result.GetType();
            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var isSuccessProperty = resultType.GetProperty("IsSuccess");
                var errorProperty = resultType.GetProperty("Error");

                isSuccess = (bool)(isSuccessProperty?.GetValue(result) ?? true);
                errorMessage = errorProperty?.GetValue(result)?.ToString();
            }
        }

        var auditCommand = new CreateAuditEntryCommand(
            AuditEntryType.Command,
            entityType,
            entityData,
            metadata,
            executionTime,
            isSuccess,
            errorMessage);

        await mediator.Send(auditCommand, cancellationToken);
    }

    private AuditMetadata CreateMetadata()
    {
        return metadataProvider.GetMetadata();
    }
}
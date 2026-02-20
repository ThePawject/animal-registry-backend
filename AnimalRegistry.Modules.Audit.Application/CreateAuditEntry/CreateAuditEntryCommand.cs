using AnimalRegistry.Modules.Audit.Domain.AuditEntries;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Audit.Application.CreateAuditEntry;

public record CreateAuditEntryCommand(
    AuditEntryType Type,
    string EntityType,
    string EntityData,
    AuditMetadata Metadata,
    TimeSpan? ExecutionTime = null,
    bool IsSuccess = true,
    string? ErrorMessage = null
) : IRequest<Result<Guid>>;
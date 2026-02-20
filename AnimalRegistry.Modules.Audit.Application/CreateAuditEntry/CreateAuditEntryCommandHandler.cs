using AnimalRegistry.Modules.Audit.Domain.AuditEntries;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Audit.Application.CreateAuditEntry;

internal sealed class CreateAuditEntryCommandHandler(IAuditEntryRepository repository)
    : IRequestHandler<CreateAuditEntryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAuditEntryCommand request, CancellationToken cancellationToken)
    {
        var auditEntry = request.Type == AuditEntryType.DomainEvent
            ? AuditEntry.CreateForDomainEvent(
                request.EntityType,
                request.EntityData,
                request.Metadata)
            : AuditEntry.CreateForCommand(
                request.EntityType,
                request.EntityData,
                request.Metadata,
                request.ExecutionTime ?? TimeSpan.Zero,
                request.IsSuccess,
                request.ErrorMessage);

        await repository.AddAsync(auditEntry, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(auditEntry.Id);
    }
}
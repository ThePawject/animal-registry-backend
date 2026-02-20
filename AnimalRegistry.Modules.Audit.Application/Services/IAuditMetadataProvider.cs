using AnimalRegistry.Modules.Audit.Domain.AuditEntries;

namespace AnimalRegistry.Modules.Audit.Application.Services;

public interface IAuditMetadataProvider
{
    AuditMetadata GetMetadata();
}
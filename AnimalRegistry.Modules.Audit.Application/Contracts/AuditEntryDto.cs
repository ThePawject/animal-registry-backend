using AnimalRegistry.Modules.Audit.Domain.AuditEntries;

namespace AnimalRegistry.Modules.Audit.Application.Contracts;

public record AuditEntryDto(
    Guid Id,
    AuditEntryType Type,
    string EntityType,
    string EntityData,
    AuditMetadataDto Metadata,
    DateTime Timestamp,
    TimeSpan? ExecutionTime,
    bool IsSuccess,
    string? ErrorMessage);

public record AuditMetadataDto(
    string UserId,
    string Email,
    string ShelterId,
    string? IpAddress,
    string? UserAgent);
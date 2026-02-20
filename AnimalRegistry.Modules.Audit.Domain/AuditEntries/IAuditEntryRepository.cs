namespace AnimalRegistry.Modules.Audit.Domain.AuditEntries;

public interface IAuditEntryRepository
{
    Task AddAsync(AuditEntry auditEntry, CancellationToken cancellationToken = default);

    Task<List<AuditEntry>> GetAllAsync(
        int page,
        int pageSize,
        AuditEntryType? type = null,
        string? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        AuditEntryType? type = null,
        string? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
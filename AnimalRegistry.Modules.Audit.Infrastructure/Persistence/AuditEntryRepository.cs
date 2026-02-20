using AnimalRegistry.Modules.Audit.Domain.AuditEntries;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Audit.Infrastructure.Persistence;

public sealed class AuditEntryRepository(AuditDbContext context) : IAuditEntryRepository
{
    public async Task AddAsync(AuditEntry auditEntry, CancellationToken cancellationToken = default)
    {
        await context.AuditEntries.AddAsync(auditEntry, cancellationToken);
    }

    public async Task<List<AuditEntry>> GetAllAsync(
        int page,
        int pageSize,
        AuditEntryType? type = null,
        string? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.AuditEntries.AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(x => x.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(x => x.Metadata.UserId == userId);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.Timestamp <= toDate.Value);
        }

        return await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        AuditEntryType? type = null,
        string? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.AuditEntries.AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(x => x.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(x => x.Metadata.UserId == userId);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.Timestamp <= toDate.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
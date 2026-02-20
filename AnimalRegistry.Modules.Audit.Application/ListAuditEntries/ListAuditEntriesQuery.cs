using AnimalRegistry.Modules.Audit.Application.Contracts;
using AnimalRegistry.Modules.Audit.Domain.AuditEntries;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.Pagination;

namespace AnimalRegistry.Modules.Audit.Application.ListAuditEntries;

public record ListAuditEntriesQuery(
    int Page,
    int PageSize,
    AuditEntryType? Type = null,
    string? UserId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<Result<PagedResult<AuditEntryDto>>>;
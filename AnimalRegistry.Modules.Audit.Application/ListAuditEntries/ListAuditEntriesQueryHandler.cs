using AnimalRegistry.Modules.Audit.Application.Contracts;
using AnimalRegistry.Modules.Audit.Domain.AuditEntries;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.Pagination;

namespace AnimalRegistry.Modules.Audit.Application.ListAuditEntries;

internal sealed class ListAuditEntriesQueryHandler(IAuditEntryRepository repository)
    : IRequestHandler<ListAuditEntriesQuery, Result<PagedResult<AuditEntryDto>>>
{
    public async Task<Result<PagedResult<AuditEntryDto>>> Handle(
        ListAuditEntriesQuery request,
        CancellationToken cancellationToken)
    {
        var entries = await repository.GetAllAsync(
            request.Page,
            request.PageSize,
            request.Type,
            request.UserId,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        var totalCount = await repository.CountAsync(
            request.Type,
            request.UserId,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        var dtos = entries.Select(e => new AuditEntryDto(
            e.Id,
            e.Type,
            e.EntityType,
            e.EntityData,
            new AuditMetadataDto(
                e.Metadata.UserId,
                e.Metadata.Email,
                e.Metadata.ShelterId,
                e.Metadata.IpAddress,
                e.Metadata.UserAgent),
            e.Timestamp,
            e.ExecutionTime,
            e.IsSuccess,
            e.ErrorMessage)).ToList();

        var pagedResult = new PagedResult<AuditEntryDto>(
            dtos,
            totalCount,
            request.Page,
            request.PageSize);

        return Result<PagedResult<AuditEntryDto>>.Success(pagedResult);
    }
}
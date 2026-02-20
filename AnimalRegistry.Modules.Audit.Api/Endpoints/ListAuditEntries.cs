using AnimalRegistry.Modules.Audit.Application.Contracts;
using AnimalRegistry.Modules.Audit.Application.ListAuditEntries;
using AnimalRegistry.Modules.Audit.Domain.AuditEntries;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.Pagination;
using FastEndpoints;

namespace AnimalRegistry.Modules.Audit.Api.Endpoints;

internal sealed class ListAuditEntries(IMediator mediator)
    : Endpoint<ListAuditEntriesRequest, PagedResult<AuditEntryDto>>
{
    public override void Configure()
    {
        Get("/api/audit");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ListAuditEntriesRequest req, CancellationToken ct)
    {
        var query = new ListAuditEntriesQuery(
            req.Page,
            req.PageSize,
            req.Type,
            req.UserId,
            req.FromDate,
            req.ToDate);

        var result = await mediator.Send(query, ct);
        await this.SendResultAsync(result, ct);
    }
}

public class ListAuditEntriesRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public AuditEntryType? Type { get; set; }
    public string? UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
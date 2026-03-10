using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.Pagination;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class ListAnimals(IMediator mediator) : Endpoint<ListAnimalsRequest, PagedResult<AnimalListItemDto>>
{
    public override void Configure()
    {
        Get(ListAnimalsRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
    }

    public override async Task HandleAsync(ListAnimalsRequest req, CancellationToken ct)
    {
        var keyWordSearch = string.IsNullOrWhiteSpace(req.KeyWordSearch)
            ? null
            : req.KeyWordSearch.Trim();

        var query = new ListAnimalsQuery(req.Page, req.PageSize, keyWordSearch);
        var result = await mediator.Send(query, ct);
        await this.SendResultAsync(result, ct);
    }
}
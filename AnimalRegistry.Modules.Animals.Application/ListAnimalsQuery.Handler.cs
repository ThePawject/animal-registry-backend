using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.CurrentUser;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.Pagination;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed class ListAnimalsQueryHandler(
    IAnimalRepository animalRepository,
    ICurrentUser currentUser)
    : IRequestHandler<ListAnimalsQuery, Result<PagedResult<AnimalListItemDto>>>
{
    public async Task<Result<PagedResult<AnimalListItemDto>>> Handle(ListAnimalsQuery request,
        CancellationToken cancellationToken)
    {
        var pagedAnimals = await animalRepository.ListAsync(
            currentUser.ShelterId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = pagedAnimals.Items.Select(AnimalListItemDto.FromDomain).ToList();
        var result = new PagedResult<AnimalListItemDto>(
            items,
            pagedAnimals.TotalCount,
            pagedAnimals.Page,
            pagedAnimals.PageSize);

        return Result<PagedResult<AnimalListItemDto>>.Success(result);
    }
}

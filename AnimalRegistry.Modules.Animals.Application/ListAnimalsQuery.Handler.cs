using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.CurrentUser;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.Pagination;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed class ListAnimalsQueryHandler(
    IAnimalRepository animalRepository,
    ICurrentUser currentUser)
    : IRequestHandler<ListAnimalsQuery, Result<PagedResult<AnimalDto>>>
{
    public async Task<Result<PagedResult<AnimalDto>>> Handle(ListAnimalsQuery request,
        CancellationToken cancellationToken)
    {
        var pagedAnimals = await animalRepository.ListAsync(
            currentUser.ShelterId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = pagedAnimals.Items.Select(AnimalDto.FromDomain).ToList();
        var result = new PagedResult<AnimalDto>(
            items,
            pagedAnimals.TotalCount,
            pagedAnimals.Page,
            pagedAnimals.PageSize);

        return Result<PagedResult<AnimalDto>>.Success(result);
    }
}
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.CurrentUser;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed class ListAnimalsQueryHandler(
    IAnimalRepository animalRepository,
    ICurrentUser currentUser)
    : IRequestHandler<ListAnimalsQuery, Result<IEnumerable<AnimalDto>>>
{
    public async Task<Result<IEnumerable<AnimalDto>>> Handle(ListAnimalsQuery request, CancellationToken cancellationToken)
    {
        var animals = await animalRepository.ListAsync(currentUser.ShelterId, cancellationToken);
        var list = animals.Select(AnimalDto.FromDomain).ToList();

        return Result<IEnumerable<AnimalDto>>.Success(list);
    }
}

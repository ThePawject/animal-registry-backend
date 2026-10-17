using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed class GetAnimalQueryHandler(
    IAnimalRepository animalRepository,
    ICurrentUser currentUser,
    IAnimalOriginReader originReader)
    : IRequestHandler<GetAnimalQuery, Result<AnimalDto>>
{
    public async Task<Result<AnimalDto>> Handle(GetAnimalQuery request, CancellationToken cancellationToken)
    {
        var animal = await animalRepository.GetByIdAsync(request.Id, currentUser.ShelterId, cancellationToken);
        if (animal is null)
        {
            return Result<AnimalDto>.NotFound();
        }

        var originLookup = await originReader.LoadAsync([animal], cancellationToken);
        var resp = AnimalDto.FromDomain(animal, originLookup?.Resolve(animal));

        return Result<AnimalDto>.Success(resp);
    }
}
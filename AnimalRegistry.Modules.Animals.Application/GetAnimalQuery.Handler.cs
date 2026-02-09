using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.CurrentUser;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed class GetAnimalQueryHandler(
    IAnimalRepository animalRepository,
    ICurrentUser currentUser)
    : IRequestHandler<GetAnimalQuery, Result<AnimalDto>>
{
    public async Task<Result<AnimalDto>> Handle(GetAnimalQuery request, CancellationToken cancellationToken)
    {
        var animal = await animalRepository.GetByIdAsync(request.Id, currentUser.ShelterId, cancellationToken);
        if (animal is null)
        {
            return Result<AnimalDto>.NotFound();
        }

        var resp = AnimalDto.FromDomain(animal);

        return Result<AnimalDto>.Success(resp);
    }
}

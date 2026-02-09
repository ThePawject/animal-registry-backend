using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.AnimalHealth;

internal sealed class CreateAnimalHealthCommandHandler(
    IAnimalRepository animalRepository,
    ICurrentUser currentUser
) : IRequestHandler<CreateAnimalHealthCommand, Result>
{
    public async Task<Result> Handle(CreateAnimalHealthCommand request, CancellationToken cancellationToken)
    {
        var animal = await animalRepository.GetByIdAsync(request.AnimalId, currentUser.ShelterId, cancellationToken);
        if (animal is null)
        {
            return Result.NotFound("Animal not found.");
        }

        animal.AddHealthRecord(request.OccurredOn, request.Description, currentUser.Email);

        await animalRepository.UpdateAsync(animal, cancellationToken);

        return Result.Success();
    }
}
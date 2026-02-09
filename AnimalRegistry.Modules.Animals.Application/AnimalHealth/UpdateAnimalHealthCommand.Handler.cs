using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.AnimalHealth;

internal sealed class UpdateAnimalHealthCommandHandler(
    IAnimalRepository animalRepository,
    ICurrentUser currentUser
) : IRequestHandler<UpdateAnimalHealthCommand, Result>
{
    public async Task<Result> Handle(UpdateAnimalHealthCommand request, CancellationToken cancellationToken)
    {
        var animal = await animalRepository.GetByIdAsync(request.AnimalId, currentUser.ShelterId, cancellationToken);
        if (animal is null)
        {
            return Result.NotFound("Animal not found.");
        }

        animal.UpdateHealthRecord(request.HealthRecordId, request.OccurredOn, request.Description);

        await animalRepository.UpdateAsync(animal, cancellationToken);

        return Result.Success();
    }
}
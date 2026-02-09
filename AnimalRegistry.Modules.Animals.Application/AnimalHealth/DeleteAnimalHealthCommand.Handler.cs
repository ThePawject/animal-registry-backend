using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.AnimalHealth;

internal sealed class DeleteAnimalHealthCommandHandler(
    IAnimalRepository animalRepository,
    ICurrentUser currentUser
) : IRequestHandler<DeleteAnimalHealthCommand, Result>
{
    public async Task<Result> Handle(DeleteAnimalHealthCommand request, CancellationToken cancellationToken)
    {
        var animal = await animalRepository.GetByIdAsync(request.AnimalId, currentUser.ShelterId, cancellationToken);
        if (animal is null)
        {
            return Result.NotFound("Animal not found.");
        }

        animal.RemoveHealthRecord(request.HealthRecordId);

        await animalRepository.UpdateAsync(animal, cancellationToken);

        return Result.Success();
    }
}
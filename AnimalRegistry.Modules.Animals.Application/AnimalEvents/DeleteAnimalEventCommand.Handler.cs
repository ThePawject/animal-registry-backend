using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.CurrentUser;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.AnimalEvents;

internal sealed class DeleteAnimalEventCommandHandler(
    IAnimalRepository animalRepository,
    ICurrentUser currentUser) : IRequestHandler<DeleteAnimalEventCommand, Result>
{
    public async Task<Result> Handle(DeleteAnimalEventCommand request, CancellationToken cancellationToken)
    {
        var animal = await animalRepository.GetByIdAsync(request.AnimalId, currentUser.ShelterId, cancellationToken);
        if (animal is null)
        {
            return Result.NotFound("Animal not found.");
        }

        animal.RemoveEvent(request.EventId);

        await animalRepository.UpdateAsync(animal, cancellationToken);

        return Result.Success();
    }
}
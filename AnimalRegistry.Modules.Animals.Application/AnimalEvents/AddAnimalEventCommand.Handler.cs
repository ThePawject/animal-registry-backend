using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.CurrentUser;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.AnimalEvents;

internal sealed class AddAnimalEventCommandHandler(
    IAnimalRepository animalRepository,
    ICurrentUser currentUser) : IRequestHandler<AddAnimalEventCommand, Result>
{
    public async Task<Result> Handle(AddAnimalEventCommand request, CancellationToken cancellationToken)
    {
        var animal = await animalRepository.GetByIdAsync(request.AnimalId, currentUser.ShelterId, cancellationToken);
        if (animal is null)
        {
            return Result.NotFound("Animal not found.");
        }

        animal.AddEvent(request.Type, request.OccurredOn, request.Description, request.PerformedBy);

        await animalRepository.UpdateAsync(animal, cancellationToken);

        return Result.Success();
    }
}
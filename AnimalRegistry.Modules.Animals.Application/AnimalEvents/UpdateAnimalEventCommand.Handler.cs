using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.CurrentUser;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.AnimalEvents;

internal sealed class UpdateAnimalEventCommandHandler(
    IAnimalRepository animalRepository,
    ICurrentUser currentUser) : IRequestHandler<UpdateAnimalEventCommand, Result>
{
    public async Task<Result> Handle(UpdateAnimalEventCommand request, CancellationToken cancellationToken)
    {
        var animal = await animalRepository.GetByIdAsync(request.AnimalId, currentUser.ShelterId, cancellationToken);
        if (animal is null)
        {
            return Result.NotFound("Animal not found.");
        }

        animal.UpdateEvent(request.EventId, request.Type, request.OccurredOn, request.Description, request.PerformedBy);

        await animalRepository.UpdateAsync(animal, cancellationToken);

        return Result.Success();
    }
}
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.AnimalEvents;

internal sealed class CreateAnimalEventCommandHandler(
    IAnimalRepository animalRepository,
    ICurrentUser currentUser
    ) : IRequestHandler<CreateAnimalEventCommand, Result>
{
    public async Task<Result> Handle(CreateAnimalEventCommand request, CancellationToken cancellationToken)
    {
        var animal = await animalRepository.GetByIdAsync(request.AnimalId, currentUser.ShelterId, cancellationToken);
        if (animal is null)
        {
            return Result.NotFound("Animal not found.");
        }

        animal.AddEvent(request.Type, request.OccurredOn, request.Description, currentUser.Email);

        await animalRepository.UpdateAsync(animal, cancellationToken);

        return Result.Success();
    }
}
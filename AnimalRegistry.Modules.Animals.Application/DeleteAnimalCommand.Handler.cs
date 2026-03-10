using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed class DeleteAnimalCommandHandler(
    IAnimalRepository animalRepository,
    ICurrentUser currentUser) : IRequestHandler<DeleteAnimalCommand, Result>
{
    public async Task<Result> Handle(DeleteAnimalCommand request, CancellationToken cancellationToken)
    {
        var animal = await animalRepository.GetByIdAsync(request.Id, currentUser.ShelterId, cancellationToken);
        if (animal is null)
        {
            return Result.NotFound();
        }

        await animalRepository.RemoveAsync(animal, cancellationToken);
        return Result.Success();
    }
}
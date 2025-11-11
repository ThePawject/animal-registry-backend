using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed class CreateAnimalCommandHandler(IAnimalRepository animalRepository)
    : IRequestHandler<CreateAnimalCommand, CreateAnimalCommandResponse>
{
    public async Task<CreateAnimalCommandResponse> Handle(CreateAnimalCommand request,
        CancellationToken cancellationToken)
    {
        var animal = Animal.Create(
            request.Signature,
            request.TransponderCode,
            request.Name,
            request.Color,
            request.Species,
            request.Sex,
            request.BirthDate
        );
        await animalRepository.AddAsync(animal, cancellationToken);
        return new CreateAnimalCommandResponse();
    }
}
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

public sealed class CreateAnimalCommandHandler : IRequestHandler<CreateAnimalCommand, CreateAnimalCommandResponse>
{
    private readonly IAnimalRepository _animalRepository;

    public CreateAnimalCommandHandler(IAnimalRepository animalRepository)
    {
        _animalRepository = animalRepository;
    }

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
        await _animalRepository.AddAsync(animal, cancellationToken);
        return new CreateAnimalCommandResponse();
    }
}
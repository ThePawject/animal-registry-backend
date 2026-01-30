using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

public class CreateAnimalCommandHandler(IAnimalsRepository animalsRepository)
    : IRequestHandler<CreateAnimalCommand, Result<CreateAnimalCommandResponse>>
{
    public async Task<Result<CreateAnimalCommandResponse>> Handle(CreateAnimalCommand request,
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

        var result = await animalsRepository.AddAsync(animal);
        return Result<CreateAnimalCommandResponse>.Success(new CreateAnimalCommandResponse(result.Id));
    }
}
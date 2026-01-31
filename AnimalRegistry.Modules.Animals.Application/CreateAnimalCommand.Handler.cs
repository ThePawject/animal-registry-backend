using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed class CreateAnimalCommandHandler(IAnimalRepository animalRepository)
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

        var result = await animalRepository.AddAsync(animal, cancellationToken);
        if (!result.IsSuccess || result.Value == null)
        {
            return Result<CreateAnimalCommandResponse>.Failure("Failed to create animal.");
        }

        return Result<CreateAnimalCommandResponse>.Success(new CreateAnimalCommandResponse(result.Value.Id));
    }
}

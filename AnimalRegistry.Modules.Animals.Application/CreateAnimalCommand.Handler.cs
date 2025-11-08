using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

public class CreateAnimalCommandHandler : IRequestHandler<CreateAnimalCommand, CreateAnimalCommandResponse>
{
    public Task<CreateAnimalCommandResponse> Handle(CreateAnimalCommand request,
        CancellationToken cancellationToken)
    {
        var animal = Animal.Create(
            request.Signature,
            request.TransponderCode,
            request.Name,
            request.Color,
            request.DictItemSpeciesId,
            request.DictItemSexId,
            request.BirthDate
        );
        return Task.FromResult(new CreateAnimalCommandResponse());
    }
}
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

public class CreateAnimalCommandHandler: IRequestHandler<CreateAnimalCommand, CreateAnimalCommandResponse>
{
    public Task<CreateAnimalCommandResponse> Handle(CreateAnimalCommand request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new CreateAnimalCommandResponse());
    }
}
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

public sealed class CreateAnimalCommand: IRequest<CreateAnimalCommandResponse>;
public sealed class CreateAnimalCommandResponse;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

public sealed class GetAllAnimalsQuery : IRequest<Result<GetAllAnimalsQueryResponse>>;

public sealed record GetAllAnimalsQueryResponse(List<Animal> Animals);
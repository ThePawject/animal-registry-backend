using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Shelters.Application;

internal sealed record GetShelterFeaturesQuery : IRequest<Result<IReadOnlyList<ShelterFeatureDto>>>;

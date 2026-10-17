using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Shelters.Application;

internal sealed record SetShelterFeatureStateCommand(string FeatureKey, bool IsEnabled)
    : IRequest<Result<ShelterFeatureDto>>;

using AnimalRegistry.Modules.Shelters.Domain;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;

namespace AnimalRegistry.Modules.Shelters.Application;

internal sealed class SetShelterFeatureStateCommandHandler(
    IEnumerable<IShelterFeature> features,
    IShelterFeatureActivationRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<SetShelterFeatureStateCommand, Result<ShelterFeatureDto>>
{
    public async Task<Result<ShelterFeatureDto>> Handle(SetShelterFeatureStateCommand request,
        CancellationToken cancellationToken)
    {
        var feature = features.FirstOrDefault(f => f.Key == request.FeatureKey);
        if (feature is null)
        {
            return Result<ShelterFeatureDto>.NotFound($"Module '{request.FeatureKey}' does not exist.");
        }

        var activations = await repository.GetByShelterIdAsync(currentUser.ShelterId, cancellationToken);
        var activation = activations.FirstOrDefault(a => a.FeatureKey == feature.Key);

        if (request.IsEnabled && activation is null)
        {
            await repository.AddAsync(
                ShelterFeatureActivation.Create(currentUser.ShelterId, feature.Key, currentUser.UserId),
                cancellationToken);
        }
        else if (!request.IsEnabled && activation is not null)
        {
            await repository.RemoveAsync(activation, cancellationToken);
        }

        return Result<ShelterFeatureDto>.Success(ShelterFeatureDto.From(feature, request.IsEnabled));
    }
}

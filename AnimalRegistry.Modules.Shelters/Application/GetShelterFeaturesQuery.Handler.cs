using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;

namespace AnimalRegistry.Modules.Shelters.Application;

internal sealed class GetShelterFeaturesQueryHandler(
    IEnumerable<IShelterFeature> features,
    IShelterFeatureToggles toggles)
    : IRequestHandler<GetShelterFeaturesQuery, Result<IReadOnlyList<ShelterFeatureDto>>>
{
    public async Task<Result<IReadOnlyList<ShelterFeatureDto>>> Handle(GetShelterFeaturesQuery request,
        CancellationToken cancellationToken)
    {
        var enabled = await toggles.GetEnabledAsync(cancellationToken);

        IReadOnlyList<ShelterFeatureDto> dtos = features
            .OrderBy(f => f.Name)
            .Select(f => ShelterFeatureDto.From(f, enabled.Contains(f.Key)))
            .ToList();

        return Result<IReadOnlyList<ShelterFeatureDto>>.Success(dtos);
    }
}

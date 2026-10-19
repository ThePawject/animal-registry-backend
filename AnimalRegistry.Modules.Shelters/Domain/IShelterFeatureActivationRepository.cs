namespace AnimalRegistry.Modules.Shelters.Domain;

internal interface IShelterFeatureActivationRepository
{
    Task<IReadOnlyList<ShelterFeatureActivation>> GetByShelterIdAsync(string shelterId,
        CancellationToken cancellationToken = default);

    Task AddAsync(ShelterFeatureActivation activation, CancellationToken cancellationToken = default);

    Task RemoveAsync(ShelterFeatureActivation activation, CancellationToken cancellationToken = default);
}

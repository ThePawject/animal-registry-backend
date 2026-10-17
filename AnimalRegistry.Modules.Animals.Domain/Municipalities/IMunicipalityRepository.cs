namespace AnimalRegistry.Modules.Animals.Domain.Municipalities;

internal interface IMunicipalityRepository
{
    Task<Municipality?> GetByIdAsync(Guid id, string shelterId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Municipality>> ListAsync(string shelterId, bool includeArchived,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Municipality>> GetByIdsAsync(IEnumerable<Guid> ids, string shelterId,
        CancellationToken cancellationToken = default);

    Task<bool> IsNameUniqueAsync(string name, string shelterId, Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<bool> AddAsync(Municipality municipality, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Municipality municipality, CancellationToken cancellationToken = default);
}

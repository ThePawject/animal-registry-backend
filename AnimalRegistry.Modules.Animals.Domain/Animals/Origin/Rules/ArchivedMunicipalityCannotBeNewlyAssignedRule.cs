using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.Origin.Rules;

internal sealed class ArchivedMunicipalityCannotBeNewlyAssignedRule(
    Guid municipalityId,
    string role,
    IReadOnlyCollection<Municipality> municipalities,
    IReadOnlyCollection<Guid> assignedMunicipalityIds) : IBusinessRule
{
    private readonly Municipality? _municipality = municipalities.FirstOrDefault(m => m.Id == municipalityId);

    public string Message => $"The {role} municipality '{_municipality?.Name}' is archived.";

    public bool IsBroken()
    {
        return _municipality is { IsArchived: true } && !assignedMunicipalityIds.Contains(municipalityId);
    }
}

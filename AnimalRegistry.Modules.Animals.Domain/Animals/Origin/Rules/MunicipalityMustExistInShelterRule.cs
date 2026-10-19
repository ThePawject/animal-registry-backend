using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.Origin.Rules;

internal sealed class MunicipalityMustExistInShelterRule(
    Guid municipalityId,
    string role,
    string shelterId,
    IReadOnlyCollection<Municipality> municipalities) : IBusinessRule
{
    public string Message => $"The {role} municipality '{municipalityId}' does not exist.";

    public bool IsBroken()
    {
        return !municipalities.Any(m => m.Id == municipalityId && m.ShelterId == shelterId);
    }
}

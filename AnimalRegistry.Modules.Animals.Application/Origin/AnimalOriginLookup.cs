using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;

namespace AnimalRegistry.Modules.Animals.Application.Origin;

public sealed class AnimalOriginLookup(
    IReadOnlyDictionary<Guid, AnimalOrigin> originsByAnimalId,
    IReadOnlyDictionary<Guid, string> municipalityNames)
{
    public AnimalOriginDto Resolve(Animal animal)
    {
        if (!originsByAnimalId.TryGetValue(animal.Id, out var origin))
        {
            return AnimalOriginDto.Unassigned;
        }

        return new AnimalOriginDto(
            true,
            origin.AdmissionDate,
            origin.AdmissionMethod,
            OriginLabels.GetAdmissionMethodName(origin.AdmissionMethod),
            origin.FoundStreet,
            ResolveMunicipality(origin.FoundMunicipalityId),
            ResolveMunicipality(origin.ContractorMunicipalityId));
    }

    private MunicipalityReferenceDto ResolveMunicipality(Guid id)
    {
        return municipalityNames.TryGetValue(id, out var name)
            ? new MunicipalityReferenceDto(id, name)
            : MunicipalityReferenceDto.Unassigned;
    }
}

using AnimalRegistry.Modules.Animals.Domain.Animals.Origin.Rules;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.Origin;

public sealed class AnimalOrigin : Entity, IAggregateRoot
{
    private AnimalOrigin()
    {
    }

    private AnimalOrigin(Guid animalId, string shelterId)
    {
        AnimalId = animalId;
        ShelterId = shelterId;
    }

    public Guid AnimalId { get; private set; }
    public string ShelterId { get; private set; } = null!;
    public DateTimeOffset AdmissionDate { get; private set; }
    public AdmissionMethod AdmissionMethod { get; private set; }
    public string FoundStreet { get; private set; } = null!;
    public Guid FoundMunicipalityId { get; private set; }
    public Guid ContractorMunicipalityId { get; private set; }

    public static AnimalOrigin Create(
        Guid animalId,
        string shelterId,
        AnimalOriginData data,
        IReadOnlyCollection<Municipality> municipalities,
        DateTimeOffset startOfNextDay)
    {
        var origin = new AnimalOrigin(animalId, shelterId);
        origin.Apply(data, municipalities, startOfNextDay);
        return origin;
    }

    public void Update(
        AnimalOriginData data,
        IReadOnlyCollection<Municipality> municipalities,
        DateTimeOffset startOfNextDay)
    {
        Apply(data, municipalities, startOfNextDay);
    }

    private void Apply(
        AnimalOriginData data,
        IReadOnlyCollection<Municipality> municipalities,
        DateTimeOffset startOfNextDay)
    {
        CheckRule(new AdmissionDateCannotBeInFutureRule(data.AdmissionDate, startOfNextDay));
        CheckRule(new AdmissionMethodMustBeAssignedRule(data.AdmissionMethod));
        CheckRule(new FoundStreetMustBeProvidedRule(data.FoundStreet));
        CheckMunicipality(data.FoundMunicipalityId, "found", municipalities);
        CheckMunicipality(data.ContractorMunicipalityId, "contractor", municipalities);

        AdmissionDate = data.AdmissionDate;
        AdmissionMethod = data.AdmissionMethod;
        FoundStreet = data.FoundStreet.Trim();
        FoundMunicipalityId = data.FoundMunicipalityId;
        ContractorMunicipalityId = data.ContractorMunicipalityId;
    }

    private void CheckMunicipality(Guid municipalityId, string role, IReadOnlyCollection<Municipality> municipalities)
    {
        CheckRule(new MunicipalityMustBeProvidedRule(municipalityId, role));
        CheckRule(new MunicipalityMustExistInShelterRule(municipalityId, role, ShelterId, municipalities));
        CheckRule(new ArchivedMunicipalityCannotBeNewlyAssignedRule(municipalityId, role, municipalities,
            [FoundMunicipalityId, ContractorMunicipalityId]));
    }
}

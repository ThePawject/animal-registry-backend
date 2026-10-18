using AnimalRegistry.Modules.Animals.Domain.Animals.Origin.Rules;
using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.Origin;

public sealed class AnimalOrigin : ValueObject
{
    private AnimalOrigin()
    {
    }

    private AnimalOrigin(AnimalOriginData data)
    {
        AdmissionDate = data.AdmissionDate;
        AdmissionMethod = data.AdmissionMethod;
        FoundStreet = data.FoundStreet.Trim();
        FoundMunicipalityId = data.FoundMunicipalityId;
        ContractorMunicipalityId = data.ContractorMunicipalityId;
    }

    public DateTimeOffset AdmissionDate { get; private set; }
    public AdmissionMethod AdmissionMethod { get; private set; }
    public string FoundStreet { get; private set; } = null!;
    public Guid FoundMunicipalityId { get; private set; }
    public Guid ContractorMunicipalityId { get; private set; }

    public static AnimalOrigin Create(AnimalOriginData data, DateTimeOffset now)
    {
        CheckRule(new AdmissionDateCannotBeInFutureRule(data.AdmissionDate, now));
        CheckRule(new AdmissionMethodMustBeAssignedRule(data.AdmissionMethod));
        CheckRule(new FoundStreetMustBeProvidedRule(data.FoundStreet));
        CheckRule(new MunicipalityMustBeProvidedRule(data.FoundMunicipalityId, "found"));
        CheckRule(new MunicipalityMustBeProvidedRule(data.ContractorMunicipalityId, "contractor"));

        return new AnimalOrigin(data);
    }

    internal bool IsAssigned(Guid municipalityId)
    {
        return FoundMunicipalityId == municipalityId || ContractorMunicipalityId == municipalityId;
    }
}

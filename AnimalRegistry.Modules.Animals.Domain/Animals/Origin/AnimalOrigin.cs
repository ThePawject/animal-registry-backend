using AnimalRegistry.Modules.Animals.Domain.Animals.Origin.Rules;
using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.Origin;

public sealed class AnimalOrigin
{
    private AnimalOrigin()
    {
    }

    private AnimalOrigin(
        DateTimeOffset admissionDate,
        AdmissionMethod admissionMethod,
        string foundStreet,
        Guid foundMunicipalityId,
        Guid contractorMunicipalityId)
    {
        AdmissionDate = admissionDate;
        AdmissionMethod = admissionMethod;
        FoundStreet = foundStreet;
        FoundMunicipalityId = foundMunicipalityId;
        ContractorMunicipalityId = contractorMunicipalityId;
    }

    public DateTimeOffset AdmissionDate { get; private set; }
    public AdmissionMethod AdmissionMethod { get; private set; }
    public string FoundStreet { get; private set; } = null!;
    public Guid FoundMunicipalityId { get; private set; }

    public Guid ContractorMunicipalityId { get; private set; }

    public static AnimalOrigin Create(
        DateTimeOffset admissionDate,
        AdmissionMethod admissionMethod,
        string foundStreet,
        Guid foundMunicipalityId,
        Guid contractorMunicipalityId)
    {
        CheckRule(new AdmissionMethodMustBeAssignedRule(admissionMethod));
        CheckRule(new FoundStreetMustBeProvidedRule(foundStreet));
        CheckRule(new MunicipalityMustBeProvidedRule(foundMunicipalityId, "found"));
        CheckRule(new MunicipalityMustBeProvidedRule(contractorMunicipalityId, "contractor"));

        return new AnimalOrigin(admissionDate, admissionMethod, foundStreet.Trim(), foundMunicipalityId,
            contractorMunicipalityId);
    }

    private static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }
}

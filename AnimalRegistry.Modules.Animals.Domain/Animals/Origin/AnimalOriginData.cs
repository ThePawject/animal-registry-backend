namespace AnimalRegistry.Modules.Animals.Domain.Animals.Origin;

public sealed record AnimalOriginData(
    DateTimeOffset AdmissionDate,
    AdmissionMethod AdmissionMethod,
    string FoundStreet,
    Guid FoundMunicipalityId,
    Guid ContractorMunicipalityId);

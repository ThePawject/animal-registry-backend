using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;

namespace AnimalRegistry.Modules.Animals.Application.Origin;

public sealed record AnimalOriginInput(
    DateTimeOffset AdmissionDate,
    AdmissionMethod AdmissionMethod,
    string FoundStreet,
    Guid FoundMunicipalityId,
    Guid ContractorMunicipalityId);

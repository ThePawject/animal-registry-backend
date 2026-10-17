using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;

namespace AnimalRegistry.Modules.Animals.Application.Origin;

public sealed record MunicipalityReferenceDto(Guid? Id, string Name)
{
    public static readonly MunicipalityReferenceDto Unassigned = new(null, OriginLabels.Unassigned);
}

public sealed record AnimalOriginDto(
    bool IsAssigned,
    DateTimeOffset? AdmissionDate,
    AdmissionMethod AdmissionMethod,
    string AdmissionMethodName,
    string FoundStreet,
    MunicipalityReferenceDto FoundMunicipality,
    MunicipalityReferenceDto ContractorMunicipality)
{
    public static readonly AnimalOriginDto Unassigned = new(
        false,
        null,
        AdmissionMethod.Unassigned,
        OriginLabels.Unassigned,
        OriginLabels.Unassigned,
        MunicipalityReferenceDto.Unassigned,
        MunicipalityReferenceDto.Unassigned);
}

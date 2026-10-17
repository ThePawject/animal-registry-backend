using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;

namespace AnimalRegistry.Modules.Animals.Api.Origin;

public sealed class AnimalOriginRequest
{
    public DateTimeOffset? AdmissionDate { get; init; }
    public AdmissionMethod? AdmissionMethod { get; init; }
    public string? FoundStreet { get; init; }
    public Guid? FoundMunicipalityId { get; init; }
    public Guid? ContractorMunicipalityId { get; init; }

    internal AnimalOriginInput ToInput()
    {
        return new AnimalOriginInput(
            AdmissionDate!.Value,
            AdmissionMethod!.Value,
            FoundStreet!,
            FoundMunicipalityId!.Value,
            ContractorMunicipalityId!.Value);
    }
}

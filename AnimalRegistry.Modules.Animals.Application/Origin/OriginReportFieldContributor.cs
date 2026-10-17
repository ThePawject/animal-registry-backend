using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Application.Origin;

internal sealed class OriginReportFieldContributor(IAnimalOriginReader originReader) : IAnimalReportFieldContributor
{
    public async Task<IReadOnlyList<AnimalReportField>> GetFieldsAsync(IReadOnlyCollection<Animal> animals,
        CancellationToken cancellationToken)
    {
        var lookup = await originReader.LoadAsync(animals, cancellationToken);
        if (lookup is null)
        {
            return [];
        }

        var origins = new Dictionary<Guid, AnimalOriginDto>();

        AnimalOriginDto Resolve(Animal animal)
        {
            if (!origins.TryGetValue(animal.Id, out var origin))
            {
                origin = lookup.Resolve(animal);
                origins[animal.Id] = origin;
            }

            return origin;
        }

        return
        [
            new AnimalReportField("Data przyjęcia", a => Resolve(a).AdmissionDate is { } date
                ? OriginReportPeriod.ToLocal(date).ToString("dd.MM.yyyy")
                : OriginLabels.Unassigned),
            new AnimalReportField("Sposób trafienia", a => Resolve(a).AdmissionMethodName),
            new AnimalReportField("Ulica znalezienia", a => Resolve(a).FoundStreet),
            new AnimalReportField("Gmina znalezienia", a => Resolve(a).FoundMunicipality.Name),
            new AnimalReportField("Gmina rozliczeniowa", a => Resolve(a).ContractorMunicipality.Name),
        ];
    }
}

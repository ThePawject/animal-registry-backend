using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

public sealed record AnimalReportField(string Label, Func<Animal, string> Format);

internal interface IAnimalReportFieldContributor
{
    Task<IReadOnlyList<AnimalReportField>> GetFieldsAsync(IReadOnlyCollection<Animal> animals,
        CancellationToken cancellationToken);
}

public interface IAnimalReportFieldsProvider
{
    Task<IReadOnlyList<AnimalReportField>> GetFieldsAsync(IReadOnlyCollection<Animal> animals,
        CancellationToken cancellationToken = default);
}

internal sealed class AnimalReportFieldsProvider(IEnumerable<IAnimalReportFieldContributor> contributors)
    : IAnimalReportFieldsProvider
{
    public async Task<IReadOnlyList<AnimalReportField>> GetFieldsAsync(IReadOnlyCollection<Animal> animals,
        CancellationToken cancellationToken = default)
    {
        var fields = new List<AnimalReportField>();
        foreach (var contributor in contributors)
        {
            fields.AddRange(await contributor.GetFieldsAsync(animals, cancellationToken));
        }

        return fields;
    }
}

using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.ReportData;

internal sealed class SelectedAnimalsDataService(IAnimalRepository animalRepository) : ISelectedAnimalsDataService
{
    public async Task<SelectedAnimalsReportData> PrepareReportDataAsync(
        string shelterId,
        IReadOnlyList<Guid> animalIds,
        CancellationToken cancellationToken = default)
    {
        var animals = await animalRepository.GetByIdsAsync(animalIds, shelterId, cancellationToken);

        return new SelectedAnimalsReportData
        {
            ShelterId = shelterId, Animals = animals, RequestedIds = animalIds, ReportDate = DateTimeOffset.UtcNow,
        };
    }
}
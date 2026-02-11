using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.ReportData;

internal sealed class RepositoryDumpDataService(IAnimalRepository animalRepository) : IRepositoryDumpDataService
{
    public async Task<RepositoryDumpReportData> PrepareReportDataAsync(
        string shelterId,
        CancellationToken cancellationToken = default)
    {
        var animals = await animalRepository.GetAllByShelterIdAsync(shelterId, cancellationToken);

        return new RepositoryDumpReportData
        {
            ShelterId = shelterId,
            Animals = animals,
            ReportDate = DateTimeOffset.UtcNow
        };
    }
}

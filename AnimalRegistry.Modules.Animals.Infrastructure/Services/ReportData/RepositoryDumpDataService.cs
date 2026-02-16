using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.ReportData;

internal sealed class RepositoryDumpDataService(IAnimalRepository animalRepository) : IRepositoryDumpDataService
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

    public async Task<RepositoryDumpReportData> PrepareReportDataAsync(
        string shelterId,
        CancellationToken cancellationToken = default)
    {
        var animals = await animalRepository.GetAllByShelterIdAsync(shelterId, cancellationToken);

        var photoData = await DownloadPhotosAsync(animals, cancellationToken);

        return new RepositoryDumpReportData
        {
            ShelterId = shelterId, Animals = animals, ReportDate = DateTimeOffset.UtcNow, PhotoData = photoData,
        };
    }

    private static async Task<Dictionary<string, byte[]>> DownloadPhotosAsync(
        IEnumerable<Animal> animals,
        CancellationToken cancellationToken)
    {
        var photoData = new Dictionary<string, byte[]>();
        var urls = animals
            .SelectMany(a => a.Photos)
            .Select(p => p.Url)
            .Where(url => !string.IsNullOrEmpty(url))
            .Distinct()
            .ToList();

        foreach (var url in urls)
        {
            try
            {
                var data = await HttpClient.GetByteArrayAsync(url!, cancellationToken);
                photoData[url!] = data;
            }
            catch
            {
                // Ignore failed downloads
            }
        }

        return photoData;
    }
}
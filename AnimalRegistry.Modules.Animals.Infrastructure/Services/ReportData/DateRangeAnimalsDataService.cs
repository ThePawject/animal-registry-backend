using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.ReportData;

internal sealed class DateRangeAnimalsDataService(
    IAnimalRepository animalRepository,
    IAnimalEventRepository animalEventRepository) : IDateRangeAnimalsDataService
{
    public async Task<DateRangeAnimalsReportData> PrepareReportDataAsync(
        string shelterId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken = default)
    {
        var eventsInRange = await animalEventRepository.GetByDateRangeAsync(
            shelterId,
            startDate,
            endDate,
            cancellationToken);

        var animalIds = eventsInRange.Select(e => e.AnimalId).Distinct().ToList();
        var animals = await animalRepository.GetByIdsAsync(animalIds, shelterId, cancellationToken);

        var animalsWithFilteredEvents = animals.Select(animal =>
        {
            var animalEvents = eventsInRange
                .Where(e => e.AnimalId == animal.Id)
                .Select(e => e.AnimalEvent)
                .OrderByDescending(e => e.OccurredOn)
                .ToList();

            return new AnimalWithFilteredEvents { Animal = animal, Events = animalEvents };
        }).ToList();

        return new DateRangeAnimalsReportData
        {
            ShelterId = shelterId,
            StartDate = startDate,
            EndDate = endDate,
            Animals = animalsWithFilteredEvents,
            ReportDate = DateTimeOffset.UtcNow,
        };
    }
}
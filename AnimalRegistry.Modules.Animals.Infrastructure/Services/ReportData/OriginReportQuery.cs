using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.ReportData;

internal sealed class OriginReportQuery(AnimalsDbContext context) : IOriginReportQuery
{
    public async Task<IReadOnlyList<AdmittedAnimal>> GetAdmittedBetweenAsync(string shelterId, DateTimeOffset from,
        DateTimeOffset to, CancellationToken cancellationToken = default)
    {
        var admitted = await context.Animals
            .AsNoTracking()
            .Where(a => a.ShelterId == shelterId)
            .Select(a => new { Animal = a, AdmittedOn = (DateTimeOffset?)a.Origin!.AdmissionDate ?? a.CreatedOn })
            .Where(r => r.AdmittedOn >= from && r.AdmittedOn < to)
            .ToListAsync(cancellationToken);

        return admitted
            .Select(r => new AdmittedAnimal(r.Animal, r.AdmittedOn))
            .ToList();
    }
}

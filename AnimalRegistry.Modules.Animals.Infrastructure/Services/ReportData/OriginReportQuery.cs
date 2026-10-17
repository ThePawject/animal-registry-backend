using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.ReportData;

internal sealed class OriginReportQuery(AnimalsDbContext context) : IOriginReportQuery
{
    public async Task<IReadOnlyList<AdmittedAnimal>> GetAdmittedBetweenAsync(string shelterId, DateTimeOffset from,
        DateTimeOffset to, CancellationToken cancellationToken = default)
    {
        var withOrigin = await (
                from origin in context.AnimalOrigins.AsNoTracking()
                where origin.ShelterId == shelterId && origin.AdmissionDate >= @from && origin.AdmissionDate < to
                join animal in context.Animals.AsNoTracking()
                    on new { Id = origin.AnimalId, origin.ShelterId } equals new { animal.Id, animal.ShelterId }
                select new { Animal = animal, Origin = origin })
            .ToListAsync(cancellationToken);

        var withoutOrigin = await context.Animals
            .AsNoTracking()
            .Where(a => a.ShelterId == shelterId && a.CreatedOn >= from && a.CreatedOn < to)
            .Where(a => !context.AnimalOrigins.Any(o => o.AnimalId == a.Id))
            .ToListAsync(cancellationToken);

        return withOrigin
            .Select(r => new AdmittedAnimal(r.Animal, r.Origin, r.Origin.AdmissionDate))
            .Concat(withoutOrigin.Select(a => new AdmittedAnimal(a, null, a.CreatedOn)))
            .ToList();
    }
}

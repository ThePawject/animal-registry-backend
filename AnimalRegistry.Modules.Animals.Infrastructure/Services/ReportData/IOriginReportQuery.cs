namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.ReportData;

internal interface IOriginReportQuery
{
    Task<IReadOnlyList<AdmittedAnimal>> GetAdmittedBetweenAsync(string shelterId, DateTimeOffset from,
        DateTimeOffset to, CancellationToken cancellationToken = default);
}

using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed class GenerateRepositoryDumpReportCommand : IRequest<Result<GenerateRepositoryDumpReportResponse>>;

public sealed record GenerateRepositoryDumpReportResponse
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] Data { get; init; }
}

public interface IRepositoryDumpReportPdfService
{
    byte[] GenerateReport(DateTimeOffset generatedAt, string shelterId);
}

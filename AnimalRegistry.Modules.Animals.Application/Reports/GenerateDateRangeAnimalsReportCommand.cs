using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed class GenerateDateRangeAnimalsReportCommand : IRequest<Result<GenerateDateRangeAnimalsReportResponse>>
{
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
    public List<AnimalSpecies>? Species { get; init; }
}

public sealed record GenerateDateRangeAnimalsReportResponse
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] Data { get; init; }
}
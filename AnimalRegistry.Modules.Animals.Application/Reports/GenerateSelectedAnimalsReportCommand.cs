using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed class GenerateSelectedAnimalsReportCommand : IRequest<Result<GenerateSelectedAnimalsReportResponse>>
{
    public List<Guid> Ids { get; init; } = [];
}

public sealed record GenerateSelectedAnimalsReportResponse
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] Data { get; init; }
}
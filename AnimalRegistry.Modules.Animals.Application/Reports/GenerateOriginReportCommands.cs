using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed record GenerateOriginMonthlyReportCommand(int Year, int Month)
    : IRequest<Result<GenerateOriginReportResponse>>;

internal sealed record GenerateOriginYearlyReportCommand(int Year)
    : IRequest<Result<GenerateOriginReportResponse>>;

public sealed record GenerateOriginReportResponse
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] Data { get; init; }
}

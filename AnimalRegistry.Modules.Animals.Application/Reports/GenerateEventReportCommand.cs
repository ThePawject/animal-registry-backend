using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal enum EventReportPeriod
{
    Week,
    Month,
    Quarter,
    Custom,
}

internal sealed class GenerateEventReportCommand : IRequest<Result<GenerateEventReportResponse>>
{
    public List<EventReportPeriod>? Periods { get; init; }
    public DateTimeOffset? CustomStartDate { get; init; }
    public DateTimeOffset? CustomEndDate { get; init; }
}

public sealed record GenerateEventReportResponse
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] Data { get; init; }
}

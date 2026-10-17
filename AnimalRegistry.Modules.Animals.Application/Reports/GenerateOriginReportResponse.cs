namespace AnimalRegistry.Modules.Animals.Application.Reports;

public sealed record GenerateOriginReportResponse
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] Data { get; init; }
}

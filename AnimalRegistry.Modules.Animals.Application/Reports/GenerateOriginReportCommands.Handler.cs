using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed class GenerateOriginMonthlyReportCommandHandler(
    ICurrentUser currentUser,
    IOriginReportDataService dataService,
    IOriginMonthlyReportPdfService pdfService)
    : IRequestHandler<GenerateOriginMonthlyReportCommand, Result<GenerateOriginReportResponse>>
{
    public async Task<Result<GenerateOriginReportResponse>> Handle(GenerateOriginMonthlyReportCommand request,
        CancellationToken cancellationToken)
    {
        var generatedAt = DateTimeOffset.UtcNow;

        var reportData = await dataService.PrepareMonthlyReportDataAsync(
            currentUser.ShelterId,
            request.Year,
            request.Month,
            cancellationToken);

        var pdfBytes = pdfService.GenerateReport(reportData, generatedAt);
        var fileName = $"RaportPochodzeniaMiesieczny_{request.Month:D2}_{request.Year}.pdf";

        return Result<GenerateOriginReportResponse>.Success(new GenerateOriginReportResponse
        {
            FileName = fileName, ContentType = "application/pdf", Data = pdfBytes,
        });
    }
}

internal sealed class GenerateOriginYearlyReportCommandHandler(
    ICurrentUser currentUser,
    IOriginReportDataService dataService,
    IOriginYearlyReportPdfService pdfService)
    : IRequestHandler<GenerateOriginYearlyReportCommand, Result<GenerateOriginReportResponse>>
{
    public async Task<Result<GenerateOriginReportResponse>> Handle(GenerateOriginYearlyReportCommand request,
        CancellationToken cancellationToken)
    {
        var generatedAt = DateTimeOffset.UtcNow;

        var reportData = await dataService.PrepareYearlyReportDataAsync(
            currentUser.ShelterId,
            request.Year,
            cancellationToken);

        var pdfBytes = pdfService.GenerateReport(reportData, generatedAt);
        var fileName = $"ZestawienieRocznePochodzenia_{request.Year}.pdf";

        return Result<GenerateOriginReportResponse>.Success(new GenerateOriginReportResponse
        {
            FileName = fileName, ContentType = "application/pdf", Data = pdfBytes,
        });
    }
}

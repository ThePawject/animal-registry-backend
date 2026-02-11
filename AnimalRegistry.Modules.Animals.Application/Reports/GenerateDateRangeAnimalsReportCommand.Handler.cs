using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed class GenerateDateRangeAnimalsReportCommandHandler(
    ICurrentUser currentUser,
    IDateRangeAnimalsDataService dataService,
    IDateRangeAnimalsReportPdfService pdfService)
    : IRequestHandler<GenerateDateRangeAnimalsReportCommand, Result<GenerateDateRangeAnimalsReportResponse>>
{
    public async Task<Result<GenerateDateRangeAnimalsReportResponse>> Handle(
        GenerateDateRangeAnimalsReportCommand request,
        CancellationToken cancellationToken)
    {
        var generatedAt = DateTimeOffset.UtcNow;

        var reportData = await dataService.PrepareReportDataAsync(
            currentUser.ShelterId,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        var pdfBytes = pdfService.GenerateReport(reportData, generatedAt);
        var fileName = $"RaportZwierzatZakresDat_{generatedAt:dd_MM_yyyy}.pdf";

        return Result<GenerateDateRangeAnimalsReportResponse>.Success(new GenerateDateRangeAnimalsReportResponse
        {
            FileName = fileName, ContentType = "application/pdf", Data = pdfBytes,
        });
    }
}
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed class GenerateSelectedAnimalsReportCommandHandler(
    ICurrentUser currentUser,
    ISelectedAnimalsDataService dataService,
    ISelectedAnimalsReportPdfService pdfService)
    : IRequestHandler<GenerateSelectedAnimalsReportCommand, Result<GenerateSelectedAnimalsReportResponse>>
{
    public async Task<Result<GenerateSelectedAnimalsReportResponse>> Handle(
        GenerateSelectedAnimalsReportCommand request,
        CancellationToken cancellationToken)
    {
        var generatedAt = DateTimeOffset.UtcNow;

        var reportData = await dataService.PrepareReportDataAsync(
            currentUser.ShelterId,
            request.Ids,
            cancellationToken);

        var pdfBytes = pdfService.GenerateReport(reportData, generatedAt);
        var fileName = $"RaportWybranychZwierzat_{generatedAt:dd_MM_yyyy}.pdf";

        return Result<GenerateSelectedAnimalsReportResponse>.Success(new GenerateSelectedAnimalsReportResponse
        {
            FileName = fileName, ContentType = "application/pdf", Data = pdfBytes,
        });
    }
}
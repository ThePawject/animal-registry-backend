using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed class GenerateRepositoryDumpReportCommandHandler(
    ICurrentUser currentUser,
    IRepositoryDumpDataService dataService,
    IRepositoryDumpReportPdfService pdfService)
    : IRequestHandler<GenerateRepositoryDumpReportCommand, Result<GenerateRepositoryDumpReportResponse>>
{
    public async Task<Result<GenerateRepositoryDumpReportResponse>> Handle(GenerateRepositoryDumpReportCommand request,
        CancellationToken cancellationToken)
    {
        var generatedAt = DateTimeOffset.UtcNow;

        var reportData = await dataService.PrepareReportDataAsync(currentUser.ShelterId, cancellationToken);
        var pdfBytes = pdfService.GenerateReport(reportData, generatedAt);
        var fileName = $"ZrzutRepozytorium_{generatedAt:dd_MM_yyyy}.pdf";

        return Result<GenerateRepositoryDumpReportResponse>.Success(new GenerateRepositoryDumpReportResponse
        {
            FileName = fileName, ContentType = "application/pdf", Data = pdfBytes,
        });
    }
}
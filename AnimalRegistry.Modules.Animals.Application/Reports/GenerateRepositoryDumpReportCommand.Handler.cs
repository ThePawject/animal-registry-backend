using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed class GenerateRepositoryDumpReportCommandHandler(
    ICurrentUser currentUser,
    IRepositoryDumpReportPdfService pdfService)
    : IRequestHandler<GenerateRepositoryDumpReportCommand, Result<GenerateRepositoryDumpReportResponse>>
{
    public async Task<Result<GenerateRepositoryDumpReportResponse>> Handle(GenerateRepositoryDumpReportCommand request,
        CancellationToken cancellationToken)
    {
        var generatedAt = DateTimeOffset.UtcNow;

        var pdfBytes = pdfService.GenerateReport(generatedAt, currentUser.ShelterId);
        var fileName = $"ZrzutRepozytorium_{generatedAt:dd_MM_yyyy}.pdf";

        return await Task.FromResult(Result<GenerateRepositoryDumpReportResponse>.Success(new GenerateRepositoryDumpReportResponse
        {
            FileName = fileName,
            ContentType = "application/pdf",
            Data = pdfBytes
        }));
    }
}

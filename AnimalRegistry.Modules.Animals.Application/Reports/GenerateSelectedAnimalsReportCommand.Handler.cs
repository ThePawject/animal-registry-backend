using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed class GenerateSelectedAnimalsReportCommandHandler(
    ICurrentUser currentUser,
    ISelectedAnimalsReportPdfService pdfService)
    : IRequestHandler<GenerateSelectedAnimalsReportCommand, Result<GenerateSelectedAnimalsReportResponse>>
{
    public async Task<Result<GenerateSelectedAnimalsReportResponse>> Handle(GenerateSelectedAnimalsReportCommand request,
        CancellationToken cancellationToken)
    {
        var generatedAt = DateTimeOffset.UtcNow;

        var pdfBytes = pdfService.GenerateReport(request.Ids, generatedAt, currentUser.ShelterId);
        var fileName = $"RaportWybranychZwierzat_{generatedAt:dd_MM_yyyy}.pdf";

        return await Task.FromResult(Result<GenerateSelectedAnimalsReportResponse>.Success(new GenerateSelectedAnimalsReportResponse
        {
            FileName = fileName,
            ContentType = "application/pdf",
            Data = pdfBytes
        }));
    }
}

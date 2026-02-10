using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed class GenerateDateRangeAnimalsReportCommandHandler(
    ICurrentUser currentUser,
    IDateRangeAnimalsReportPdfService pdfService)
    : IRequestHandler<GenerateDateRangeAnimalsReportCommand, Result<GenerateDateRangeAnimalsReportResponse>>
{
    public async Task<Result<GenerateDateRangeAnimalsReportResponse>> Handle(GenerateDateRangeAnimalsReportCommand request,
        CancellationToken cancellationToken)
    {
        var generatedAt = DateTimeOffset.UtcNow;

        var pdfBytes = pdfService.GenerateReport(request.StartDate, request.EndDate, request.Species, generatedAt, currentUser.ShelterId);
        var fileName = $"RaportZwierzatZakresDat_{generatedAt:dd_MM_yyyy}.pdf";

        return await Task.FromResult(Result<GenerateDateRangeAnimalsReportResponse>.Success(new GenerateDateRangeAnimalsReportResponse
        {
            FileName = fileName,
            ContentType = "application/pdf",
            Data = pdfBytes
        }));
    }
}

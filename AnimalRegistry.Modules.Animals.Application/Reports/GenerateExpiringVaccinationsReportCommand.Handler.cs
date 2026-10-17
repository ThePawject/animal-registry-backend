using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed class GenerateExpiringVaccinationsReportCommandHandler(
    ICurrentUser currentUser,
    IAnimalVaccinationRepository vaccinationRepository,
    IBlobStorageService blobStorageService,
    IExpiringVaccinationsReportPdfService pdfService)
    : IRequestHandler<GenerateExpiringVaccinationsReportCommand, Result<GenerateVaccinationReportResponse>>
{
    public async Task<Result<GenerateVaccinationReportResponse>> Handle(
        GenerateExpiringVaccinationsReportCommand request,
        CancellationToken cancellationToken)
    {
        var generatedAt = DateTimeOffset.UtcNow;
        var (validFrom, validBefore) =
            ExpiringVaccinationsWindow.Resolve(request.ExpiresInDays, request.IncludeExpired, generatedAt);

        var vaccinations = await vaccinationRepository.GetExpiringAsync(currentUser.ShelterId, validFrom,
            validBefore, request.InShelterOnly, cancellationToken);

        var reportData = new ExpiringVaccinationsReportData
        {
            ShelterId = currentUser.ShelterId,
            ExpiresInDays = request.ExpiresInDays,
            IncludeExpired = request.IncludeExpired,
            InShelterOnly = request.InShelterOnly,
            Entries = vaccinations
                .Select(v => VaccinationWithAnimalDto.FromDomain(v, generatedAt, blobStorageService))
                .ToList(),
            ReportDate = generatedAt,
        };

        var pdfBytes = pdfService.GenerateReport(reportData, generatedAt);
        var fileName = $"KonczaceSieSzczepienia_{ShelterCalendar.ToLocal(generatedAt):yyyy-MM-dd}.pdf";

        return Result<GenerateVaccinationReportResponse>.Success(new GenerateVaccinationReportResponse
        {
            FileName = fileName, ContentType = "application/pdf", Data = pdfBytes,
        });
    }
}

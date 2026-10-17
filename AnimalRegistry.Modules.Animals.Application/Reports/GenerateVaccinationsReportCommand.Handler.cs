using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed class GenerateVaccinationsReportCommandHandler(
    ICurrentUser currentUser,
    IVaccinationQuery vaccinationQuery,
    IBlobStorageService blobStorageService,
    IVaccinationsReportPdfService pdfService)
    : IRequestHandler<GenerateVaccinationsReportCommand, Result<GenerateVaccinationReportResponse>>
{
    public async Task<Result<GenerateVaccinationReportResponse>> Handle(GenerateVaccinationsReportCommand request,
        CancellationToken cancellationToken)
    {
        var generatedAt = DateTimeOffset.UtcNow;
        var fromDate = ShelterCalendar.ToLocal(request.From).Date;
        var toDate = ShelterCalendar.ToLocal(request.To).Date;

        var vaccinations = await vaccinationQuery.GetAdministeredBetweenAsync(
            currentUser.ShelterId,
            ShelterCalendar.StartOfDay(fromDate),
            ShelterCalendar.StartOfDay(toDate.AddDays(1)),
            cancellationToken);

        var reportData = new VaccinationsReportData
        {
            ShelterId = currentUser.ShelterId,
            From = fromDate,
            To = toDate,
            Entries = vaccinations
                .Select(v => VaccinationWithAnimalDto.FromDomain(v, generatedAt, blobStorageService))
                .ToList(),
            ReportDate = generatedAt,
        };

        var pdfBytes = pdfService.GenerateReport(reportData, generatedAt);
        var fileName = $"Szczepienia_{fromDate:yyyy-MM-dd}_{toDate:yyyy-MM-dd}.pdf";

        return Result<GenerateVaccinationReportResponse>.Success(new GenerateVaccinationReportResponse
        {
            FileName = fileName, ContentType = "application/pdf", Data = pdfBytes,
        });
    }
}

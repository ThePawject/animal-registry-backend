using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed record GenerateExpiringVaccinationsReportCommand(
    int ExpiresInDays,
    bool IncludeExpired,
    bool InShelterOnly) : IRequest<Result<GenerateVaccinationReportResponse>>;

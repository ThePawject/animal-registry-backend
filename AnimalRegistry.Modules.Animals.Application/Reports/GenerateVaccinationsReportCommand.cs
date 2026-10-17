using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed record GenerateVaccinationsReportCommand(DateTimeOffset From, DateTimeOffset To)
    : IRequest<Result<GenerateVaccinationReportResponse>>;

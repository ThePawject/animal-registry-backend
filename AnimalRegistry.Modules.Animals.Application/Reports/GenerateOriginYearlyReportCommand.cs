using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed record GenerateOriginYearlyReportCommand(int Year)
    : IRequest<Result<GenerateOriginReportResponse>>;

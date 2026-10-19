using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed record GenerateOriginMonthlyReportCommand(int Year, int Month)
    : IRequest<Result<GenerateOriginReportResponse>>;

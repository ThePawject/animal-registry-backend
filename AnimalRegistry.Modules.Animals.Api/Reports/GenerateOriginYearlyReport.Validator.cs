using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

internal sealed class GenerateOriginYearlyReportValidator : Validator<GenerateOriginYearlyReportRequest>
{
    public GenerateOriginYearlyReportValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100);
    }
}

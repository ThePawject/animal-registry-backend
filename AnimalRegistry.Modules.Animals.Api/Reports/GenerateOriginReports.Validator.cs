using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

internal sealed class GenerateOriginMonthlyReportValidator : Validator<GenerateOriginMonthlyReportRequest>
{
    public GenerateOriginMonthlyReportValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100);

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12);
    }
}

internal sealed class GenerateOriginYearlyReportValidator : Validator<GenerateOriginYearlyReportRequest>
{
    public GenerateOriginYearlyReportValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100);
    }
}

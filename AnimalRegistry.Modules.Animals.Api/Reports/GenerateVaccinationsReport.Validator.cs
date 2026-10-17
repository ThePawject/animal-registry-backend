using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

internal sealed class GenerateVaccinationsReportValidator : Validator<GenerateVaccinationsReportRequest>
{
    public GenerateVaccinationsReportValidator()
    {
        RuleFor(x => x.From).NotNull();
        RuleFor(x => x.To).NotNull();

        RuleFor(x => x)
            .Must(x => x.From is null || x.To is null || x.From <= x.To)
            .WithMessage("From date must be earlier or equal to the to date.");
    }
}

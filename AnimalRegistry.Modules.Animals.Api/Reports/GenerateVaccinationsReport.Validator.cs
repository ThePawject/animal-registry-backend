using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

internal sealed class GenerateVaccinationsReportValidator : Validator<GenerateVaccinationsReportRequest>
{
    public GenerateVaccinationsReportValidator()
    {
        RuleFor(x => x.From)
            .NotNull()
            .Must(IsWithinSupportedRange)
            .WithMessage("From date must be between years 2000 and 2100.");

        RuleFor(x => x.To)
            .NotNull()
            .Must(IsWithinSupportedRange)
            .WithMessage("To date must be between years 2000 and 2100.");

        RuleFor(x => x)
            .Must(x => x.From is null || x.To is null || x.From <= x.To)
            .WithMessage("From date must be earlier or equal to the to date.");
    }

    private static bool IsWithinSupportedRange(DateTimeOffset? date)
    {
        return date is not { } value || value.Year is >= 2000 and <= 2100;
    }
}

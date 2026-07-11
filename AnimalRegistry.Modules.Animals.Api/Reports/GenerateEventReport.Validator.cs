using AnimalRegistry.Modules.Animals.Application.Reports;
using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

internal sealed class GenerateEventReportValidator : Validator<GenerateEventReportRequest>
{
    public GenerateEventReportValidator()
    {
        RuleFor(x => x.Periods)
            .Must(periods => periods == null || periods.Count > 0)
            .WithMessage("At least one period must be selected.");

        RuleForEach(x => x.Periods)
            .Must(period => Enum.TryParse<EventReportPeriod>(period, ignoreCase: true, out _))
            .WithMessage("Unknown event report period.");

        When(x => x.Periods?.Any(period => string.Equals(period, nameof(EventReportPeriod.Custom),
            StringComparison.OrdinalIgnoreCase)) == true, () =>
        {
            RuleFor(x => x.CustomStartDate)
                .NotEmpty()
                .WithMessage("Custom start date is required.");

            RuleFor(x => x.CustomEndDate)
                .NotEmpty()
                .WithMessage("Custom end date is required.");
        });

        RuleFor(x => x)
            .Must(x => !x.CustomStartDate.HasValue || !x.CustomEndDate.HasValue || x.CustomStartDate <= x.CustomEndDate)
            .WithMessage("Custom start date must be earlier or equal to the custom end date.");
    }
}

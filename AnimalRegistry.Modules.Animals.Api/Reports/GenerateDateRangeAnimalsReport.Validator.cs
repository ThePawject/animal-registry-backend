using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

internal sealed class GenerateDateRangeAnimalsReportValidator : Validator<GenerateDateRangeAnimalsReportRequest>
{
    public GenerateDateRangeAnimalsReportValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required.");

        RuleFor(x => x)
            .Must(x => x.StartDate <= x.EndDate)
            .WithMessage("Start date must be earlier or equal to the end date.");

        RuleFor(x => x.Species)
            .Must(species => species == null || species.Count <= 50)
            .WithMessage("Species list cannot exceed 50 items.");
    }
}

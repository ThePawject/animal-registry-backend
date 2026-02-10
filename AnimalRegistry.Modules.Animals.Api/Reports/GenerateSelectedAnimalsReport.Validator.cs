using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

internal sealed class GenerateSelectedAnimalsReportValidator : Validator<GenerateSelectedAnimalsReportRequest>
{
    public GenerateSelectedAnimalsReportValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty()
            .WithMessage("List of animal IDs is required.");

        RuleForEach(x => x.Ids)
            .NotEmpty()
            .WithMessage("Animal ID cannot be empty.");
    }
}

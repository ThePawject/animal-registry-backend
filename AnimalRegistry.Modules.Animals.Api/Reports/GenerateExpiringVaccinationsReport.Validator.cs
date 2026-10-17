using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

internal sealed class GenerateExpiringVaccinationsReportValidator
    : Validator<GenerateExpiringVaccinationsReportRequest>
{
    public GenerateExpiringVaccinationsReportValidator()
    {
        RuleFor(x => x.ExpiresInDays).InclusiveBetween(0, 3650);
    }
}

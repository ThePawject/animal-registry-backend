using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.Origin;

internal sealed class AnimalOriginRequestValidator : AbstractValidator<AnimalOriginRequest>
{
    public AnimalOriginRequestValidator()
    {
        RuleFor(x => x.AdmissionDate)
            .NotNull()
            .LessThan(_ => OriginReportPeriod.StartOfNextDay(DateTimeOffset.UtcNow))
            .WithMessage("Admission date cannot be in the future.");

        RuleFor(x => x.AdmissionMethod)
            .NotNull()
            .IsInEnum()
            .NotEqual(AdmissionMethod.Unassigned);

        RuleFor(x => x.FoundStreet)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.FoundMunicipalityId)
            .NotEmpty();

        RuleFor(x => x.ContractorMunicipalityId)
            .NotEmpty();
    }
}

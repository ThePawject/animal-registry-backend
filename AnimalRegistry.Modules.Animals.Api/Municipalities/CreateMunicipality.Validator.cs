using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.Municipalities;

internal sealed class CreateMunicipalityValidator : Validator<CreateMunicipalityRequest>
{
    public CreateMunicipalityValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(Municipality.NameMaxLength);
    }
}

using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.Municipalities;

internal sealed class UpdateMunicipalityValidator : Validator<UpdateMunicipalityRequest>
{
    public UpdateMunicipalityValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(Municipality.NameMaxLength);
    }
}

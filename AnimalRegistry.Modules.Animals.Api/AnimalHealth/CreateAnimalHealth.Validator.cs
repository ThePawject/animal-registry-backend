using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.AnimalHealth;

public sealed class CreateAnimalHealthValidator : Validator<CreateAnimalHealthRequest>
{
    public CreateAnimalHealthValidator()
    {
        RuleFor(x => x.AnimalId).NotEmpty();
        RuleFor(x => x.OccurredOn).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}
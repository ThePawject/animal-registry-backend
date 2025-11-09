using AnimalRegistry.Modules.Animals.Domain.Animals;
using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api;

public sealed class CreateAnimalValidator : Validator<CreateAnimalRequest>
{
    public CreateAnimalValidator()
    {
        RuleFor(x => x.Signature)
            .NotEmpty();
        RuleFor(x => x.TransponderCode)
            .NotEmpty();
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2);
        RuleFor(x => x.Color)
            .NotEmpty();
        RuleFor(x => x.Species)
            .IsInEnum().NotEqual(AnimalSpecies.None);
        RuleFor(x => x.Sex)
            .IsInEnum().NotEqual(AnimalSex.None);
        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(DateTime.UtcNow);
    }
}
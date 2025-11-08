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
        RuleFor(x => x.DictItemSpeciesId)
            .GreaterThan(0);
        RuleFor(x => x.DictItemSexId)
            .GreaterThan(0);
        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(DateTime.UtcNow);
    }
}
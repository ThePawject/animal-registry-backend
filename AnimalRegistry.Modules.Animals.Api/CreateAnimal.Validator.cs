using AnimalRegistry.Modules.Animals.Domain.Animals;
using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class CreateAnimalValidator : Validator<CreateAnimalRequest>
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
            .NotEmpty()
            .IsInEnum();
        RuleFor(x => x.Sex)
            .NotEmpty()
            .IsInEnum();
        RuleFor(x => x.BirthDate)
            .NotEmpty();
    }

    private static bool BeValidSpecies(string species)
    {
        return Enum.TryParse<AnimalSpecies>(species, true, out var result) && result != AnimalSpecies.None;
    }

    private static bool BeValidSex(string sex)
    {
        return Enum.TryParse<AnimalSex>(sex, true, out var result) && result != AnimalSex.None;
    }
}
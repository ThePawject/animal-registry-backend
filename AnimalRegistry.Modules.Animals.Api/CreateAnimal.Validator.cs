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
        RuleFor(x => x.MainPhotoIndex)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MainPhotoIndex.HasValue);
    }
}
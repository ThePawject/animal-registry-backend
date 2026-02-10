using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class UpdateAnimalValidator : Validator<UpdateAnimalRequest>
{
    public UpdateAnimalValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

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

        RuleFor(x => x)
            .Must(x => !(x.MainPhotoId.HasValue && x.MainPhotoIndex.HasValue))
            .WithMessage("Cannot set both MainPhotoId and MainPhotoIndex");

        RuleFor(x => x.MainPhotoId)
            .Must((request, mainPhotoId) =>
                !mainPhotoId.HasValue || request.ExistingPhotoIds.Contains(mainPhotoId.Value))
            .When(x => x.MainPhotoId.HasValue);
    }
}
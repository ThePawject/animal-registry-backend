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
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(100);

        RuleFor(x => x.TransponderCode)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);

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
            .When(x => x.MainPhotoId.HasValue)
            .WithMessage("MainPhotoId must belong to the provided ExistingPhotoIds");

        RuleFor(x => x.NewPhotos)
            .NotNull()
            .Must(photos => photos.Count <= 10)
            .WithMessage("Maximum 10 photos allowed");

        RuleForEach(x => x.NewPhotos)
            .Must(file => file.Length <= 10 * 1024 * 1024)
            .WithMessage("Each photo must be 10MB or less");

        RuleFor(x => x.NewPhotos)
            .Must(photos => photos.Sum(f => f.Length) <= 100 * 1024 * 1024)
            .WithMessage("Total photo size must not exceed 100MB");
    }
}

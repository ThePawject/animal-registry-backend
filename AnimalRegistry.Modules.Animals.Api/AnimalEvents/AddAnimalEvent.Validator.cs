using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.AnimalEvents;

public sealed class AddAnimalEventValidator : Validator<AddAnimalEventRequest>
{
    public AddAnimalEventValidator()
    {
        RuleFor(x => x.AnimalId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.OccurredOn).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.PerformedBy).MaximumLength(100);
    }
}
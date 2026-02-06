using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.AnimalEvents;

public sealed class UpdateAnimalEventValidator : Validator<UpdateAnimalEventRequest>
{
    public UpdateAnimalEventValidator()
    {
        RuleFor(x => x.AnimalId).NotEmpty();
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.OccurredOn).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.PerformedBy).MaximumLength(100);
    }
}
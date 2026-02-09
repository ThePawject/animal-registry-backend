using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.AnimalEvents;

public sealed class CreateAnimalEventValidator : Validator<CreateAnimalEventRequest>
{
    public CreateAnimalEventValidator()
    {
        RuleFor(x => x.AnimalId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.OccurredOn).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.AnimalEvents;

public sealed class DeleteAnimalEventValidator : Validator<DeleteAnimalEventRequest>
{
    public DeleteAnimalEventValidator()
    {
        RuleFor(x => x.AnimalId).NotEmpty();
        RuleFor(x => x.EventId).NotEmpty();
    }
}
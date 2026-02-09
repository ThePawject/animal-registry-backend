using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.AnimalHealth;

public sealed class UpdateAnimalHealthValidator : Validator<UpdateAnimalHealthRequest>
{
    public UpdateAnimalHealthValidator()
    {
        RuleFor(x => x.AnimalId).NotEmpty();
        RuleFor(x => x.HealthRecordId).NotEmpty();
        RuleFor(x => x.OccurredOn).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}
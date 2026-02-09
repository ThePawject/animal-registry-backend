using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api;

public sealed class DeleteAnimalValidator : Validator<DeleteAnimalRequest>
{
    public DeleteAnimalValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
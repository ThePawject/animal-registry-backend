using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class GetAnimalValidator : Validator<GetAnimalRequest>
{
    public GetAnimalValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
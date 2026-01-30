using FluentValidation;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api;

public sealed class GetAnimalValidator : Validator<GetAnimalRequest>
{
    public GetAnimalValidator()
    {
        RuleFor(x => x.Id).NotNull();
    }
}

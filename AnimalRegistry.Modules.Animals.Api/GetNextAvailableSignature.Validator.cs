using AnimalRegistry.Modules.Animals.Domain.Animals;
using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class GetNextAvailableSignatureRequestValidator : Validator<GetNextAvailableSignatureRequest>
{
    public GetNextAvailableSignatureRequestValidator()
    {
        RuleFor(x => x.Species)
            .Must(BeValidSpecies)
            .WithMessage("Species must be Dog or Cat.");
    }

    private static bool BeValidSpecies(AnimalSpecies species)
    {
        return species is AnimalSpecies.Dog or AnimalSpecies.Cat;
    }
}

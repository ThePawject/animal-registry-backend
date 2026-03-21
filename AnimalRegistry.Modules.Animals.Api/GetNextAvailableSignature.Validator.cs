using AnimalRegistry.Modules.Animals.Domain.Animals;
using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class GetNextAvailableSignatureRequestValidator : Validator<GetNextAvailableSignatureRequest>
{
    public GetNextAvailableSignatureRequestValidator()
    {
        RuleFor(x => x.Species)
            .IsInEnum()
            .WithMessage("Species must be a valid value.");
    }
}

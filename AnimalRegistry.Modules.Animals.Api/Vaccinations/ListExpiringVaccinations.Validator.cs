using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

internal sealed class ListExpiringVaccinationsValidator : Validator<ListExpiringVaccinationsRequest>
{
    public ListExpiringVaccinationsValidator()
    {
        RuleFor(x => x.ExpiresInDays).InclusiveBetween(0, 3650);
    }
}

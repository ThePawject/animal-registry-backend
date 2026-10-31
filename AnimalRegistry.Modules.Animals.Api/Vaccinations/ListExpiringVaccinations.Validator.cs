using AnimalRegistry.Shared.Pagination;
using FastEndpoints;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

internal sealed class ListExpiringVaccinationsValidator : Validator<ListExpiringVaccinationsRequest>
{
    public ListExpiringVaccinationsValidator(IOptions<PaginationSettings> settings)
    {
        Include(new PaginationRequestValidator<ListExpiringVaccinationsRequest>(settings));

        RuleFor(x => x.ExpiresInDays).InclusiveBetween(0, 3650);
    }
}

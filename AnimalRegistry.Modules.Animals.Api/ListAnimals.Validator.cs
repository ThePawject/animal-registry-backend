using AnimalRegistry.Shared.Pagination;
using FastEndpoints;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class ListAnimalsValidator : Validator<ListAnimalsRequest>
{
    public ListAnimalsValidator(IOptions<PaginationSettings> settings)
    {
        Include(new PaginationRequestValidator<ListAnimalsRequest>(settings));

        RuleFor(x => x.KeyWordSearch)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.KeyWordSearch));
    }
}
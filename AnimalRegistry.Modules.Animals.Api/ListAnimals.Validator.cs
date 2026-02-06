using AnimalRegistry.Shared.Pagination;
using FastEndpoints;
using Microsoft.Extensions.Options;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class ListAnimalsValidator : Validator<ListAnimalsRequest>
{
    public ListAnimalsValidator(IOptions<PaginationSettings> settings)
    {
        Include(new PaginationRequestValidator<ListAnimalsRequest>(settings));
    }
}
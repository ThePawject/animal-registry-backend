using FluentValidation;
using Microsoft.Extensions.Options;

namespace AnimalRegistry.Shared.Pagination;

public class PaginationRequestValidator<T> : AbstractValidator<T> where T : PaginationRequest
{
    public PaginationRequestValidator(IOptions<PaginationSettings> settings)
    {
        var paginationSettings = settings.Value;

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(paginationSettings.MinPage);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(paginationSettings.MinPageSize)
            .LessThanOrEqualTo(paginationSettings.MaxPageSize);
    }
}

namespace AnimalRegistry.Shared.Pagination;

public sealed record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize);

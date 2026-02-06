namespace AnimalRegistry.Shared.Pagination;

public abstract record PaginationRequest
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public int Skip => (Page - 1) * PageSize;
}
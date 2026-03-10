namespace AnimalRegistry.Shared.Pagination;

public sealed class PaginationSettings
{
    public int DefaultPageSize { get; init; } = 20;
    public int MaxPageSize { get; init; } = 100;
    public int MinPageSize { get; init; } = 1;
    public int MinPage { get; init; } = 1;
}
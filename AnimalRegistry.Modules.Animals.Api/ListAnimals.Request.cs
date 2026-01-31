namespace AnimalRegistry.Modules.Animals.Api;

public sealed class ListAnimalsRequest
{
    public const string Route = "/animals";

    // optional paging/query parameters to make the DTO bindable by FastEndpoints
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}

namespace AnimalRegistry.Modules.Shelters.Api;

public sealed class SetShelterFeatureStateRequest
{
    public const string Route = "/shelter/features/{key}";

    public string Key { get; init; } = null!;
    public bool IsEnabled { get; init; }

    public static string BuildRoute(string key)
    {
        return Route.Replace("{key}", Uri.EscapeDataString(key));
    }
}

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

public sealed class TestUser
{
    public const string Name = "Test User";

    public const string Email = "test@example.com";

    public const string UserId = "TestUser-0000";

    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, string> CustomClaims { get; init; } = new Dictionary<string, string>();

    public static TestUser WithShelterAccess(string shelterId)
    {
        return new TestUser { Roles = [$"Shelter_Access_{shelterId}"] };
    }

    public static TestUser WithoutShelterAccess()
    {
        return new TestUser { Roles = Array.Empty<string>() };
    }

    public static TestUser WithMultipleShelters(params string[] shelterIds)
    {
        return new TestUser { Roles = shelterIds.Select(id => $"Shelter_Access_{id}").ToArray() };
    }

    public static TestUser WithCustomRole(string role)
    {
        return new TestUser { Roles = [role] };
    }
}
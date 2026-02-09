using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.AnimalHealths;

public sealed class AnimalHealth : Entity
{
    private AnimalHealth()
    {
        // For ORM
    }

    private AnimalHealth(DateTimeOffset occurredOn, string description, string performedBy)
    {
        OccurredOn = occurredOn;
        Description = description;
        PerformedBy = performedBy;
    }

    public DateTimeOffset OccurredOn { get; private set; }
    public string Description { get; private set; } = null!;
    public string PerformedBy { get; private set; } = null!;

    public static AnimalHealth Create(DateTimeOffset occurredOn, string description, string performedBy)
    {
        return new AnimalHealth(occurredOn, description, performedBy);
    }

    internal void Update(DateTimeOffset occurredOn, string description)
    {
        OccurredOn = occurredOn;
        Description = description;
    }
}
using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;

public sealed class AnimalEvent : Entity
{
    private AnimalEvent()
    {
        // For ORM
    }

    private AnimalEvent(AnimalEventType type, DateTimeOffset occurredOn, string description, string performedBy)
    {
        OccurredOn = occurredOn;
        Type = type;
        Description = description;
        PerformedBy = performedBy;
    }

    public AnimalEventType Type { get; private set; }
    public string Description { get; private set; } = null!;
    public DateTimeOffset OccurredOn { get; private set; }
    public string PerformedBy { get; private set; } = null!;

    public static AnimalEvent Create(AnimalEventType type, DateTimeOffset occurredOn, string description,
        string performedBy)
    {
        return new AnimalEvent(type, occurredOn, description, performedBy);
    }

    internal void Update(AnimalEventType type, DateTimeOffset occurredOn, string description, string performedBy)
    {
        Type = type;
        OccurredOn = occurredOn;
        Description = description;
        PerformedBy = performedBy;
    }
}
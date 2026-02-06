using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;

public sealed class AnimalEvent : Entity
{
    private AnimalEvent()
    {
        // For ORM
    }

    private AnimalEvent(TimeProvider timeProvider, AnimalEventType type, string description, string performedBy)
    {
        OccurredOn = timeProvider.GetUtcNow();
        Type = type;
        Description = description;
        PerformedBy = performedBy;
    }

    public AnimalEventType Type { get; private set; }
    public string Description { get; private set; } = null!;
    public DateTimeOffset OccurredOn { get; private set; }
    public string PerformedBy { get; private set; } = null!;

    public static AnimalEvent Create(TimeProvider timeProvider, AnimalEventType type, string description,
        string performedBy)
    {
        return new AnimalEvent(timeProvider, type, description, performedBy);
    }
}
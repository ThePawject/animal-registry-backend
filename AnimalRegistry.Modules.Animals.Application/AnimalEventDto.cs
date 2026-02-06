using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;

namespace AnimalRegistry.Modules.Animals.Application;

public sealed record AnimalEventDto(
    Guid Id,
    AnimalEventType Type,
    DateTimeOffset OccurredOn,
    string Description,
    string PerformedBy
)
{
    public static AnimalEventDto FromDomain(AnimalEvent e)
    {
        return new AnimalEventDto(
            e.Id,
            e.Type,
            e.OccurredOn,
            e.Description,
            e.PerformedBy
        );
    }
}

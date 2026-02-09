using DomainAnimalHealth = AnimalRegistry.Modules.Animals.Domain.Animals.AnimalHealths.AnimalHealth;

namespace AnimalRegistry.Modules.Animals.Application;

public sealed record AnimalHealthDto(
    Guid Id,
    DateTimeOffset OccurredOn,
    string Description,
    string PerformedBy
)
{
    public static AnimalHealthDto FromDomain(DomainAnimalHealth health)
    {
        return new AnimalHealthDto(
            health.Id,
            health.OccurredOn,
            health.Description,
            health.PerformedBy
        );
    }
}
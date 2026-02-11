using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;

namespace AnimalRegistry.Modules.Animals.Domain.Animals;

internal interface IAnimalEventRepository
{
    Task<IReadOnlyList<AnimalEventWithAnimalInfo>> GetAllByShelterIdAsync(string shelterId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnimalEventWithAnimalInfo>> GetByDateRangeAsync(
        string shelterId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken = default);
}

public sealed class AnimalEventWithAnimalInfo(
    AnimalEvent animalEvent,
    AnimalSpecies species,
    Guid animalId,
    string animalName)
{
    public AnimalEvent AnimalEvent { get; } = animalEvent;
    public AnimalSpecies Species { get; } = species;
    public Guid AnimalId { get; } = animalId;
    public string AnimalName { get; } = animalName;
}
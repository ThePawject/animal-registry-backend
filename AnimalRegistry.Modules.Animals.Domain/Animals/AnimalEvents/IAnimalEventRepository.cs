using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;

namespace AnimalRegistry.Modules.Animals.Domain.Animals;

internal interface IAnimalEventRepository
{
    Task<IReadOnlyList<AnimalEventWithAnimalInfo>> GetAllByShelterIdAsync(string shelterId, CancellationToken cancellationToken = default);
}

public sealed class AnimalEventWithAnimalInfo(AnimalEvent animalEvent, AnimalSpecies species)
{
    public AnimalEvent AnimalEvent { get; } = animalEvent;
    public AnimalSpecies Species { get; } = species;
}
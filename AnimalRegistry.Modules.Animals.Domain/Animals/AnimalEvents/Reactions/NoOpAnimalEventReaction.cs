namespace AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents.Reactions;

internal sealed class NoOpAnimalEventReaction : IAnimalEventReaction
{
    public void Apply(Animal animal, AnimalEvent animalEvent)
    {
    }

    public void Undo(Animal animal, AnimalEvent animalEvent)
    {
    }
}

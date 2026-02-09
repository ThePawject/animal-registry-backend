namespace AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents.Reactions;

internal sealed class OutOfShelterAnimalEventReaction : IAnimalEventReaction
{
    public void Apply(Animal animal, AnimalEvent animalEvent) =>
        animal.SetOutOfShelter();

    public void Undo(Animal animal, AnimalEvent animalEvent) => 
        animal.SetInShelter();
}
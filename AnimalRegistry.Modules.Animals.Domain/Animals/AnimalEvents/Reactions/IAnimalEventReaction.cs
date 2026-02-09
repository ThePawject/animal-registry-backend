namespace AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents.Reactions;

public interface IAnimalEventReaction
{
    void Apply(Animal animal, AnimalEvent animalEvent);

    void Undo(Animal animal, AnimalEvent animalEvent);
}

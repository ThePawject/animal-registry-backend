namespace AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents.Reactions;

internal sealed class AdmissionToShelterAnimalEventReaction : IAnimalEventReaction
{
    public void Apply(Animal animal, AnimalEvent animalEvent)
    {
        animal.SetInShelter();
    }

    public void Undo(Animal animal, AnimalEvent animalEvent)
    {
        animal.RecalculateShelterStatus();
    }
}
namespace AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents.Reactions;

public static class AnimalEventReactionRegistry
{
    private static readonly Dictionary<AnimalEventType, IAnimalEventReaction> Reactions =
        new()
        {
            [AnimalEventType.None] = new NoOpAnimalEventReaction(),
            [AnimalEventType.Adoption] = new OutOfShelterAnimalEventReaction(),
            [AnimalEventType.PickedUpByOwner] = new OutOfShelterAnimalEventReaction(),
            [AnimalEventType.Death] = new OutOfShelterAnimalEventReaction(),
            [AnimalEventType.Euthanasia] = new OutOfShelterAnimalEventReaction(),
        };

    public static IAnimalEventReaction For(AnimalEventType type)
    {
        return Reactions.TryGetValue(type, out var reaction) ? reaction : new NoOpAnimalEventReaction();
    }
}
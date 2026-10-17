using AnimalRegistry.Shared.ShelterFeatures;

namespace AnimalRegistry.Modules.Animals.Application.Origin;

internal sealed class AnimalOriginFeature : IShelterFeature
{
    public const string FeatureKey = "animal-origin";

    public string Key => FeatureKey;

    public string Name => "Pochodzenie zwierzęcia";

    public string Description =>
        "Pozwala zapisać, skąd zwierzę trafiło do schroniska: kiedy i w jaki sposób zostało przyjęte, " +
        "gdzie je znaleziono oraz która gmina płaci za jego pobyt. " +
        "Na tej podstawie przygotujesz miesięczne i roczne zestawienia do rozliczeń z gminami. " +
        "Po włączeniu te informacje trzeba podawać przy dodawaniu i edycji każdego zwierzęcia. " +
        "Przy zwierzętach dodanych wcześniej zobaczysz „Brak przypisania”, dopóki ich nie uzupełnisz. " +
        "Moduł można w każdej chwili wyłączyć, a wpisane dane nie zostaną utracone.";
}

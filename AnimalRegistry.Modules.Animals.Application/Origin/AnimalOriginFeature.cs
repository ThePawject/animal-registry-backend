using AnimalRegistry.Shared.ShelterFeatures;

namespace AnimalRegistry.Modules.Animals.Application.Origin;

internal sealed class AnimalOriginFeature : IShelterFeature
{
    public const string FeatureKey = "animal-origin";

    public string Key => FeatureKey;

    public string Name => "Pochodzenie zwierzęcia";

    public string Description =>
        "Wymagane dane o pochodzeniu zwierzęcia (data i sposób przyjęcia, miejsce znalezienia, gmina rozliczeniowa), " +
        "słownik gmin-kontrahentów oraz raport miesięczny i zestawienie roczne per gmina. " +
        "Zwierzęta dodane przed włączeniem modułu otrzymują wartość „Brak przypisania”.";
}

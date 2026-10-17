using AnimalRegistry.Shared.ShelterFeatures;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed class AnimalVaccinationFeature : IShelterFeature
{
    public const string FeatureKey = "animal-vaccinations";

    public string Key => FeatureKey;

    public string Name => "Szczepienia";

    public string Description =>
        "Pozwala prowadzić rejestr szczepień każdego zwierzęcia: nazwę szczepionki, datę podania, " +
        "termin ważności, lekarza weterynarii, numer serii, notatkę i skan dokumentu. " +
        "Zobaczysz, którym zwierzętom szczepienie niedługo się kończy, i wydrukujesz zestawienia szczepień. " +
        "Moduł można w każdej chwili wyłączyć, a wpisane dane nie zostaną utracone.";
}

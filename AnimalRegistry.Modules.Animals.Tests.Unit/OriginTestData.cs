using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;

namespace AnimalRegistry.Modules.Animals.Tests.Unit;

internal static class OriginTestData
{
    public const string ShelterId = "test-shelter-id";
    public static readonly DateTimeOffset AdmissionDate = new(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset Now = new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset StartOfNextDay = new(2026, 3, 16, 0, 0, 0, TimeSpan.FromHours(1));
    private static readonly DateTimeOffset FarFuture = new(2100, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static Animal CreateAnimal(string signature = "2026/0001")
    {
        return Animal.Create(AnimalSignature.Create(signature).Value!, null, "Burek", "Brown", "Mix",
            "", AnimalSpecies.Dog, AnimalSex.Male, null, ShelterId);
    }

    public static Municipality CreateMunicipality(string name, bool archived = false)
    {
        var municipality = Municipality.Create(ShelterId, name);
        if (archived)
        {
            municipality.Archive();
        }

        return municipality;
    }

    public static AnimalOriginData Data(
        Guid foundId,
        Guid contractorId,
        DateTimeOffset? admissionDate = null,
        AdmissionMethod method = AdmissionMethod.Abandoned,
        string street = "Leśna 1")
    {
        return new AnimalOriginData(admissionDate ?? AdmissionDate, method, street, foundId, contractorId);
    }

    public static AnimalOrigin CreateOrigin(Animal animal, Municipality found, Municipality contractor,
        DateTimeOffset? admissionDate = null)
    {
        return AnimalOrigin.Create(animal.Id, animal.ShelterId, Data(found.Id, contractor.Id, admissionDate),
            [found, contractor], FarFuture);
    }
}

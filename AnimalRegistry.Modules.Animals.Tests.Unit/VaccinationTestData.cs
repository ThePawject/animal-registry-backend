using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;

namespace AnimalRegistry.Modules.Animals.Tests.Unit;

internal static class VaccinationTestData
{
    public const string ShelterId = "test-shelter-id";
    public const string UserEmail = "vet@shelter.test";
    public static readonly DateTimeOffset Now = new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset StartOfNextDay = new(2026, 3, 16, 0, 0, 0, TimeSpan.FromHours(1));
    public static readonly DateTimeOffset AdministeredOn = new(2026, 3, 10, 9, 0, 0, TimeSpan.FromHours(1));

    public static AnimalVaccinationData Data(
        string vaccineName = "Nobivac Rabies",
        DateTimeOffset? administeredOn = null,
        DateTimeOffset? validUntil = null,
        string? veterinarian = "Jan Kowalski",
        string? batchNumber = "A123",
        string? notes = null)
    {
        return new AnimalVaccinationData(vaccineName, administeredOn ?? AdministeredOn, validUntil, veterinarian,
            batchNumber, notes);
    }

    public static AnimalVaccination CreateVaccination(Guid? animalId = null, DateTimeOffset? validUntil = null)
    {
        return AnimalVaccination.Create(animalId ?? Guid.NewGuid(), ShelterId, Data(validUntil: validUntil), null,
            false, UserEmail, Now);
    }

    public static DateTimeOffset LocalDate(int year, int month, int day)
    {
        return new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.FromHours(1));
    }
}

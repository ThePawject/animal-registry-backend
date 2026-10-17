using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations.Rules;
using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;

public sealed class AnimalVaccination : Entity, IAggregateRoot
{
    public const int VaccineNameMaxLength = 200;
    public const int VeterinarianMaxLength = 200;
    public const int BatchNumberMaxLength = 100;
    public const int NotesMaxLength = 1000;

    private AnimalVaccination()
    {
    }

    private AnimalVaccination(Guid animalId, string shelterId, string createdBy, DateTimeOffset now)
    {
        AnimalId = animalId;
        ShelterId = shelterId;
        CreatedBy = createdBy;
        CreatedOn = now;
    }

    public Guid AnimalId { get; private set; }
    public string ShelterId { get; private set; } = null!;
    public string VaccineName { get; private set; } = null!;
    public DateTimeOffset AdministeredOn { get; private set; }
    public DateTimeOffset? ValidUntil { get; private set; }
    public string? Veterinarian { get; private set; }
    public string? BatchNumber { get; private set; }
    public string? Notes { get; private set; }
    public VaccinationAttachment? Attachment { get; private set; }
    public string CreatedBy { get; private set; } = null!;
    public DateTimeOffset CreatedOn { get; private set; }
    public DateTimeOffset ModifiedOn { get; private set; }

    public static AnimalVaccination Create(
        Guid animalId,
        string shelterId,
        AnimalVaccinationData data,
        string createdBy,
        DateTimeOffset now)
    {
        var vaccination = new AnimalVaccination(animalId, shelterId, createdBy, now);
        vaccination.Apply(data, now);
        return vaccination;
    }

    public void Update(AnimalVaccinationData data, DateTimeOffset now)
    {
        Apply(data, now);
    }

    public void SetAttachment(VaccinationAttachment attachment, DateTimeOffset now)
    {
        Attachment = attachment;
        ModifiedOn = now;
    }

    public void RemoveAttachment(DateTimeOffset now)
    {
        Attachment = null;
        ModifiedOn = now;
    }

    public bool IsActive(DateTimeOffset now)
    {
        return DaysUntilExpiry(now) is null or >= 0;
    }

    public int? DaysUntilExpiry(DateTimeOffset now)
    {
        if (ValidUntil is not { } validUntil)
        {
            return null;
        }

        return (ShelterCalendar.ToLocal(validUntil).Date - ShelterCalendar.ToLocal(now).Date).Days;
    }

    private void Apply(AnimalVaccinationData data, DateTimeOffset now)
    {
        CheckRule(new VaccineNameMustBeProvidedRule(data.VaccineName));
        CheckRule(new AdministrationDateCannotBeInFutureRule(data.AdministeredOn, now));
        CheckRule(new ValidUntilCannotBeBeforeAdministrationRule(data.AdministeredOn, data.ValidUntil));

        VaccineName = data.VaccineName.Trim();
        AdministeredOn = data.AdministeredOn;
        ValidUntil = data.ValidUntil;
        Veterinarian = Normalize(data.Veterinarian);
        BatchNumber = Normalize(data.BatchNumber);
        Notes = Normalize(data.Notes);
        ModifiedOn = now;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

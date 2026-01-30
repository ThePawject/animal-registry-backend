using AnimalRegistry.Modules.Animals.Domain.Animals.DomainEvents;
using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals;

internal sealed class Animal : Entity, IAggregateRoot
{
    private Animal()
    {
    }

    private Animal(
        string signature,
        string transponderCode,
        string name,
        string color,
        AnimalSpecies species,
        AnimalSex sex,
        DateTimeOffset birthDate)
    {
        Signature = signature;
        TransponderCode = transponderCode;
        Name = name;
        Color = color;
        Species = species;
        Sex = sex;
        BirthDate = birthDate;

        IsActive = true;
        CreatedOn = DateTimeOffset.Now;
        ModifiedOn = DateTimeOffset.UtcNow;
    }

    public string TransponderCode { get; private set; } = null!;
    public string Signature { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Color { get; private set; } = null!;
    public AnimalSpecies Species { get; private set; }
    public AnimalSex Sex { get; private set; }
    public DateTimeOffset BirthDate { get; private set; }
    public DateTimeOffset CreatedOn { get; private set; }
    public DateTimeOffset ModifiedOn { get; private set; }
    public bool IsActive { get; private set; }

    public static Animal Create(
        string signature,
        string transponderCode,
        string name,
        string color,
        AnimalSpecies species,
        AnimalSex sex,
        DateTimeOffset birthDate)
    {
        var animal = new Animal(signature, transponderCode, name, color, species, sex, birthDate);

        animal.AddDomainEvent(new AnimalCreatedDomainEvent(animal.Id, animal.Signature, animal.Name));

        return animal;
    }

    public void Archive()
    {
        if (!IsActive) return;

        IsActive = false;
        ModifiedOn = DateTimeOffset.UtcNow;
        AddDomainEvent(new AnimalArchivedDomainEvent(Id));
    }
}
using AnimalRegistry.Shared.DDD;

internal class Animal : Entity, IAggregateRoot
{
    private Animal()
    {
    }

    private Animal(
        string signature,
        string transponderCode,
        string name,
        string color,
        int dictItemSpeciesId,
        int dictItemSexId,
        DateTime birthDate)
    {
        Signature = signature;
        TransponderCode = transponderCode;
        Name = name;
        Color = color;
        DictItemSpeciesId = dictItemSpeciesId;
        DictItemSexId = dictItemSexId;
        BirthDate = birthDate;

        IsActive = true;
        CreatedOn = DateTime.UtcNow;
        ModifiedOn = DateTime.UtcNow;
    }

    public string TransponderCode { get; private set; } = null!;
    public string Signature { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Color { get; private set; } = null!;
    public int DictItemSpeciesId { get; private set; }
    public int DictItemSexId { get; private set; }
    public DateTime BirthDate { get; private set; }
    public DateTime CreatedOn { get; private set; }
    public DateTime ModifiedOn { get; private set; }
    public bool IsActive { get; private set; }

    public static Animal Create(
        string signature,
        string transponderCode,
        string name,
        string color,
        int dictItemSpeciesId,
        int dictItemSexId,
        DateTime birthDate)
    {
        var animal = new Animal(signature, transponderCode, name, color, dictItemSpeciesId, dictItemSexId, birthDate);

        animal.AddDomainEvent(new AnimalCreatedDomainEvent(animal.Id, animal.Signature, animal.Name));

        return animal;
    }

    public void Archive()
    {
        if (!IsActive) return;

        IsActive = false;
        ModifiedOn = DateTime.UtcNow;
        AddDomainEvent(new AnimalArchivedDomainEvent(Id));
    }
}
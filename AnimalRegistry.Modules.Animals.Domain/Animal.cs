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
        //TODO: check rule
        // if (string.IsNullOrWhiteSpace(signature))
        //     throw new ArgumentException("Signature cannot be empty.", nameof(signature));
        //
        // if (string.IsNullOrWhiteSpace(name))
        //     throw new ArgumentException("Name cannot be empty.", nameof(name));
        //
        // if (dictItemSpeciesId <= 0)
        //     throw new ArgumentException("SpeciesId must be valid.", nameof(dictItemSpeciesId));
        //
        // if (dictItemSexId <= 0)
        //     throw new ArgumentException("SexId must be valid.", nameof(dictItemSexId));
        //
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

        // animal.AddDomainEvent(new AnimalCreatedEvent(animal.Id, animal.Signature, animal.Name));

        return animal;
    }

    public void UpdateDetails(string name, string color, string transponderCode, DateTime birthDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name;
        Color = color;
        TransponderCode = transponderCode;
        BirthDate = birthDate;
        ModifiedOn = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (!IsActive) return;

        IsActive = false;
        ModifiedOn = DateTime.UtcNow;
        // AddDomainEvent(new AnimalArchivedEvent(Id));
    }
}
using AnimalRegistry.Modules.Animals.Domain.Animals.DomainEvents;
using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals;

public sealed class Animal : Entity, IAggregateRoot
{
    private readonly List<AnimalPhoto> _photos = [];

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
        DateTimeOffset birthDate,
        string shelterId)
    {
        Signature = signature;
        TransponderCode = transponderCode;
        Name = name;
        Color = color;
        Species = species;
        Sex = sex;
        BirthDate = birthDate;
        ShelterId = shelterId;

        IsActive = true;
        CreatedOn = DateTimeOffset.Now;
        ModifiedOn = DateTimeOffset.UtcNow;
    }

    public string TransponderCode { get; private set; } = null!;
    public string Signature { get; } = null!;
    public string Name { get; } = null!;
    public string Color { get; private set; } = null!;
    public AnimalSpecies Species { get; private set; }
    public AnimalSex Sex { get; private set; }
    public DateTimeOffset BirthDate { get; private set; }
    public DateTimeOffset CreatedOn { get; private set; }
    public DateTimeOffset ModifiedOn { get; private set; }
    public bool IsActive { get; private set; }
    public string ShelterId { get; private set; } = null!;
    public IReadOnlyCollection<AnimalPhoto> Photos => _photos.AsReadOnly();
    public AnimalPhoto? MainPhoto => _photos.FirstOrDefault(p => p.IsMain);

    public static Animal Create(
        string signature,
        string transponderCode,
        string name,
        string color,
        AnimalSpecies species,
        AnimalSex sex,
        DateTimeOffset birthDate,
        string shelterId)
    {
        var animal = new Animal(signature, transponderCode, name, color, species, sex, birthDate, shelterId);

        animal.AddDomainEvent(new AnimalCreatedDomainEvent(animal.Id, animal.Signature, animal.Name));

        return animal;
    }

    public void Archive()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        ModifiedOn = DateTimeOffset.UtcNow;
        AddDomainEvent(new AnimalArchivedDomainEvent(Id));
    }

    public void AddPhoto(string blobUrl, string fileName, bool isMain = false)
    {
        if (isMain)
        {
            foreach (var existingPhoto in _photos.Where(p => p.IsMain))
            {
                existingPhoto.UnsetAsMain();
            }
        }

        var photo = AnimalPhoto.Create(blobUrl, fileName, isMain);
        _photos.Add(photo);
        ModifiedOn = DateTimeOffset.UtcNow;
    }

    public void RemovePhoto(Guid photoId)
    {
        var photoToRemove = _photos.FirstOrDefault(p => p.Id == photoId);
        if (photoToRemove is not null)
        {
            _photos.Remove(photoToRemove);
            ModifiedOn = DateTimeOffset.UtcNow;
        }
    }

    public void SetMainPhoto(Guid photoId)
    {
        var photoToSet = _photos.FirstOrDefault(p => p.Id == photoId);
        if (photoToSet is null)
        {
            return;
        }

        foreach (var p in _photos.Where(p => p.IsMain))
        {
            p.UnsetAsMain();
        }

        photoToSet.SetAsMain();
        ModifiedOn = DateTimeOffset.UtcNow;
    }
}
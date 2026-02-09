using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents.Reactions;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalHealths;
using AnimalRegistry.Modules.Animals.Domain.Animals.DomainEvents;
using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals;

public sealed class Animal : Entity, IAggregateRoot
{
    private readonly List<AnimalEvent> _events = [];
    private readonly List<AnimalHealth> _healthRecords = [];
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

        IsInShelter = true;
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
    public bool IsInShelter { get; private set; }
    public string ShelterId { get; private set; } = null!;
    public Guid? MainPhotoId { get; private set; }
    public IReadOnlyCollection<AnimalPhoto> Photos => GetOrderedPhotos();
    public IReadOnlyCollection<AnimalEvent> Events => _events.AsReadOnly();
    public IReadOnlyCollection<AnimalHealth> HealthRecords => _healthRecords.AsReadOnly();

    public AnimalPhoto? MainPhoto => MainPhotoId.HasValue
        ? _photos.FirstOrDefault(p => p.Id == MainPhotoId.Value)
        : null;

    private IReadOnlyCollection<AnimalPhoto> GetOrderedPhotos()
    {
        if (MainPhotoId.HasValue)
        {
            var mainPhoto = _photos.FirstOrDefault(p => p.Id == MainPhotoId.Value);
            if (mainPhoto is not null)
            {
                var others = _photos
                    .Where(p => p.Id != MainPhotoId.Value)
                    .OrderBy(p => p.UploadedOn)
                    .ToList();
                return new List<AnimalPhoto> { mainPhoto }.Concat(others).ToList().AsReadOnly();
            }
        }

        return _photos.OrderBy(p => p.UploadedOn).ToList().AsReadOnly();
    }

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

    public void Update(
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
        ModifiedOn = DateTimeOffset.UtcNow;
        
        AddDomainEvent(new AnimalUpdatedDomainEvent(Id, Name, Signature));
    }

    internal void SetOutOfShelter()
    {
        if (!IsInShelter)
        {
            return;
        }

        IsInShelter = false;
        ModifiedOn = DateTimeOffset.UtcNow;
        AddDomainEvent(new AnimalArchivedDomainEvent(Id));
    }

    internal void SetInShelter()
    {
        if (IsInShelter)
        {
            return;
        }

        IsInShelter = true;
        ModifiedOn = DateTimeOffset.UtcNow;
    }


    public void AddPhoto(string blobPath, string fileName, bool isMain = false)
    {
        var photo = AnimalPhoto.Create(blobPath, fileName);
        _photos.Add(photo);

        if (isMain)
        {
            MainPhotoId = photo.Id;
        }

        ModifiedOn = DateTimeOffset.UtcNow;
    }

    public void RemovePhoto(Guid photoId)
    {
        var photoToRemove = _photos.FirstOrDefault(p => p.Id == photoId);
        if (photoToRemove is not null)
        {
            _photos.Remove(photoToRemove);

            if (MainPhotoId == photoId)
            {
                MainPhotoId = null;
            }

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

        MainPhotoId = photoId;
        ModifiedOn = DateTimeOffset.UtcNow;
    }

    internal void AddEvent(AnimalEventType type, DateTimeOffset occurredOn, string description, string performedBy)
    {
        var animalEvent = AnimalEvent.Create(type, occurredOn, description, performedBy);
        _events.Add(animalEvent);

        AnimalEventReactionRegistry.For(type).Apply(this, animalEvent);
        AddDomainEvent(new AnimalEventAddedDomainEvent(Id, animalEvent));
    }

    internal void UpdateEvent(Guid eventId, AnimalEventType type, DateTimeOffset occurredOn, string description)
    {
        var animalEvent = _events.FirstOrDefault(e => e.Id == eventId);
        if (animalEvent is null)
        {
            return;
        }

        AnimalEventReactionRegistry.For(animalEvent.Type).Undo(this, animalEvent);
        animalEvent.Update(type, occurredOn, description);
        AnimalEventReactionRegistry.For(animalEvent.Type).Apply(this, animalEvent);
    }

    internal void RemoveEvent(Guid eventId)
    {
        var eventToRemove = _events.FirstOrDefault(e => e.Id == eventId);
        if (eventToRemove is not null)
        {
            AnimalEventReactionRegistry.For(eventToRemove.Type).Undo(this, eventToRemove);
            _events.Remove(eventToRemove);
            ModifiedOn = DateTimeOffset.UtcNow;
        }
    }

    internal void AddHealthRecord(DateTimeOffset occurredOn, string description, string performedBy)
    {
        var healthRecord = AnimalHealth.Create(occurredOn, description, performedBy);
        _healthRecords.Add(healthRecord);
        ModifiedOn = DateTimeOffset.UtcNow;
    }

    internal void UpdateHealthRecord(Guid healthRecordId, DateTimeOffset occurredOn, string description)
    {
        var healthRecord = _healthRecords.FirstOrDefault(h => h.Id == healthRecordId);
        if (healthRecord is null)
        {
            return;
        }

        healthRecord.Update(occurredOn, description);
        ModifiedOn = DateTimeOffset.UtcNow;
    }

    internal void RemoveHealthRecord(Guid healthRecordId)
    {
        var healthRecord = _healthRecords.FirstOrDefault(h => h.Id == healthRecordId);
        if (healthRecord is not null)
        {
            _healthRecords.Remove(healthRecord);
            ModifiedOn = DateTimeOffset.UtcNow;
        }
    }
}
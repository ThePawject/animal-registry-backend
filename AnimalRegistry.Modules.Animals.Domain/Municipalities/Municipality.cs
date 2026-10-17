using AnimalRegistry.Modules.Animals.Domain.Municipalities.Rules;
using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Municipalities;

public sealed class Municipality : Entity, IAggregateRoot
{
    public const int NameMaxLength = 200;

    private Municipality()
    {
    }

    private Municipality(string shelterId, string name)
    {
        ShelterId = shelterId;
        Name = name;
        CreatedOn = DateTimeOffset.UtcNow;
    }

    public string ShelterId { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public bool IsArchived { get; private set; }
    public DateTimeOffset CreatedOn { get; private set; }

    public static Municipality Create(string shelterId, string name)
    {
        CheckRule(new MunicipalityNameMustBeProvidedRule(name));
        return new Municipality(shelterId, name.Trim());
    }

    public void Rename(string name)
    {
        CheckRule(new MunicipalityNameMustBeProvidedRule(name));
        Name = name.Trim();
    }

    public void Archive()
    {
        IsArchived = true;
    }

    public void Restore()
    {
        IsArchived = false;
    }
}

using AnimalRegistry.Modules.Animals.Domain.Municipalities;

namespace AnimalRegistry.Modules.Animals.Application.Municipalities;

public sealed record MunicipalityDto(Guid Id, string Name, bool IsArchived, DateTimeOffset CreatedOn)
{
    public static MunicipalityDto FromDomain(Municipality m)
    {
        return new MunicipalityDto(m.Id, m.Name, m.IsArchived, m.CreatedOn);
    }
}

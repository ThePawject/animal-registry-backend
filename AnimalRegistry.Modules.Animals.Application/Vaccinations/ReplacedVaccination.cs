using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed record ReplacedVaccination(AnimalVaccination? Vaccination, bool IsReplacedElsewhere)
{
    public static readonly ReplacedVaccination None = new(null, false);
}

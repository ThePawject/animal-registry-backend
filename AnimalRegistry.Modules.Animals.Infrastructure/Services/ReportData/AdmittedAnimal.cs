using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.ReportData;

internal sealed record AdmittedAnimal(Animal Animal, AnimalOrigin? Origin, DateTimeOffset AdmittedOn);

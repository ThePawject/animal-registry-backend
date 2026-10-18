using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.ReportData;

internal sealed record AdmittedAnimal(Animal Animal, DateTimeOffset AdmittedOn);

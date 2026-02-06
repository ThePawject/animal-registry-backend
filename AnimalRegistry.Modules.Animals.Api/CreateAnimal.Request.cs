using AnimalRegistry.Modules.Animals.Domain.Animals;
using Microsoft.AspNetCore.Http;

namespace AnimalRegistry.Modules.Animals.Api;

public sealed class CreateAnimalRequest
{
    public const string Route = "/animals";
    public string Signature { get; init; } = null!;
    public string TransponderCode { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Color { get; init; } = null!;
    public AnimalSpecies Species { get; init; }
    public AnimalSex Sex { get; init; }
    public DateTimeOffset BirthDate { get; init; }
    public IFormFileCollection Photos { get; init; } = new FormFileCollection();
    public int? MainPhotoIndex { get; init; }
}
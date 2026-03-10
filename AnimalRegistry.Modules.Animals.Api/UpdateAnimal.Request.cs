using AnimalRegistry.Modules.Animals.Domain.Animals;
using Microsoft.AspNetCore.Http;

namespace AnimalRegistry.Modules.Animals.Api;

public sealed class UpdateAnimalRequest
{
    public const string Route = "/animals/{id}";

    public Guid Id { get; init; }
    public string Signature { get; init; } = null!;
    public string TransponderCode { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Color { get; init; } = null!;
    public AnimalSpecies Species { get; init; }
    public AnimalSex Sex { get; init; }
    public DateTimeOffset BirthDate { get; init; }
    public List<Guid> ExistingPhotoIds { get; init; } = [];
    public IFormFileCollection NewPhotos { get; init; } = new FormFileCollection();
    public Guid? MainPhotoId { get; init; }
    public int? MainPhotoIndex { get; init; }

    public static string BuildRoute(Guid id)
    {
        return Route.Replace("{id}", id.ToString());
    }
}
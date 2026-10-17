using Microsoft.AspNetCore.Http;

namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

public sealed class UploadVaccinationAttachmentRequest
{
    public const string Route = "/animals/{AnimalId}/vaccinations/{VaccinationId}/attachment";

    public Guid AnimalId { get; init; }
    public Guid VaccinationId { get; init; }
    public IFormFile? File { get; init; }

    public static string BuildRoute(Guid animalId, Guid vaccinationId)
    {
        return Route.Replace("{AnimalId}", animalId.ToString())
            .Replace("{VaccinationId}", vaccinationId.ToString());
    }
}

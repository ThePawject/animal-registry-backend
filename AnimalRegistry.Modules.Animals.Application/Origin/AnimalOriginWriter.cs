using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.DDD;
using AnimalRegistry.Shared.ShelterFeatures;

namespace AnimalRegistry.Modules.Animals.Application.Origin;

internal sealed class AnimalOriginWriter(
    IShelterFeatureToggles toggles,
    IMunicipalityRepository municipalityRepository,
    ICurrentUser currentUser) : IAnimalOriginWriter
{
    public async Task<Result> ApplyAsync(Animal animal, AnimalOriginInput? input,
        CancellationToken cancellationToken = default)
    {
        if (!await toggles.IsEnabledAsync(AnimalOriginFeature.FeatureKey, cancellationToken))
        {
            return Result.Success();
        }

        if (input is null)
        {
            return Result.ValidationError("Animal origin is required when the origin module is enabled.");
        }

        AnimalOrigin origin;
        try
        {
            origin = AnimalOrigin.Create(
                input.AdmissionDate,
                input.AdmissionMethod,
                input.FoundStreet,
                input.FoundMunicipalityId,
                input.ContractorMunicipalityId);
        }
        catch (BusinessRuleValidationException ex)
        {
            return Result.ValidationError(ex.Message);
        }

        var municipalities = await municipalityRepository.GetByIdsAsync(
            [input.FoundMunicipalityId, input.ContractorMunicipalityId],
            currentUser.ShelterId,
            cancellationToken);
        var byId = municipalities.ToDictionary(m => m.Id);

        var current = animal.Origin;
        var foundError = ValidateMunicipality(byId, input.FoundMunicipalityId, current?.FoundMunicipalityId,
            "found");
        if (foundError is not null)
        {
            return foundError;
        }

        var contractorError = ValidateMunicipality(byId, input.ContractorMunicipalityId,
            current?.ContractorMunicipalityId, "contractor");
        if (contractorError is not null)
        {
            return contractorError;
        }

        animal.SetOrigin(origin);

        return Result.Success();
    }

    private static Result? ValidateMunicipality(
        IReadOnlyDictionary<Guid, Municipality> municipalities,
        Guid id,
        Guid? currentId,
        string role)
    {
        if (!municipalities.TryGetValue(id, out var municipality))
        {
            return Result.ValidationError($"The {role} municipality '{id}' does not exist.");
        }

        if (municipality.IsArchived && id != currentId)
        {
            return Result.ValidationError($"The {role} municipality '{municipality.Name}' is archived.");
        }

        return null;
    }
}

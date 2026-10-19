using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

internal sealed class CreateAnimalVaccinationValidator : Validator<CreateAnimalVaccinationRequest>
{
    public CreateAnimalVaccinationValidator()
    {
        RuleFor(x => x.AnimalId).NotEmpty();
        RuleFor(x => x.VaccineName).NotEmpty().MaximumLength(AnimalVaccination.VaccineNameMaxLength);
        RuleFor(x => x.AdministeredOn).NotNull();
        RuleFor(x => x.Veterinarian).MaximumLength(AnimalVaccination.VeterinarianMaxLength);
        RuleFor(x => x.BatchNumber).MaximumLength(AnimalVaccination.BatchNumberMaxLength);
        RuleFor(x => x.Notes).MaximumLength(AnimalVaccination.NotesMaxLength);
        RuleFor(x => x.ReplacesVaccinationId).NotEqual(Guid.Empty);
    }
}

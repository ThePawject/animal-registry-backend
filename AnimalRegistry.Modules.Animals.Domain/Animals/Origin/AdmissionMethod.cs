namespace AnimalRegistry.Modules.Animals.Domain.Animals.Origin;

public enum AdmissionMethod
{
    Unassigned = 0,
    Intervention = 1,
    Abandoned = 2,
    BroughtByPrivatePerson = 3,
    ReturnedFromAdoption = 4,
    TransferredFromAnotherShelter = 5,
    Other = 6,
}

namespace AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;

public enum AnimalEventType
{
    None = 0,
    AdmissionToShelter = 1,
    StartOfQuarantine = 2,
    EndOfQuarantine = 3,
    InfectiousDiseaseVaccination = 4,
    Deworming = 5,
    Defleaing = 6,
    Sterilization = 7,
    RabiesVaccination = 8,
    Adoption = 9,
    Walk = 10,
    NewKennelNumber = 11,
    PickedUpByOwner = 12,
    Weighing = 13,
    Euthanasia = 14,
    Death = 15,
}
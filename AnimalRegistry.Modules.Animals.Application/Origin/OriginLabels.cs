using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;

namespace AnimalRegistry.Modules.Animals.Application.Origin;

public static class OriginLabels
{
    public const string Unassigned = "Brak przypisania";

    public static string GetAdmissionMethodName(AdmissionMethod method)
    {
        return method switch
        {
            AdmissionMethod.Unassigned => Unassigned,
            AdmissionMethod.Intervention => "Interwencja (Straż Miejska / Policja)",
            AdmissionMethod.Abandoned => "Podrzucony",
            AdmissionMethod.BroughtByPrivatePerson => "Doprowadzony przez osobę fizyczną",
            AdmissionMethod.ReturnedFromAdoption => "Zwrot z adopcji",
            AdmissionMethod.TransferredFromAnotherShelter => "Przekazany z innego schroniska",
            AdmissionMethod.Other => "Inne",
            _ => method.ToString(),
        };
    }
}

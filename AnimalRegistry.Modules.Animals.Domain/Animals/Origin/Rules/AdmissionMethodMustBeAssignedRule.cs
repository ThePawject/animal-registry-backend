using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.Origin.Rules;

internal sealed class AdmissionMethodMustBeAssignedRule(AdmissionMethod admissionMethod) : IBusinessRule
{
    public string Message => "Admission method must be provided.";

    public bool IsBroken()
    {
        return admissionMethod == AdmissionMethod.Unassigned || !Enum.IsDefined(admissionMethod);
    }
}

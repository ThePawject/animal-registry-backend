using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Shared.Outbox.Domain.Rules;

internal sealed class ErrorMessageMustNotBeEmptyRule(string errorMessage) : IBusinessRule
{
    public string Message => "Error message cannot be null or whitespace";

    public bool IsBroken()
    {
        return string.IsNullOrWhiteSpace(errorMessage);
    }
}
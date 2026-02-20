using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Shared.Outbox.Domain.Rules;

internal sealed class MessageTypeMustNotBeEmptyRule(string messageType) : IBusinessRule
{
    public string Message => "Message type cannot be null or whitespace";

    public bool IsBroken()
    {
        return string.IsNullOrWhiteSpace(messageType);
    }
}
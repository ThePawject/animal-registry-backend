using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Shared.Outbox.Domain.Rules;

internal sealed class MessageDataMustNotBeEmptyRule(string messageData) : IBusinessRule
{
    public string Message => "Message data cannot be null or whitespace";

    public bool IsBroken()
    {
        return string.IsNullOrWhiteSpace(messageData);
    }
}
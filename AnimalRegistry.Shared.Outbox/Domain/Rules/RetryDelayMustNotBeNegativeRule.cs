using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Shared.Outbox.Domain.Rules;

internal sealed class RetryDelayMustNotBeNegativeRule(TimeSpan retryDelay) : IBusinessRule
{
    public string Message => "Retry delay cannot be negative";

    public bool IsBroken()
    {
        return retryDelay < TimeSpan.Zero;
    }
}
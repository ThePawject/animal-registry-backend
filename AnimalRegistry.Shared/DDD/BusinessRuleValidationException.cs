namespace AnimalRegistry.Shared.DDD;

public sealed class BusinessRuleValidationException(IBusinessRule brokenRule) : Exception(brokenRule.Message)
{
    private IBusinessRule BrokenRule { get; } = brokenRule;

    // ReSharper disable once UnusedMember.Local
    private string Details { get; } = brokenRule.Message;

    public override string ToString()
    {
        return $"{BrokenRule.GetType().FullName}: {BrokenRule.Message}";
    }
}
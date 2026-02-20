using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Audit.Domain.AuditEntries.Rules;

internal sealed class ExecutionTimeMustNotBeNegativeRule(TimeSpan executionTime) : IBusinessRule
{
    public string Message => "Execution time cannot be negative";

    public bool IsBroken()
    {
        return executionTime < TimeSpan.Zero;
    }
}
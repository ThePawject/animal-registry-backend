namespace AnimalRegistry.Shared.Outbox.Application;

public class OutboxSettings
{
    public int PollingIntervalSeconds { get; set; } = 30;
    public int BatchSize { get; set; } = 100;
    public int MaxRetryCount { get; set; } = 5;
    public int InitialRetryDelaySeconds { get; set; } = 10;
    public int MaxRetryDelaySeconds { get; set; } = 3600;

    public TimeSpan PollingInterval => TimeSpan.FromSeconds(PollingIntervalSeconds);
    public TimeSpan InitialRetryDelay => TimeSpan.FromSeconds(InitialRetryDelaySeconds);
    public TimeSpan MaxRetryDelay => TimeSpan.FromSeconds(MaxRetryDelaySeconds);

    public TimeSpan CalculateRetryDelay(int retryCount)
    {
        var exponentialDelay = InitialRetryDelay * Math.Pow(2, retryCount);
        var cappedDelay = Math.Min(exponentialDelay.TotalSeconds, MaxRetryDelay.TotalSeconds);
        return TimeSpan.FromSeconds(cappedDelay);
    }
}
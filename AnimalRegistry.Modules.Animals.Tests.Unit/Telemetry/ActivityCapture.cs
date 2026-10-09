using System.Diagnostics;
using AnimalRegistry.Shared.Telemetry;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Telemetry;

internal sealed class ActivityCapture : IDisposable
{
    private readonly ActivityListener _listener;

    public ActivityCapture()
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == AnimalRegistryTelemetry.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => Stopped.Add(activity),
        };

        ActivitySource.AddActivityListener(_listener);
    }

    public List<Activity> Stopped { get; } = [];

    public void Dispose() => _listener.Dispose();
}

using System.Diagnostics;

namespace AnimalRegistry.Shared.Telemetry;

public static class AnimalRegistryTelemetry
{
    public const string SourceName = "AnimalRegistry";

    public static readonly ActivitySource Source = new(SourceName);
}

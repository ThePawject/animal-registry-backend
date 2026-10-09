namespace AnimalRegistry.Shared.Telemetry;

public sealed class TelemetryOptions
{
    public const string SectionName = "Telemetry";
    public const double DefaultSamplingRatio = 0.25;

    public double SamplingRatio { get; init; } = DefaultSamplingRatio;

    public bool HasValidSamplingRatio => SamplingRatio >= 0 && SamplingRatio <= 1;

    public double EffectiveSamplingRatio => HasValidSamplingRatio ? SamplingRatio : DefaultSamplingRatio;
}

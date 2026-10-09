using AnimalRegistry.Shared.Telemetry;
using FluentAssertions;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Telemetry;

public class TelemetryOptionsTests
{
    [Fact]
    public void SamplingRatio_ShouldDefaultTo_QuarterOfTraces()
    {
        var options = new TelemetryOptions();

        options.SamplingRatio.Should().Be(0.25);
        options.HasValidSamplingRatio.Should().BeTrue();
        options.EffectiveSamplingRatio.Should().Be(0.25);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(1)]
    public void EffectiveSamplingRatio_ShouldUseConfiguredValue_WhenWithinZeroAndOne(double ratio)
    {
        var options = new TelemetryOptions { SamplingRatio = ratio };

        options.HasValidSamplingRatio.Should().BeTrue();
        options.EffectiveSamplingRatio.Should().Be(ratio);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void EffectiveSamplingRatio_ShouldFallBackToDefault_WhenOutsideZeroAndOne(double ratio)
    {
        var options = new TelemetryOptions { SamplingRatio = ratio };

        options.HasValidSamplingRatio.Should().BeFalse();
        options.EffectiveSamplingRatio.Should().Be(TelemetryOptions.DefaultSamplingRatio);
    }
}

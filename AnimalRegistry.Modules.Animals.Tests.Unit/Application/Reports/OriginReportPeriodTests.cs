using AnimalRegistry.Modules.Animals.Application.Reports;
using FluentAssertions;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Application.Reports;

public sealed class OriginReportPeriodTests
{
    [Fact]
    public void StartOfNextDay_ShouldUsePolishCalendar()
    {
        var value = new DateTimeOffset(2026, 3, 31, 23, 30, 0, TimeSpan.Zero);

        OriginReportPeriod.StartOfNextDay(value)
            .Should().Be(new DateTimeOffset(2026, 4, 2, 0, 0, 0, TimeSpan.FromHours(2)));
    }

    [Fact]
    public void StartOfNextDay_ShouldHandleWinterTime()
    {
        var value = new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

        OriginReportPeriod.StartOfNextDay(value)
            .Should().Be(new DateTimeOffset(2026, 1, 16, 0, 0, 0, TimeSpan.FromHours(1)));
    }
}

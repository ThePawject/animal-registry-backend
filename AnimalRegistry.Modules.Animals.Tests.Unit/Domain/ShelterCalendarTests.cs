using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Domain;
using FluentAssertions;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Domain;

public sealed class ShelterCalendarTests
{
    [Fact]
    public void StartOfNextDay_ShouldUsePolishCalendar()
    {
        var value = new DateTimeOffset(2026, 3, 31, 23, 30, 0, TimeSpan.Zero);

        ShelterCalendar.StartOfNextDay(value)
            .Should().Be(new DateTimeOffset(2026, 4, 2, 0, 0, 0, TimeSpan.FromHours(2)));
    }

    [Fact]
    public void StartOfNextDay_ShouldHandleWinterTime()
    {
        var value = new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

        ShelterCalendar.StartOfNextDay(value)
            .Should().Be(new DateTimeOffset(2026, 1, 16, 0, 0, 0, TimeSpan.FromHours(1)));
    }

    [Fact]
    public void ToLocal_ShouldConvertToPolishTime()
    {
        var value = new DateTimeOffset(2026, 3, 31, 23, 30, 0, TimeSpan.Zero);

        ShelterCalendar.ToLocal(value).Should().Be(new DateTime(2026, 4, 1, 1, 30, 0));
    }

    [Theory]
    [InlineData(DateTimeKind.Unspecified)]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Local)]
    public void StartOfDay_ShouldTreatDateAsPolishDate_RegardlessOfKind(DateTimeKind kind)
    {
        var date = new DateTime(2026, 7, 1, 15, 0, 0, kind);

        ShelterCalendar.StartOfDay(date)
            .Should().Be(new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.FromHours(2)));
    }

    [Fact]
    public void OriginReportMonth_ShouldStartAndEndAtPolishMidnight()
    {
        OriginReportPeriod.Month(2026, 3).Should().Be((
            new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.FromHours(1)),
            new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.FromHours(2))));
    }
}

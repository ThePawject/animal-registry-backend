using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using FluentAssertions;
using static AnimalRegistry.Modules.Animals.Tests.Unit.VaccinationTestData;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Application.Vaccinations;

public sealed class ExpiringVaccinationsWindowTests
{
    [Fact]
    public void Resolve_ShouldStartTodayAndEndAfterLastDay()
    {
        var (validFrom, validBefore) = ExpiringVaccinationsWindow.Resolve(30, false, Now);

        validFrom.Should().Be(LocalDate(2026, 3, 15));
        validBefore.Should().Be(new DateTimeOffset(2026, 4, 15, 0, 0, 0, TimeSpan.FromHours(2)));
    }

    [Fact]
    public void Resolve_ShouldCoverOnlyToday_WhenDaysIsZero()
    {
        var (validFrom, validBefore) = ExpiringVaccinationsWindow.Resolve(0, false, Now);

        validFrom.Should().Be(LocalDate(2026, 3, 15));
        validBefore.Should().Be(LocalDate(2026, 3, 16));
    }

    [Fact]
    public void Resolve_ShouldHaveNoLowerBound_WhenExpiredAreIncluded()
    {
        var (validFrom, _) = ExpiringVaccinationsWindow.Resolve(7, true, Now);

        validFrom.Should().BeNull();
    }

    [Fact]
    public void Resolve_ShouldUseShelterDay_WhenUtcIsStillPreviousDay()
    {
        var lateEvening = new DateTimeOffset(2026, 3, 14, 23, 30, 0, TimeSpan.Zero);

        var (validFrom, _) = ExpiringVaccinationsWindow.Resolve(0, false, lateEvening);

        validFrom.Should().Be(LocalDate(2026, 3, 15));
    }
}

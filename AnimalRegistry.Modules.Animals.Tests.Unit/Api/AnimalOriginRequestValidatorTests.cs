using AnimalRegistry.Modules.Animals.Api.Origin;
using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using FluentAssertions;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Api;

public sealed class AnimalOriginRequestValidatorTests
{
    private readonly AnimalOriginRequestValidator _validator = new();

    private static AnimalOriginRequest Request(DateTimeOffset admissionDate)
    {
        return new AnimalOriginRequest
        {
            AdmissionDate = admissionDate,
            AdmissionMethod = AdmissionMethod.Abandoned,
            FoundStreet = "Leśna 1",
            FoundMunicipalityId = Guid.NewGuid(),
            ContractorMunicipalityId = Guid.NewGuid(),
        };
    }

    [Fact]
    public void Validate_ShouldAcceptAdmissionDateLaterToday()
    {
        var endOfToday = OriginReportPeriod.StartOfNextDay(DateTimeOffset.UtcNow).AddMinutes(-1);

        _validator.Validate(Request(endOfToday)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldRejectAdmissionDateTomorrow()
    {
        var tomorrow = OriginReportPeriod.StartOfNextDay(DateTimeOffset.UtcNow);

        var result = _validator.Validate(Request(tomorrow));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(AnimalOriginRequest.AdmissionDate));
    }
}

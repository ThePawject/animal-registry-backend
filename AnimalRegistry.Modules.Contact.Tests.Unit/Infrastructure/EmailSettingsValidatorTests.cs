using AnimalRegistry.Modules.Contact.Infrastructure.Email;
using FluentAssertions;

namespace AnimalRegistry.Modules.Contact.Tests.Unit.Infrastructure;

public sealed class EmailSettingsValidatorTests
{
    private readonly EmailSettingsValidator _validator = new();

    private static EmailSettings ValidSettings()
    {
        return new EmailSettings
        {
            Enabled = true,
            Host = "smtp.gmail.com",
            Port = 587,
            FromAddress = "noreply@shelter.example",
            ContactRecipient = "team@shelter.example",
            TimeoutSeconds = 30,
        };
    }

    [Fact]
    public void Validate_WithCompleteSettings_Succeeds()
    {
        _validator.Validate(null, ValidSettings()).Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenDisabled_SkipsEveryOtherRule()
    {
        var settings = new EmailSettings { Enabled = false };

        _validator.Validate(null, settings).Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithPort25_Fails()
    {
        var settings = ValidSettings();
        settings.Port = 25;

        var result = _validator.Validate(null, settings);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("587");
    }

    [Theory]
    [InlineData("")]
    [InlineData("<to-be-injected>")]
    [InlineData("not-an-address")]
    public void Validate_WithAnInvalidRecipient_Fails(string recipient)
    {
        var settings = ValidSettings();
        settings.ContactRecipient = recipient;

        _validator.Validate(null, settings).Failed.Should().BeTrue();
    }
}
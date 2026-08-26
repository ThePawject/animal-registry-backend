using AnimalRegistry.Modules.Contact.Infrastructure.Email;
using FluentAssertions;

namespace AnimalRegistry.Modules.Contact.Tests.Unit.Infrastructure;

public sealed class EmailHeaderSanitizerTests
{
    [Theory]
    [InlineData("Shelter\r\nBcc: bot@evil.pl", "Shelter Bcc: bot@evil.pl")]
    [InlineData("Shelter\nBcc: bot@evil.pl", "Shelter Bcc: bot@evil.pl")]
    [InlineData("Shelter\rBcc: bot@evil.pl", "Shelter Bcc: bot@evil.pl")]
    public void Sanitize_WithLineBreaks_ReplacesThemWithASingleSpace(string value, string expected)
    {
        EmailHeaderSanitizer.Sanitize(value).Should().Be(expected);
    }

    [Fact]
    public void Sanitize_WithControlCharacters_DropsThem()
    {
        EmailHeaderSanitizer.Sanitize("Shel\u0001ter\u0007").Should().Be("Shelter");
    }

    [Fact]
    public void Sanitize_WithValueLongerThanTheLimit_TruncatesIt()
    {
        EmailHeaderSanitizer.Sanitize(new string('x', 50), 10).Should().Be(new string('x', 10));
    }
}
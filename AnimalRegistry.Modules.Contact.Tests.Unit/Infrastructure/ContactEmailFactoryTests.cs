using AnimalRegistry.Modules.Contact.Domain;
using AnimalRegistry.Modules.Contact.Infrastructure.Email;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace AnimalRegistry.Modules.Contact.Tests.Unit.Infrastructure;

public sealed class ContactEmailFactoryTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);

    private static readonly EmailSettings Settings = new()
    {
        Host = "smtp.gmail.com",
        Port = 587,
        FromAddress = "noreply@shelter.example",
        FromDisplayName = "MojeSchronisko",
        ContactRecipient = "team@shelter.example",
    };

    private readonly ContactEmailFactory _factory = new(Options.Create(Settings));

    private static ContactRequest Request(
        string shelterName = "Riverside Animal Shelter",
        string contactPerson = "Alex Morgan",
        string email = "alex@riverside-shelter.example",
        string? phone = null,
        string? message = null)
    {
        return ContactRequest.Create(shelterName, contactPerson, email, phone, message, Now);
    }

    [Fact]
    public void Create_AddressesTheTeamMailboxAndRepliesToTheSubmitter()
    {
        var message = _factory.Create(Request());

        message.To.Should().Be("team@shelter.example");
        message.ReplyTo.Should().Be("alex@riverside-shelter.example");
        message.ReplyToDisplayName.Should().Be("Alex Morgan");
    }

    [Fact]
    public void Create_WithLineBreaksInTheShelterName_KeepsThemOutOfTheSubject()
    {
        var message = _factory.Create(Request("Shelter\r\nBcc: bot@evil.pl"));

        message.Subject.Should().NotContain("\r").And.NotContain("\n");
        message.Subject.Should().Be("[MojeSchronisko] Inquiry: Shelter Bcc: bot@evil.pl");
    }

    [Fact]
    public void Create_WithAnUnparsableEmail_OmitsTheReplyToHeader()
    {
        var message = _factory.Create(Request(email: "not-an-address"));

        message.ReplyTo.Should().BeNull();
        message.ReplyToDisplayName.Should().BeNull();
    }

    [Fact]
    public void Create_WithHtmlInTheMessage_EscapesItInTheHtmlBody()
    {
        var message = _factory.Create(Request(message: "<script>alert('xss')</script>"));

        message.HtmlBody.Should().NotContain("<script>");
        message.HtmlBody.Should().Contain("&lt;script&gt;");
    }

    [Fact]
    public void Create_WithAVeryLongShelterName_KeepsTheSubjectWithinTheHeaderLimit()
    {
        var message = _factory.Create(Request(new string('x', 500)));

        message.Subject.Length.Should().BeLessThanOrEqualTo(200);
    }
}
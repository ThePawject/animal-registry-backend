using FluentAssertions;
using System.Text;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure.PdfHelpers;

public static class PdfTestHelpers
{
    public static void AssertValidPdfStructure(byte[] pdfBytes)
    {
        pdfBytes.Should().NotBeNullOrEmpty();
        pdfBytes.Length.Should().BeGreaterThan(1000);

        pdfBytes[0].Should().Be(0x25);
        pdfBytes[1].Should().Be(0x50);
        pdfBytes[2].Should().Be(0x44);
        pdfBytes[3].Should().Be(0x46);

        var pdfContent = Encoding.ASCII.GetString(pdfBytes);

        pdfContent.Should().Contain("%PDF-");
        pdfContent.Should().Contain("%%EOF");
        pdfContent.Should().Contain("/Type /Catalog");
        pdfContent.Should().Contain("/Type /Pages");
        pdfContent.Should().Contain("/Length");
        pdfContent.Should().Contain("stream");
        pdfContent.Should().Contain("endstream");
    }
}
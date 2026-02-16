using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.Common;

internal abstract class ReportPdfBase
{
    static ReportPdfBase()
    {
        Settings.License = LicenseType.Community;
    }

    protected static void AddCoverPage(PageDescriptor page, string shelterId)
    {
        page.Size(PageSizes.A4);
        page.Margin(2f, Unit.Centimetre);
        page.PageColor(Colors.White);

        page.Content().PaddingTop(7f, Unit.Centimetre).AlignCenter().Column(column =>
        {
            column.Item().Text("Moje Schronisko").FontSize(48).Bold();
            column.Item().Height(0.5f, Unit.Centimetre);
            column.Item().Text($"Schronisko: {shelterId}").FontSize(18);
        });
    }

    protected static void AddReportTitle(ColumnDescriptor column, string title, string shelterId, DateTimeOffset generatedAt)
    {
        column.Item().AlignCenter().Text(title).FontSize(24).Bold();
        column.Item().AlignCenter().Text($"Schronisko: {shelterId}").FontSize(14);
        column.Item().AlignCenter().Text($"Data wygenerowania: {generatedAt:dd.MM.yyyy HH:mm}").FontSize(14);
        column.Item().Height(1f, Unit.Centimetre);
    }

    protected static void AddSectionTitle(ColumnDescriptor column, string title)
    {
        column.Item().PageBreak();
        column.Item().Text(title).FontSize(18).Bold();
    }

    protected static void AddSubsectionTitle(ColumnDescriptor column, string title)
    {
        column.Item().Height(0.5f, Unit.Centimetre);
        column.Item().Text(title).FontSize(14).Bold();
    }

    protected static void AddFooter(PageDescriptor page, DateTimeOffset generatedAt, string shelterId)
    {
        page.Footer().AlignCenter()
            .Text($"Raport wygenerowany: {generatedAt:dd.MM.yyyy HH:mm} | Moje Schronisko | {shelterId}")
            .FontSize(9);
    }

    protected static void AddPageConfiguration(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.Margin(2f, Unit.Centimetre);
        page.PageColor(Colors.White);
    }

    protected static byte[] GeneratePdfDocument(Action<IDocumentContainer> buildDocument)
    {
        var document = Document.Create(buildDocument);
        return document.GeneratePdf();
    }
}

internal static class ReportStyles
{
    public const float BorderThickness = 0.5f;
    public static string HeaderBackground => Colors.Grey.Lighten2;
    public static string BorderColor => Colors.Grey.Lighten1;
    public static string HighlightColor => Colors.Blue.Lighten4;

    public static IContainer StandardPadding(IContainer container)
    {
        return container.Padding(5);
    }

    public static IContainer HeaderStyle(IContainer container)
    {
        return container.Background(HeaderBackground).Padding(5);
    }

    public static IContainer CellStyle(IContainer container)
    {
        return container.Padding(5).BorderBottom(BorderThickness).BorderColor(BorderColor);
    }
}

internal static class ReportComponents
{
    public static void AddInfoTable(ColumnDescriptor column, Dictionary<string, string> items)
    {
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn(2);
            });

            foreach (var item in items)
            {
                table.Cell().Element(ReportStyles.CellStyle).Text(item.Key).Bold();
                table.Cell().Element(ReportStyles.CellStyle).Text(item.Value);
            }
        });
    }

    public static void AddEmptyState(ColumnDescriptor column, string message)
    {
        column.Item().Text(message).FontColor(Colors.Grey.Medium);
    }
}
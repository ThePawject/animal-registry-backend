using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.Common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;

internal sealed class OriginYearlyReportPdfService : ReportPdfBase, IOriginYearlyReportPdfService
{
    private static readonly string[] MonthHeaders =
        ["I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII"];

    public byte[] GenerateReport(OriginYearlyReportData data, DateTimeOffset generatedAt)
    {
        return GeneratePdfDocument(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);

                page.Content().Column(column =>
                {
                    AddReportTitle(
                        column,
                        $"Zestawienie roczne przyjęć wg gmin — {data.Year}",
                        data.ShelterId,
                        generatedAt);

                    if (data.Total == 0)
                    {
                        ReportComponents.AddEmptyState(column, "Brak przyjętych zwierząt w wybranym roku.");
                        return;
                    }

                    AddYearTable(column, data);
                });

                AddFooter(page, generatedAt, data.ShelterId);
            });
        });
    }

    private static void AddYearTable(ColumnDescriptor column, OriginYearlyReportData data)
    {
        column.Item().DefaultTextStyle(x => x.FontSize(9)).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);
                foreach (var _ in MonthHeaders)
                {
                    columns.RelativeColumn();
                }

                columns.RelativeColumn(1.2f);
            });

            table.Header(header =>
            {
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Gmina rozliczeniowa").Bold();
                foreach (var month in MonthHeaders)
                {
                    header.Cell().Element(ReportStyles.HeaderStyle).AlignRight().Text(month).Bold();
                }

                header.Cell().Element(ReportStyles.HeaderStyle).AlignRight().Text("Razem").Bold();
            });

            foreach (var row in data.Rows)
            {
                table.Cell().Element(ReportStyles.CellStyle).Text(row.ContractorName);
                foreach (var count in row.MonthCounts)
                {
                    table.Cell().Element(ReportStyles.CellStyle).AlignRight().Text(count.ToString());
                }

                table.Cell().Element(ReportStyles.CellStyle).AlignRight().Text(row.Total.ToString()).Bold();
            }

            table.Cell().Element(ReportStyles.HeaderStyle).Text("Razem").Bold();
            foreach (var total in data.MonthTotals)
            {
                table.Cell().Element(ReportStyles.HeaderStyle).AlignRight().Text(total.ToString()).Bold();
            }

            table.Cell().Element(ReportStyles.HeaderStyle).AlignRight().Text(data.Total.ToString()).Bold();
        });
    }
}

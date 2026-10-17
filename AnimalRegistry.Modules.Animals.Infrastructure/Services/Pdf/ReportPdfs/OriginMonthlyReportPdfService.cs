using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.Common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;

internal sealed class OriginMonthlyReportPdfService : ReportPdfBase, IOriginMonthlyReportPdfService
{
    private static readonly CultureInfo Polish = new("pl-PL");

    public byte[] GenerateReport(OriginMonthlyReportData data, DateTimeOffset generatedAt)
    {
        var monthName = Polish.DateTimeFormat.GetMonthName(data.Month);

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
                        $"Raport miesięczny pochodzenia zwierząt — {monthName} {data.Year}",
                        data.ShelterId,
                        generatedAt);

                    AddSummary(column, data);

                    if (data.TotalAnimals == 0)
                    {
                        ReportComponents.AddEmptyState(column, "Brak przyjętych zwierząt w wybranym miesiącu.");
                        return;
                    }

                    foreach (var group in data.Groups)
                    {
                        AddSubsectionTitle(column, $"{group.ContractorName} ({group.Entries.Count})");
                        AddGroupTable(column, group);
                    }
                });

                AddFooter(page, generatedAt, data.ShelterId);
            });
        });
    }

    private static void AddSummary(ColumnDescriptor column, OriginMonthlyReportData data)
    {
        column.Item().Text("Podsumowanie").FontSize(14).Bold();
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Gmina rozliczeniowa").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).AlignRight().Text("Liczba zwierząt").Bold();
            });

            foreach (var group in data.Groups)
            {
                table.Cell().Element(ReportStyles.CellStyle).Text(group.ContractorName);
                table.Cell().Element(ReportStyles.CellStyle).AlignRight().Text(group.Entries.Count.ToString());
            }

            table.Cell().Element(ReportStyles.HeaderStyle).Text("Razem").Bold();
            table.Cell().Element(ReportStyles.HeaderStyle).AlignRight().Text(data.TotalAnimals.ToString()).Bold();
        });
    }

    private static void AddGroupTable(ColumnDescriptor column, OriginContractorGroup group)
    {
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(1f, Unit.Centimetre);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(2);
                columns.RelativeColumn(3);
                columns.RelativeColumn(4);
            });

            table.Header(header =>
            {
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Lp.").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Sygnatura").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Imię").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Gatunek").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Data przyjęcia").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Sposób trafienia").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Miejsce znalezienia").Bold();
            });

            for (var i = 0; i < group.Entries.Count; i++)
            {
                var entry = group.Entries[i];
                var origin = entry.Origin;
                var foundPlace = origin.IsAssigned
                    ? $"{origin.FoundStreet}, {origin.FoundMunicipality.Name}"
                    : origin.FoundStreet;

                table.Cell().Element(ReportStyles.CellStyle).Text((i + 1).ToString());
                table.Cell().Element(ReportStyles.CellStyle).Text(entry.Animal.Signature.Value);
                table.Cell().Element(ReportStyles.CellStyle).Text(entry.Animal.Name ?? "-");
                table.Cell().Element(ReportStyles.CellStyle)
                    .Text(AnimalPdfComponents.GetSpeciesName(entry.Animal.Species));
                table.Cell().Element(ReportStyles.CellStyle)
                    .Text(OriginReportPeriod.ToLocal(entry.AdmittedOn).ToString("dd.MM.yyyy"));
                table.Cell().Element(ReportStyles.CellStyle).Text(origin.AdmissionMethodName);
                table.Cell().Element(ReportStyles.CellStyle).Text(foundPlace);
            }
        });
    }
}

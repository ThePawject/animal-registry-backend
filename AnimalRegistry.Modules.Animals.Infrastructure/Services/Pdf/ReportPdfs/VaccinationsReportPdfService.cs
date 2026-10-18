using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.Common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;

internal sealed class VaccinationsReportPdfService : ReportPdfBase, IVaccinationsReportPdfService
{
    private static readonly StringComparer PolishIgnoreCase = StringComparer.Create(new CultureInfo("pl-PL"), true);

    public byte[] GenerateReport(VaccinationsReportData data, DateTimeOffset generatedAt)
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
                        $"Raport szczepień — {data.From:dd.MM.yyyy} – {data.To:dd.MM.yyyy}",
                        data.ShelterId,
                        generatedAt);

                    if (data.Entries.Count == 0)
                    {
                        ReportComponents.AddEmptyState(column, "Brak szczepień w wybranym okresie.");
                        return;
                    }

                    AddSummary(column, data);
                    AddSubsectionTitle(column, $"Wykonane szczepienia ({data.Entries.Count})");
                    AddTable(column, data);
                });

                AddFooter(page, generatedAt, data.ShelterId);
            });
        });
    }

    private static void AddSummary(ColumnDescriptor column, VaccinationsReportData data)
    {
        var groups = data.Entries
            .GroupBy(e => e.Vaccination.VaccineName, PolishIgnoreCase)
            .Select(g => (Name: g.First().Vaccination.VaccineName, Count: g.Count()))
            .OrderByDescending(g => g.Count)
            .ThenBy(g => g.Name, PolishIgnoreCase)
            .ToList();

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
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Szczepionka").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).AlignRight().Text("Liczba szczepień").Bold();
            });

            foreach (var (name, count) in groups)
            {
                table.Cell().Element(ReportStyles.CellStyle).Text(name);
                table.Cell().Element(ReportStyles.CellStyle).AlignRight().Text(count.ToString());
            }

            table.Cell().Element(ReportStyles.HeaderStyle).Text("Razem").Bold();
            table.Cell().Element(ReportStyles.HeaderStyle).AlignRight().Text(data.Entries.Count.ToString()).Bold();
        });
    }

    private static void AddTable(ColumnDescriptor column, VaccinationsReportData data)
    {
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(1f, Unit.Centimetre);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Lp.").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Data podania").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Sygnatura").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Imię").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Gatunek").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Szczepionka").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Seria").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Lekarz weterynarii").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Ważne do").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Status").Bold();
            });

            for (var i = 0; i < data.Entries.Count; i++)
            {
                var entry = data.Entries[i];
                var vaccination = entry.Vaccination;

                table.Cell().Element(ReportStyles.CellStyle).Text((i + 1).ToString());
                table.Cell().Element(ReportStyles.CellStyle).Text(FormatDate(vaccination.AdministeredOn));
                table.Cell().Element(ReportStyles.CellStyle).Text(entry.AnimalSignature);
                table.Cell().Element(ReportStyles.CellStyle).Text(entry.AnimalName ?? "-");
                table.Cell().Element(ReportStyles.CellStyle).Text(AnimalPdfComponents.GetSpeciesName(entry.Species));
                table.Cell().Element(ReportStyles.CellStyle).Text(vaccination.VaccineName);
                table.Cell().Element(ReportStyles.CellStyle).Text(vaccination.BatchNumber ?? "-");
                table.Cell().Element(ReportStyles.CellStyle).Text(vaccination.Veterinarian ?? "-");
                table.Cell().Element(ReportStyles.CellStyle).Text(vaccination.ValidUntil is { } validUntil
                    ? FormatDate(validUntil)
                    : "-");
                table.Cell().Element(ReportStyles.CellStyle).Text(GetStatusName(vaccination.Status));
            }
        });
    }

    private static string GetStatusName(VaccinationStatus status)
    {
        return status switch
        {
            VaccinationStatus.Replaced => "Zastąpione",
            VaccinationStatus.Expired => "Wygasłe",
            _ => "Aktywne",
        };
    }

    private static string FormatDate(DateTimeOffset value)
    {
        return ShelterCalendar.ToLocal(value).ToString("dd.MM.yyyy");
    }
}

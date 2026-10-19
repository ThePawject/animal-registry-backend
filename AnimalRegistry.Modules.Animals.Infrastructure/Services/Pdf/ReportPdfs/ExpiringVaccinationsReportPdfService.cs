using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.Common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;

internal sealed class ExpiringVaccinationsReportPdfService : ReportPdfBase, IExpiringVaccinationsReportPdfService
{
    public byte[] GenerateReport(ExpiringVaccinationsReportData data, DateTimeOffset generatedAt)
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
                    AddReportTitle(column, "Kończące się szczepienia", data.ShelterId, generatedAt);

                    ReportComponents.AddInfoTable(column, new Dictionary<string, string>
                    {
                        { "Kończą się w ciągu", $"{data.ExpiresInDays} dni" },
                        { "Po terminie", data.IncludeExpired ? "Uwzględnione" : "Pominięte" },
                        { "Zwierzęta", data.InShelterOnly ? "Tylko w schronisku" : "Wszystkie" },
                        { "Liczba szczepień", data.Entries.Count.ToString() },
                    });

                    column.Item().Height(0.5f, Unit.Centimetre);

                    if (data.Entries.Count == 0)
                    {
                        ReportComponents.AddEmptyState(column, "Brak szczepień spełniających kryteria.");
                        return;
                    }

                    AddTable(column, data);
                });

                AddFooter(page, generatedAt, data.ShelterId);
            });
        });
    }

    private static void AddTable(ColumnDescriptor column, ExpiringVaccinationsReportData data)
    {
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(1f, Unit.Centimetre);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(3);
            });

            table.Header(header =>
            {
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Lp.").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Sygnatura").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Imię").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Gatunek").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Szczepionka").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Data podania").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Ważne do").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Termin").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Lekarz weterynarii").Bold();
            });

            for (var i = 0; i < data.Entries.Count; i++)
            {
                var entry = data.Entries[i];
                var vaccination = entry.Vaccination;

                table.Cell().Element(ReportStyles.CellStyle).Text((i + 1).ToString());
                table.Cell().Element(ReportStyles.CellStyle).Text(entry.AnimalSignature);
                table.Cell().Element(ReportStyles.CellStyle).Text(entry.AnimalName ?? "-");
                table.Cell().Element(ReportStyles.CellStyle).Text(AnimalPdfComponents.GetSpeciesName(entry.Species));
                table.Cell().Element(ReportStyles.CellStyle).Text(vaccination.VaccineName);
                table.Cell().Element(ReportStyles.CellStyle).Text(FormatDate(vaccination.AdministeredOn));
                table.Cell().Element(ReportStyles.CellStyle).Text(vaccination.ValidUntil is { } validUntil
                    ? FormatDate(validUntil)
                    : "-");
                table.Cell().Element(ReportStyles.CellStyle).Text(FormatRemaining(vaccination.DaysUntilExpiry));
                table.Cell().Element(ReportStyles.CellStyle).Text(vaccination.Veterinarian ?? "-");
            }
        });
    }

    private static string FormatDate(DateTimeOffset value)
    {
        return ShelterCalendar.ToLocal(value).ToString("dd.MM.yyyy");
    }

    private static string FormatRemaining(int? daysUntilExpiry)
    {
        return daysUntilExpiry switch
        {
            null => "-",
            < 0 => $"Po terminie ({-daysUntilExpiry} dni)",
            0 => "Dziś",
            _ => $"Za {daysUntilExpiry} dni",
        };
    }
}

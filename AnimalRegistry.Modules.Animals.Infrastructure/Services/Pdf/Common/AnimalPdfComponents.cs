using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalHealths;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.Common;

internal static class AnimalPdfComponents
{
    public static void AddAnimalSection(ColumnDescriptor column, Animal animal, int index, int total)
    {
        if (index > 0)
        {
            column.Item().PageBreak();
        }

        column.Item().Text($"{animal.Name}").FontSize(16).Bold();
        column.Item().Height(0.3f, Unit.Centimetre);

        AddAnimalBasicInfo(column, animal);

        if (animal.Events.Any())
        {
            column.Item().Height(0.5f, Unit.Centimetre);
            AddAnimalEventsTable(column, animal.Events);
        }

        if (animal.HealthRecords.Any())
        {
            column.Item().Height(0.5f, Unit.Centimetre);
            AddAnimalHealthTable(column, animal.HealthRecords);
        }

        if (animal.Photos.Any())
        {
            column.Item().Height(0.5f, Unit.Centimetre);
            AddAnimalPhotosInfo(column, animal.Photos, animal.MainPhotoId);
        }
    }

    private static void AddAnimalBasicInfo(ColumnDescriptor column, Animal animal)
    {
        var info = new Dictionary<string, string>
        {
            { "ID", animal.Id.ToString() },
            { "Sygnatura", animal.Signature.Value },
            { "Kod transpondera", string.IsNullOrEmpty(animal.TransponderCode) ? "-" : animal.TransponderCode },
            { "Gatunek", GetSpeciesName(animal.Species) },
            { "Płeć", GetSexName(animal.Sex) },
            { "Kolor", animal.Color },
            { "Data urodzenia", animal.BirthDate.ToString("dd.MM.yyyy") },
            { "W schronisku", animal.IsInShelter ? "Tak" : "Nie" },
            { "Data utworzenia", animal.CreatedOn.ToString("dd.MM.yyyy HH:mm") },
            { "Ostatnia modyfikacja", animal.ModifiedOn.ToString("dd.MM.yyyy HH:mm") },
        };

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn(2);
            });

            foreach (var item in info)
            {
                table.Cell().Element(ReportStyles.CellStyle).Text(item.Key).Bold();
                table.Cell().Element(ReportStyles.CellStyle).Text(item.Value);
            }
        });
    }

    private static void AddAnimalEventsTable(ColumnDescriptor column, IEnumerable<AnimalEvent> events)
    {
        column.Item().Text("Zdarzenia").FontSize(12).Bold();

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Data").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Typ").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Wykonane przez").Bold();
            });

            foreach (var evt in events.OrderByDescending(e => e.OccurredOn))
            {
                table.Cell().Element(ReportStyles.CellStyle).Text(evt.OccurredOn.ToString("dd.MM.yyyy"));
                table.Cell().Element(ReportStyles.CellStyle).Text(GetEventTypeName(evt.Type));
                table.Cell().Element(ReportStyles.CellStyle).Text(evt.PerformedBy);
            }
        });
    }

    private static void AddAnimalHealthTable(ColumnDescriptor column, IEnumerable<AnimalHealth> healthRecords)
    {
        column.Item().Text("Dane zdrowotne").FontSize(12).Bold();

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Data").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Opis").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Wykonane przez").Bold();
            });

            foreach (var record in healthRecords.OrderByDescending(h => h.OccurredOn))
            {
                table.Cell().Element(ReportStyles.CellStyle).Text(record.OccurredOn.ToString("dd.MM.yyyy"));
                table.Cell().Element(ReportStyles.CellStyle).Text(record.Description);
                table.Cell().Element(ReportStyles.CellStyle).Text(record.PerformedBy);
            }
        });
    }

    private static void AddAnimalPhotosInfo(ColumnDescriptor column, IEnumerable<AnimalPhoto> photos, Guid? mainPhotoId)
    {
        var photoList = photos.ToList();
        var photoCount = photoList.Count;
        column.Item().Text($"Zdjęcia: {photoCount}").FontSize(12).Bold();

        if (photoCount > 0)
        {
            column.Item().Text("Lista zdjęć:").FontSize(10);

            foreach (var photo in photoList)
            {
                var isMain = mainPhotoId.HasValue && photo.Id == mainPhotoId.Value;
                var mainIndicator = isMain ? " (główne)" : "";
                column.Item().Text($"  • {photo.FileName}{mainIndicator} - dodano {photo.UploadedOn:dd.MM.yyyy}")
                    .FontSize(9);
            }
        }
    }

    public static string GetSpeciesName(AnimalSpecies species)
    {
        return species switch
        {
            AnimalSpecies.Dog => "Pies",
            AnimalSpecies.Cat => "Kot",
            _ => species.ToString(),
        };
    }

    public static string GetSexName(AnimalSex sex)
    {
        return sex switch
        {
            AnimalSex.Male => "Samiec",
            AnimalSex.Female => "Samica",
            _ => sex.ToString(),
        };
    }

    public static string GetEventTypeName(AnimalEventType eventType)
    {
        return eventType switch
        {
            AnimalEventType.None => "Brak",
            AnimalEventType.AdmissionToShelter => "Przyjęcie do schroniska",
            AnimalEventType.StartOfQuarantine => "Rozpoczęcie kwarantanny",
            AnimalEventType.EndOfQuarantine => "Zakończenie kwarantanny",
            AnimalEventType.InfectiousDiseaseVaccination => "Szczepienie przeciw chorobom zakaźnym",
            AnimalEventType.Deworming => "Odrobaczenie",
            AnimalEventType.Defleaing => "Odpluskwienie",
            AnimalEventType.Sterilization => "Sterylizacja/Kastracja",
            AnimalEventType.RabiesVaccination => "Szczepienie przeciw wściekliźnie",
            AnimalEventType.Adoption => "Adopcja",
            AnimalEventType.Walk => "Spacer",
            AnimalEventType.NewKennelNumber => "Nowy numer kojca",
            AnimalEventType.PickedUpByOwner => "Odbiór przez właściciela",
            AnimalEventType.Weighing => "Ważenie",
            AnimalEventType.Euthanasia => "Eutanazja",
            AnimalEventType.Death => "Śmierć",
            _ => eventType.ToString(),
        };
    }
}
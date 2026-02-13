using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.Common;

internal static class PdfImageHelper
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    public static byte[]? DownloadImage(string url)
    {
        try
        {
            return HttpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
        }
        catch
        {
            return null;
        }
    }

    public static void AddPhotoGrid(ColumnDescriptor column, IEnumerable<string> imageUrls)
    {
        var urls = imageUrls.ToList();
        if (urls.Count == 0) return;

        column.Item().Text($"Zdjęcia: {urls.Count}").FontSize(12).Bold();
        column.Item().Height(0.3f, Unit.Centimetre);

        var images = urls.Select(DownloadImage).Where(img => img != null).ToList();

        if (images.Count == 0)
        {
            column.Item().Text("Nie udało się pobrać zdjęć").FontSize(10).FontColor(Colors.Grey.Medium);
            return;
        }

        for (var i = 0; i < images.Count; i += 3)
        {
            var rowImages = images.Skip(i).Take(3).ToList();

            column.Item().Row(row =>
            {
                foreach (var imageBytes in rowImages)
                {
                    row.RelativeItem().Padding(2).AlignCenter().AlignMiddle().Height(5f, Unit.Centimetre).Width(5f, Unit.Centimetre)
                        .Image(imageBytes!)
                        .FitArea();
                }

                for (var j = rowImages.Count; j < 3; j++)
                {
                    row.RelativeItem();
                }
            });
        }
    }
}

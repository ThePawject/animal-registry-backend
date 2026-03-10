using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

public static class TestImageHelper
{
    public static byte[] CreateTestImage(int width = 100, int height = 100)
    {
        using var image = new Image<Rgba32>(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var color = new Rgba32((byte)(x % 256), (byte)(y % 256), 128);
                image[x, y] = color;
            }
        }

        using var stream = new MemoryStream();
        image.SaveAsJpeg(stream, new JpegEncoder());
        return stream.ToArray();
    }
}
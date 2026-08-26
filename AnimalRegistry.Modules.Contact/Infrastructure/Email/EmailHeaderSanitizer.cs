using System.Text;

namespace AnimalRegistry.Modules.Contact.Infrastructure.Email;

public static class EmailHeaderSanitizer
{
    public static string Sanitize(string? value, int maxLength = 200)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLength);

        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (character is '\r' or '\n' or '\t')
            {
                builder.Append(' ');
                continue;
            }

            if (char.IsControl(character))
            {
                continue;
            }

            builder.Append(character);
        }

        var sanitized = CollapseWhitespace(builder.ToString()).Trim();

        return sanitized.Length <= maxLength ? sanitized : sanitized[..maxLength];
    }

    private static string CollapseWhitespace(string value)
    {
        var builder = new StringBuilder(value.Length);
        var previousWasSpace = false;

        foreach (var character in value)
        {
            var isSpace = character == ' ';
            if (isSpace && previousWasSpace)
            {
                continue;
            }

            builder.Append(character);
            previousWasSpace = isSpace;
        }

        return builder.ToString();
    }
}
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.DDD;
using System.Text.RegularExpressions;

namespace AnimalRegistry.Modules.Animals.Domain.Animals;

public sealed class AnimalSignature : ValueObject
{
    private static readonly Regex SignaturePattern = new(
        @"^(\d{4})/(\d{4})$",
        RegexOptions.Compiled);

    private AnimalSignature(int year, int number)
    {
        Year = year;
        Number = number;
        Value = $"{Year:D4}/{Number:D4}";
    }

    public int Year { get; }
    public int Number { get; }
    public string Value { get; }

    public static Result<AnimalSignature> Create(string signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            return Result<AnimalSignature>.Failure("Signature cannot be empty.");
        }

        var match = SignaturePattern.Match(signature.Trim());
        if (!match.Success)
        {
            return Result<AnimalSignature>.Failure(
                "Invalid signature format. Expected format: YYYY/NNNN (e.g., 2026/0001).");
        }

        var year = int.Parse(match.Groups[1].Value);
        var number = int.Parse(match.Groups[2].Value);

        if (year is < 2000 or > 2100)
        {
            return Result<AnimalSignature>.Failure("Year must be between 2000 and 2100.");
        }

        if (number is < 1 or > 9999)
        {
            return Result<AnimalSignature>.Failure("Number must be between 0001 and 9999.");
        }

        return Result<AnimalSignature>.Success(new AnimalSignature(year, number));
    }

    public static AnimalSignature CreateForYear(int year, int number)
    {
        if (year is < 2000 or > 2100)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 2000 and 2100.");
        }

        return number is < 1 or > 9999
            ? throw new ArgumentOutOfRangeException(nameof(number), "Number must be between 1 and 9999.")
            : new AnimalSignature(year, number);
    }

    public override string ToString()
    {
        return Value;
    }
}
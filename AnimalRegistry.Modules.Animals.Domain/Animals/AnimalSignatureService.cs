namespace AnimalRegistry.Modules.Animals.Domain.Animals;

public interface IAnimalSignatureService
{
    Task<AnimalSignature> GetNextAvailableSignatureAsync(int year, string shelterId, CancellationToken cancellationToken = default);
    Task<bool> IsSignatureUniqueAsync(string signature, string shelterId, Guid? excludeAnimalId = null, CancellationToken cancellationToken = default);
}

internal sealed class AnimalSignatureService(IAnimalRepository animalRepository) : IAnimalSignatureService
{
    public async Task<AnimalSignature> GetNextAvailableSignatureAsync(int year, string shelterId, CancellationToken cancellationToken = default)
    {
        var existingNumbers = await animalRepository.GetExistingNumbersForYearAsync(year, shelterId, cancellationToken);
        
        var nextNumber = FindLowestAvailableNumber(existingNumbers);
        
        return AnimalSignature.CreateForYear(year, nextNumber);
    }

    public async Task<bool> IsSignatureUniqueAsync(string signature, string shelterId, Guid? excludeAnimalId = null, CancellationToken cancellationToken = default)
    {
        return await animalRepository.IsSignatureUniqueAsync(signature, shelterId, excludeAnimalId, cancellationToken);
    }

    private static int FindLowestAvailableNumber(IEnumerable<int> existingNumbers)
    {
        var sortedNumbers = existingNumbers.OrderBy(n => n).ToList();
        
        var nextNumber = 1;
        foreach (var number in sortedNumbers.TakeWhile(number => number <= nextNumber))
        {
            nextNumber = number + 1;
        }
        
        return nextNumber > 9999 ? throw new InvalidOperationException("No available signature numbers for this year. Maximum 9999 reached.") : nextNumber;
    }
}

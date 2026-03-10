using AnimalRegistry.Shared;

namespace AnimalRegistry.Modules.Animals.Application;

public interface IImageOptimizationService
{
    Task<Result<Stream>> OptimizeImageAsync(Stream sourceStream, CancellationToken cancellationToken = default);
}
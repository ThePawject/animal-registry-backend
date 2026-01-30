using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

public class GetAllAnimalsQueryHandler(IAnimalsRepository animalsRepository)
    : IRequestHandler<GetAllAnimalsQuery, Result<GetAllAnimalsQueryResponse>>
{
    public async Task<Result<GetAllAnimalsQueryResponse>> Handle(GetAllAnimalsQuery request,
        CancellationToken cancellationToken)
    {
        var animals = await animalsRepository.GetAllAsync();
        return Result<GetAllAnimalsQueryResponse>.Success(new GetAllAnimalsQueryResponse(animals));
    }
}
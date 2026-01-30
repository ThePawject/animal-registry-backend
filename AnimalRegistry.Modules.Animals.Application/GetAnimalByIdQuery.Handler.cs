using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

public class GetAnimalByIdQueryHandler(IAnimalsRepository animalsRepository)
    : IRequestHandler<GetAnimalByIdQuery, Result<GetAnimalByIdQueryResponse>>
{
    public async Task<Result<GetAnimalByIdQueryResponse>> Handle(GetAnimalByIdQuery request,
        CancellationToken cancellationToken)
    {
        var animal = await animalsRepository.GetAsync(request.Id);
        if (animal is null)
            return Result<GetAnimalByIdQueryResponse>.NotFound();

        return Result<GetAnimalByIdQueryResponse>.Success(new GetAnimalByIdQueryResponse(animal));
    }
}
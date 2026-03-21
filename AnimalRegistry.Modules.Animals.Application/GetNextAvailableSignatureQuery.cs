using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed class GetNextAvailableSignatureQuery(int year, AnimalSpecies species)
    : IRequest<Result<GetNextAvailableSignatureResponse>>
{
    public int Year { get; } = year;
    public AnimalSpecies Species { get; } = species;
}

public sealed record GetNextAvailableSignatureResponse(string Signature);

internal sealed class GetNextAvailableSignatureQueryHandler(
    IAnimalSignatureService signatureService,
    ICurrentUser currentUser)
    : IRequestHandler<GetNextAvailableSignatureQuery, Result<GetNextAvailableSignatureResponse>>
{
    public async Task<Result<GetNextAvailableSignatureResponse>> Handle(
        GetNextAvailableSignatureQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Year is < 2000 or > 2100)
        {
            return Result<GetNextAvailableSignatureResponse>.ValidationError("Year must be between 2000 and 2100.");
        }

        var signature = await signatureService.GetNextAvailableSignatureAsync(
            request.Year,
            currentUser.ShelterId,
            request.Species,
            cancellationToken);

        return Result<GetNextAvailableSignatureResponse>.Success(
            new GetNextAvailableSignatureResponse(signature.Value));
    }
}
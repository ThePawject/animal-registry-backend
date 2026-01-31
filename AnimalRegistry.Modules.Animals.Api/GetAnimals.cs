using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api;

public class GetAnimals : Endpoint<EmptyRequest, string>
{
    public override void Configure()
    {
        Get(GetAnimalsRequest.Route);
        AllowAnonymous();
    }
    
    public override Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        Response = "doggo";

        return Task.CompletedTask;
    }
}
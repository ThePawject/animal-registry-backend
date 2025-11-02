using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Accounts.Api;

public class MyEndpoint(IMediator mediator) : Endpoint<MyRequest, MyResponse>
{
    public override void Configure()
    {
        Post("/hello/world");
        AllowAnonymous();
    }

    public override async Task HandleAsync(MyRequest req, CancellationToken ct)
    {
        var query = new GetWelcomeMessageQuery
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
        };

        var result = await mediator.Send(query, ct);

        var response = new MyResponse
        {
            FullName = result.FullName,
            Message = result.Message,
        };

        await Send.OkAsync(response, ct);
    }
}

public class GetWelcomeMessageQuery : IRequest<GetWelcomeMessageQueryResponse>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}

public class GetWelcomeMessageQueryResponse
{
    public required string FullName { get; set; }
    public required string Message { get; set; }
}

public class GetWelcomeMessageQueryHandler : IRequestHandler<GetWelcomeMessageQuery, GetWelcomeMessageQueryResponse>
{
    public Task<GetWelcomeMessageQueryResponse> Handle(GetWelcomeMessageQuery request,
        CancellationToken cancellationToken)
    {
        var response = new GetWelcomeMessageQueryResponse
        {
            FullName = $"{request.FirstName} {request.LastName}",
            Message = "Welcome to FastEndpoints...",
        };

        return Task.FromResult(response);
    }
}
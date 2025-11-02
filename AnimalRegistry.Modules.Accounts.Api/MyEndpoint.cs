using FastEndpoints;

namespace AnimalRegistry.Modules.Accounts.Api;

public class MyEndpoint : Endpoint<MyRequest, MyResponse>
{
    public override void Configure()
    {
        Post("/hello/world");
        AllowAnonymous();
    }

    public override async Task HandleAsync(MyRequest r, CancellationToken c)
    {
        await Send.OkAsync(new()
        {
            FullName = $"{r.FirstName} {r.LastName}",
            Message = "Welcome to FastEndpoints..."				
        });
    }
}
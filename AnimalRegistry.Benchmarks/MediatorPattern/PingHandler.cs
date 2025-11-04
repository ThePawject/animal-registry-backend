namespace AnimalRegistry.Benchmarks.MediatorPattern;

public class Ping : IRequest<string>
{
}

public class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
    {
        return Task.FromResult("Pong");
    }
}
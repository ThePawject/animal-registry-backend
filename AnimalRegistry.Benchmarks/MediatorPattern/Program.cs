using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalRegistry.Benchmarks.MediatorPattern;

[MemoryDiagnoser]
public class MediatorBenchmarks(IMediator mediatorCached, IMediator mediatorDynamic, IServiceProvider serviceProvider)
{
    private readonly Ping _pingRequest = new();
    private IMediator _mediatorCached = mediatorCached;
    private IMediator _mediatorDynamic = mediatorDynamic;
    private IServiceProvider _serviceProvider = serviceProvider;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IRequestHandler<Ping, string>, PingHandler>();
        _serviceProvider = services.BuildServiceProvider();

        _mediatorDynamic = new MediatorDynamic(_serviceProvider);
        _mediatorCached = new MediatorCached(_serviceProvider);
    }

    [Benchmark(Baseline = true)]
    public Task<string> DynamicMediator()
    {
        return _mediatorDynamic.Send(_pingRequest);
    }

    [Benchmark]
    public Task<string> CachedMediator()
    {
        return _mediatorCached.Send(_pingRequest);
    }
}

public class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<MediatorBenchmarks>();
    }
}
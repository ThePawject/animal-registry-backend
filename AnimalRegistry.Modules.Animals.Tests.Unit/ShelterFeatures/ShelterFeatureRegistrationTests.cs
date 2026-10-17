using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Shared.ShelterFeatures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.ShelterFeatures;

public sealed class ShelterFeatureRegistrationTests
{
    private sealed class EnabledToggles : IShelterFeatureToggles
    {
        public Task<bool> IsEnabledAsync(string featureKey, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<IReadOnlySet<string>> GetEnabledAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlySet<string>>(new HashSet<string> { AnimalOriginFeature.FeatureKey });
        }
    }

    private static async Task<bool> IsOriginEnabledAsync(IServiceCollection services)
    {
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IShelterFeatureToggles>()
            .IsEnabledAsync(AnimalOriginFeature.FeatureKey);
    }

    [Fact]
    public async Task AddShelterFeature_ShouldTreatEveryFeatureAsDisabled_WhenNoTogglesAreRegistered()
    {
        var services = new ServiceCollection().AddShelterFeature<AnimalOriginFeature>();

        (await IsOriginEnabledAsync(services)).Should().BeFalse();
    }

    [Fact]
    public async Task AddShelterFeatureToggles_ShouldWin_WhenRegisteredAfterFeatures()
    {
        var services = new ServiceCollection()
            .AddShelterFeature<AnimalOriginFeature>()
            .AddShelterFeatureToggles<EnabledToggles>();

        (await IsOriginEnabledAsync(services)).Should().BeTrue();
        services.Count(d => d.ServiceType == typeof(IShelterFeatureToggles)).Should().Be(1);
    }

    [Fact]
    public async Task AddShelterFeatureToggles_ShouldWin_WhenRegisteredBeforeFeatures()
    {
        var services = new ServiceCollection()
            .AddShelterFeatureToggles<EnabledToggles>()
            .AddShelterFeature<AnimalOriginFeature>();

        (await IsOriginEnabledAsync(services)).Should().BeTrue();
        services.Count(d => d.ServiceType == typeof(IShelterFeatureToggles)).Should().Be(1);
    }
}

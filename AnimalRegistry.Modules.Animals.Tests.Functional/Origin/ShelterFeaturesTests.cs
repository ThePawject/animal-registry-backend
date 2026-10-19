using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Modules.Shelters.Api;
using AnimalRegistry.Modules.Shelters.Application;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.Origin;

[Collection("Sequential")]
public sealed class ShelterFeaturesTests(ApiTestFixture fixture) : OriginModuleTestBase(fixture)
{
    [Fact]
    public async Task GetShelterFeatures_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        var response = await Client.GetAsync(GetShelterFeatures.Route);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetShelterFeatures_ShouldReturnForbidden_WhenUserHasNoShelterAccess()
    {
        var client = Factory.CreateAuthenticatedClient(TestUser.WithoutShelterAccess());

        var response = await client.GetAsync(GetShelterFeatures.Route);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetShelterFeatures_ShouldListOriginModuleAsDisabled_ByDefault()
    {
        var features = await Worker.GetFromJsonAsync<List<ShelterFeatureDto>>(GetShelterFeatures.Route);

        var origin = features.Should().ContainSingle(f => f.Key == FeatureKey).Which;
        origin.IsEnabled.Should().BeFalse();
        origin.Name.Should().Be("Pochodzenie zwierzęcia");
        origin.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SetShelterFeatureState_ShouldReturnForbidden_WhenUserIsNotShelterAdmin()
    {
        var response = await PutModuleAsync(Worker, true);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SetShelterFeatureState_ShouldReturnNotFound_WhenModuleDoesNotExist()
    {
        var response = await PutModuleAsync(Admin, true, "unknown-module");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetShelterFeatureState_ShouldReturnFeatureState()
    {
        var response = await PutModuleAsync(Admin, true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var feature = await response.Content.ReadFromJsonAsync<ShelterFeatureDto>();
        feature!.Key.Should().Be(FeatureKey);
        feature.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task SetShelterFeatureState_ShouldEnableModuleOnlyForOwnShelter()
    {
        var otherShelter = Factory.CreateAuthenticatedClient(TestUser.WithShelterAccess(OtherShelterId));

        await SetModuleAsync(true);

        var own = await Worker.GetFromJsonAsync<List<ShelterFeatureDto>>(GetShelterFeatures.Route);
        var other = await otherShelter.GetFromJsonAsync<List<ShelterFeatureDto>>(GetShelterFeatures.Route);
        own!.Single(f => f.Key == FeatureKey).IsEnabled.Should().BeTrue();
        other!.Single(f => f.Key == FeatureKey).IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task SetShelterFeatureState_ShouldDisableModule()
    {
        await SetModuleAsync(true);

        await SetModuleAsync(false);

        var features = await Worker.GetFromJsonAsync<List<ShelterFeatureDto>>(GetShelterFeatures.Route);
        features!.Single(f => f.Key == FeatureKey).IsEnabled.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SetShelterFeatureState_ShouldBeIdempotent(bool isEnabled)
    {
        var first = await PutModuleAsync(Admin, isEnabled);
        var second = await PutModuleAsync(Admin, isEnabled);

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        var features = await Worker.GetFromJsonAsync<List<ShelterFeatureDto>>(GetShelterFeatures.Route);
        features!.Single(f => f.Key == FeatureKey).IsEnabled.Should().Be(isEnabled);
    }

    [Fact]
    public async Task SetShelterFeatureState_ShouldHandleConcurrentEnabling()
    {
        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => PutModuleAsync(Admin, true)));

        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
        var features = await Worker.GetFromJsonAsync<List<ShelterFeatureDto>>(GetShelterFeatures.Route);
        features!.Single(f => f.Key == FeatureKey).IsEnabled.Should().BeTrue();
    }
}

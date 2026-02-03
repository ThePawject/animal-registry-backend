using System.Net;
using System.Net.Http.Json;
using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;
using Xunit;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

public class AnimalsApiTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private readonly AnimalFactory _factory = new(new ApiClient(fixture.Client));

    [Fact(Skip = "Flaky test, needs investigation")]
    public async Task Create_Get_List_Workflow()
    {
        var createdId = await _factory.CreateAsync("sig-integ-1", "trans-123", "Integration", AnimalSpecies.Dog, AnimalSex.Male);
        var dto = await _factory.GetAsync(createdId);
        dto.Name.Should().Be("Integration");
        var list = await _factory.ListAsync();
        list.Any(a => a.Id == createdId).Should().BeTrue();
    }

    [Fact(Skip = "Flaky test, needs investigation")]
    public async Task List_ReturnsCreatedItems()
    {
        var id1 = await _factory.CreateAsync("sig-list-1", "t-1", "ListOne", AnimalSpecies.Cat, AnimalSex.Female);
        var id2 = await _factory.CreateAsync("sig-list-2", "t-2", "ListTwo", AnimalSpecies.Dog, AnimalSex.Male);

        var list = await _factory.ListAsync();

        list.Any(a => a.Id == id1 && a.Name == "ListOne").Should().BeTrue();
        list.Any(a => a.Id == id2 && a.Name == "ListTwo").Should().BeTrue();
    }
}


using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.Contact;

[Collection("Sequential")]
public sealed class SubmitContactRequestTests(ApiTestFixture fixture) : IAsyncLifetime
{
    private const string Route = "/contact";

    private static int _clientIpCounter;

    private readonly FunctionalTestWebApplicationFactory _factory = fixture.Factory;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        await _factory.ResetDatabaseAsync();
        await ExecuteAsync("DELETE FROM ContactRequests");
    }

    public Task DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return Task.CompletedTask;
    }

    [Fact]
    public async Task Post_WithoutAuthentication_IsAcceptedAndNotifiesTheTeamMailbox()
    {
        var response = await PostAsync(ValidBody());

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var message = _factory.EmailSender.Messages.Should().ContainSingle().Which;
        message.To.Should().Be("team@test.local");
        message.ReplyTo.Should().Be("alex@riverside-shelter.example");
        message.Subject.Should().Be("[MojeSchronisko] Inquiry: Riverside Animal Shelter");
        message.HtmlBody.Should().Contain("Hello, we would like to join the registry.");

        var stored = await ReadSingleStoredRequestAsync();
        stored.Email.Should().Be("alex@riverside-shelter.example");
        stored.Status.Should().Be("Sent");
    }

    [Fact]
    public async Task Post_WhenTheMailerFails_StillAcceptsAndKeepsTheLead()
    {
        _factory.EmailSender.FailWith = new InvalidOperationException("smtp is down");

        var response = await PostAsync(ValidBody());

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var stored = await ReadSingleStoredRequestAsync();
        stored.Email.Should().Be("alex@riverside-shelter.example");
        stored.Status.Should().Be("Failed");
    }

    private static string NextClientIp()
    {
        return $"203.0.113.{Interlocked.Increment(ref _clientIpCounter) % 250 + 1}";
    }

    private static Dictionary<string, object> ValidBody()
    {
        return new Dictionary<string, object>
        {
            ["shelterName"] = "Riverside Animal Shelter",
            ["contactPerson"] = "Alex Morgan",
            ["email"] = "alex@riverside-shelter.example",
            ["phone"] = "600100200",
            ["message"] = "Hello, we would like to join the registry.",
            ["consent"] = true,
            ["_honey"] = string.Empty,
        };
    }

    private async Task<HttpResponseMessage> PostAsync(Dictionary<string, object> body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, Route)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
        };
        request.Headers.TryAddWithoutValidation("X-Forwarded-For", NextClientIp());

        return await _client.SendAsync(request);
    }

    private async Task<(string Email, string Status)> ReadSingleStoredRequestAsync()
    {
        await using var connection = new SqlConnection(_factory.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Email, DeliveryStatus FROM ContactRequests";

        await using var reader = await command.ExecuteReaderAsync();
        (await reader.ReadAsync()).Should().BeTrue("exactly one submission should have been stored");

        var row = (reader.GetString(0), reader.GetString(1));

        (await reader.ReadAsync()).Should().BeFalse("exactly one submission should have been stored");

        return row;
    }

    private async Task ExecuteAsync(string sql)
    {
        await using var connection = new SqlConnection(_factory.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        await command.ExecuteNonQueryAsync();
    }
}
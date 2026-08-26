using System.Text.Json.Serialization;

namespace AnimalRegistry.Modules.Contact.Api;

public sealed class SubmitContactRequestBody
{
    public const string Route = "/contact";

    public string ShelterName { get; init; } = string.Empty;
    public string ContactPerson { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool Consent { get; init; }

    [JsonPropertyName("_honey")]
    public string Honey { get; init; } = string.Empty;
}
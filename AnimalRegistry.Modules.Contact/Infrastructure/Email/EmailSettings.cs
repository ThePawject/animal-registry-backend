namespace AnimalRegistry.Modules.Contact.Infrastructure.Email;

public sealed class EmailSettings
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; } = true;

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public bool UseImplicitTls { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string FromAddress { get; set; } = string.Empty;

    public string FromDisplayName { get; set; } = "MojeSchronisko";

    public string ContactRecipient { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;
}
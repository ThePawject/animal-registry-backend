namespace AnimalRegistry.Modules.Contact.RateLimiting;

public sealed class ContactRateLimitSettings
{
    public const string SectionName = "Contact:RateLimit";

    public int PermitLimit { get; set; } = 5;

    public int WindowMinutes { get; set; } = 15;
}
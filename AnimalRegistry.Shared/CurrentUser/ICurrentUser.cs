namespace AnimalRegistry.Shared.CurrentUser;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    string? Email { get; }
}
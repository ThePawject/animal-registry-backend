namespace AnimalRegistry.Shared.CurrentUser;

public interface ICurrentUser
{
    string UserId { get; }
    string Email { get; }
    string ShelterId { get; }
}
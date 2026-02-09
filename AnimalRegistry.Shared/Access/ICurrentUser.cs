namespace AnimalRegistry.Shared.Access;

public interface ICurrentUser
{
    string UserId { get; }
    string Email { get; }
    string ShelterId { get; }
}
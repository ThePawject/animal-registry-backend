namespace AnimalRegistry.Shared.DDD;

public interface IBusinessRule
{
    string Message { get; }
    bool IsBroken();
}

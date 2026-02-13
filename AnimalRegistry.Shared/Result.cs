namespace AnimalRegistry.Shared;

public class Result
{
    protected Result(bool isSuccess, string? error, ResultStatus status)
    {
        IsSuccess = isSuccess;
        Error = error;
        Status = status;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public ResultStatus Status { get; }

    public static Result Success()
    {
        return new Result(true, null, ResultStatus.Ok);
    }

    public static Result Failure(string error)
    {
        return new Result(false, error, ResultStatus.Error);
    }

    public static Result Forbidden(string error)
    {
        return new Result(false, error, ResultStatus.Forbidden);
    }

    public static Result NotFound(string? error = null)
    {
        return new Result(false, error ?? "Not found", ResultStatus.NotFound);
    }

    public static Result ValidationError(string error)
    {
        return new Result(false, error, ResultStatus.ValidationError);
    }
}

public enum ResultStatus
{
    Error = 0,
    Ok = 1,
    NotFound = 2,
    ValidationError = 3,
    Forbidden = 4,
}

public class Result<T> : Result
{
    protected Result(T value) : base(true, null, ResultStatus.Ok)
    {
        Value = value;
    }

    protected Result(string error, ResultStatus status) : base(false, error, status)
    {
    }

    public T? Value { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    public static new Result<T> Failure(string error)
    {
        return new Result<T>(error, ResultStatus.Error);
    }

    public static new Result<T> NotFound(string? error = null)
    {
        return new Result<T>(error ?? "Not found", ResultStatus.NotFound);
    }

    public static new Result<T> ValidationError(string error)
    {
        return new Result<T>(error, ResultStatus.ValidationError);
    }
}
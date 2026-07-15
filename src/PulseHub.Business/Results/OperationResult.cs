namespace PulseHub.Business.Results;

public sealed record OperationResult<TValue, TError>(
    TValue? Value,
    TError Error,
    bool IsSuccess)
{
    public static OperationResult<TValue, TError> Success(TValue value)
        => new(value, default!, true);

    public static OperationResult<TValue, TError> Failure(TError error)
        => new(default, error, false);
}

namespace PulseHub.Business.Results;

public sealed record OperationResult<T>(
    T? Value,
    ReservationError Error)
{
    public bool IsSuccess => Error == ReservationError.None;

    public static OperationResult<T> Success(T value)
        => new(value, ReservationError.None);

    public static OperationResult<T> Failure(ReservationError error)
        => new(default, error);
}

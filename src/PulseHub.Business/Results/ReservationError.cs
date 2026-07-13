namespace PulseHub.Business.Results;

public enum ReservationError
{
    None = 0,
    ReservationNotFound,
    PropertyNotFound,
    ExternalIdAlreadyExists,
    InvalidDateRange,
    InvalidStatusTransition
}

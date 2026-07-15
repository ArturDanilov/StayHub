namespace PulseHub.Business.Results;

public enum ReservationError
{
    None = 0,
    ReservationNotFound,
    PropertyNotFound,
    GuestNotFound,
    ExternalIdAlreadyExists,
    InvalidDateRange,
    InvalidStatusTransition
}

namespace StayHub.Business.Results;

public enum ReservationError
{
    None = 0,
    ReservationNotFound,
    PropertyNotFound,
    GuestNotFound,
    SourceNotFound,
    ExternalIdAlreadyExists,
    InvalidDateRange,
    InvalidStatusTransition
}

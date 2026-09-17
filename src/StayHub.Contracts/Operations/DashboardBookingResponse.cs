using StayHub.Contracts.Reservations;

namespace StayHub.Contracts.Operations;

public sealed record DashboardBookingResponse(
    int Id,
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    ReservationStatusContract Status,
    int PropertyId,
    string PropertyName,
    int GuestId,
    string GuestFirstName,
    string GuestLastName);

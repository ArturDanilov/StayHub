using StayHub.Domain.Models;

namespace StayHub.Business.Models;

public sealed record DashboardBooking(
    int Id,
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    ReservationStatus Status,
    int PropertyId,
    string PropertyName,
    int GuestId,
    string GuestFirstName,
    string GuestLastName);

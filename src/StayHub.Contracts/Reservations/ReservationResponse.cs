using StayHub.Contracts.Guests;

namespace StayHub.Contracts.Reservations;

public sealed record ReservationResponse(
    int Id,
    string ExternalId,
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    ReservationStatusContract Status,
    int PropertyId,
    string PropertyName,
    GuestResponse Guest,
    DateTime CreatedAtUtc);

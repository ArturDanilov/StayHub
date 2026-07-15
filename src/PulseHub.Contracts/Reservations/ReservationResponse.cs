using PulseHub.Contracts.Guests;

namespace PulseHub.Contracts.Reservations;

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

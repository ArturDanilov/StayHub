using StayHub.Contracts.Guests;

namespace StayHub.Contracts.Reservations;

public sealed record ReservationResponse(
    int Id,
    string ExternalId,
    int SourceId,
    string SourceName,
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    ReservationStatusContract Status,
    int PropertyId,
    string PropertyName,
    GuestResponse Guest,
    DateTime CreatedAtUtc);

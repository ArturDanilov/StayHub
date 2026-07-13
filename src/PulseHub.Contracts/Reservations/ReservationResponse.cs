namespace PulseHub.Contracts.Reservations;

public sealed record ReservationResponse(
    int Id,
    string ExternalId,
    string GuestName,
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    ReservationStatusContract Status,
    int PropertyId,
    string PropertyName,
    DateTime CreatedAtUtc);

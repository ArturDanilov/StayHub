namespace StayHub.Contracts.Reservations;

public sealed record CreateReservationRequest(
    string ExternalId,
    int SourceId,
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    int PropertyId,
    int GuestId);

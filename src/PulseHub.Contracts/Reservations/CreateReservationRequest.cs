namespace PulseHub.Contracts.Reservations;

public sealed record CreateReservationRequest(
    string ExternalId,
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    int PropertyId,
    int GuestId);

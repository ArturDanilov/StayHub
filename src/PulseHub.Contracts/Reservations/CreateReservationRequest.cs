namespace PulseHub.Contracts.Reservations;

public sealed record CreateReservationRequest(
    string ExternalId,
    string GuestName,
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    int PropertyId);

namespace PulseHub.Contracts.Reservations;

public sealed record UpdateReservationRequest(
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    int PropertyId,
    int GuestId);

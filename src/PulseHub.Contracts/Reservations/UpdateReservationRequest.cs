namespace PulseHub.Contracts.Reservations;

public sealed record UpdateReservationRequest(
    string GuestName,
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    int PropertyId);

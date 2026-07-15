namespace PulseHub.Contracts.Reservations;

public sealed record UpdateReservationStatusRequest(
    ReservationStatusContract Status);

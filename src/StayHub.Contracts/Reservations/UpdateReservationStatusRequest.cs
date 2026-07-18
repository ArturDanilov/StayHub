namespace StayHub.Contracts.Reservations;

public sealed record UpdateReservationStatusRequest(
    ReservationStatusContract Status);
